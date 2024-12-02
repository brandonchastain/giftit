namespace GiftServer
{
    public class GiftRepository : Repository<Gift>
    {   
        public GiftRepository(TursoClient dbClient)
        : base(dbClient)
        {
        }

        public async Task<Gift[]> GetGiftIdeasForPerson(Guid personId)
        {
            var parameters = new List<(string, object)>{ ("text", personId) };
            var result = await DbClient.ExecuteQueryAsync(
                """
                SELECT g.id, g.name, p.name, g.Link FROM Gift3 g
                JOIN People2 p on p.id = g.personId
                WHERE personId = ?
                """,
                parameters);
            return ParseResults(result);
        }

        public async Task AddNewGift(string name, Guid personId, string link)
        {
            if (!link.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                link = "https://" + link;
            }

            var query = "INSERT INTO Gift3 VALUES (?, ?, ?, ?)";
            var idStr = Guid.NewGuid().ToString();
            var personIdStr = personId.ToString();

            var parameters = new List<(string, object)>(){
                ("text", idStr),
                ("text", name),
                ("text", personIdStr),
                ("text", link) };
            await DbClient.ExecuteQueryAsync(query, parameters); // todo: check status?
        }

        public async Task DeleteGift(Guid id)
        {
            var query =
            """
            DELETE FROM Gift3
            WHERE id = ?
            """;
            var idStr = id.ToString();
            var parameters = new List<(string, object)>(){ ("text", idStr) };
            await DbClient.ExecuteQueryAsync(query, parameters);
        }

        protected override Gift ParseSingleResult(Column[] cols, Row[] row)
        {
            Guid id = Guid.Parse(row[0].Value);
            string name = row[1].Value;
            string personName = row[2].Value;
            string link = row[3].Value;
            
            return new Gift(id, name, personName, link);
        }
    }
}
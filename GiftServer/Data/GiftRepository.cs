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
                SELECT * FROM Gift3
                JOIN People2 on People2.id = Gift3.personId
                WHERE personId = ?
                """,
                parameters);
            return ParseResults(result);
        }

        public async Task AddNewGift(string name, Guid personId)
        {
            var query = "INSERT INTO Gift3 VALUES (?, ?, ?)";
            var idStr = Guid.NewGuid().ToString();
            var personIdStr = personId.ToString();

            var parameters = new List<(string, object)>(){ ("text", idStr), ("text", name), ("text", personIdStr) };
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
            Guid id = Guid.Empty;
            string name = string.Empty;
            string personName = string.Empty;
            id = Guid.Parse(row[0].Value);
            name = row[1].Value;
            personName = row[4].Value;
            
            return new Gift(id, name, personName);
        }
    }
}
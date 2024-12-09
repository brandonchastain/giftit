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
                SELECT g.id, g.name, p.name, g.Link, g.date, g.isPurchased
                FROM Gift3 g
                JOIN People2 p on p.id = g.personId
                WHERE personId = ?
                """,
                parameters);
            return ParseResults(result);
        }

        public async Task<Gift> GetGift(Guid giftId)
        {
            var parameters = new List<(string, object)>{ ("text", giftId) };
            var result = await DbClient.ExecuteQueryAsync(
                """
                SELECT g.id, g.name, p.name, g.Link, g.date, g.isPurchased
                FROM Gift3 g
                JOIN People2 p on p.id = g.personId
                WHERE g.id = ?
                """,
                parameters);
            return ParseResults(result).First();
        }

        public async Task SetIsPurchased(Guid giftId)
        {
            var parameters = new List<(string, object)>{ ("text", giftId) };
            var result = await DbClient.ExecuteQueryAsync(
                """
                UPDATE Gift3
                SET isPurchased = 1
                WHERE id = ?
                """,
                parameters);
        }

        public async Task<Gift> AddNewGift(string name, Guid personId, string link, string date)
        {
            if (!link.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                link = "https://" + link;
            }

            var query = "INSERT INTO Gift3 VALUES (?, ?, ?, ?, ?)";
            var idStr = Guid.NewGuid().ToString();
            var personIdStr = personId.ToString();
            var parameters = new List<(string, object)>(){
                ("text", idStr),
                ("text", name),
                ("text", personIdStr),
                ("text", link),
                ("text", date),
                ("text", "0"),
            };
            await DbClient.ExecuteQueryAsync(query, parameters); // todo: check status?

            return await this.GetGift(Guid.Parse(idStr));
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
            DateTime? date = row[4].Value == null ? null : DateTime.Parse(row[4].Value);
            bool isPurchased = row[5].Value == null ? false : row[5].Value == "1";
            
            return new Gift(id, name, personName, link, date, isPurchased);
        }
    }
}
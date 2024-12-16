namespace GiftServer
{
    public class StoreRepository : Repository<Store>
    {   
        public StoreRepository(TursoClient dbClient)
        : base(dbClient)
        {
        }

        public async Task<Store[]> GetStoresForPerson(Guid personId)
        {
            var parameters = new List<(string, object)>{ ("text", personId) };
            var result = await DbClient.ExecuteQueryAsync(
                """
                SELECT s.id, s.personId, s.name, s.url
                FROM Stores s
                JOIN People2 p on p.id = s.personId
                WHERE personId = ?
                """,
                parameters);
            return ParseResults(result);
        }

        public async Task AddStoreAsync(Guid personId, string name, string link)
        {
            if (!link.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                link = "https://" + link;
            }

            var query = "INSERT INTO Stores VALUES (?, ?, ?, ?)";
            var idStr = Guid.NewGuid().ToString();
            var personIdStr = personId.ToString();
            var parameters = new List<(string, object)>(){
                ("text", idStr),
                ("text", personIdStr),
                ("text", name),
                ("text", link),
            };
            await DbClient.ExecuteQueryAsync(query, parameters); // todo: check status?
        }

        public async Task DeleteStoreAsync(Guid id)
        {
            var query =
            """
            DELETE FROM Stores
            WHERE id = ?
            """;
            var idStr = id.ToString();
            var parameters = new List<(string, object)>(){ ("text", idStr) };
            await DbClient.ExecuteQueryAsync(query, parameters);
        }

        protected override Store ParseSingleResult(Column[] cols, Row[] row)
        {
            Guid id = Guid.Parse(row[0].Value);
            Guid personId = Guid.Parse(row[1].Value);
            string name = row[2].Value;
            string link = row[3].Value;
            
            return new Store(id, personId, name, link);
        }
    }
}
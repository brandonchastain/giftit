namespace GiftServer
{
    public class UserRepository : Repository<User>
    {   
        public UserRepository(TursoClient dbClient)
        : base(dbClient)
        {
        }

        public async Task<User> GetUser(string email)
        {
            var parameters = new List<(string, object)>{ ("text", email) };
            var getResult = async () => await DbClient.ExecuteQueryAsync(
                """
                SELECT u.userId, u.name, u.email, u.Admin
                FROM User u
                WHERE u.email = ?
                """,
                parameters);

            var result = await getResult();
            var parsed = ParseResults(result);
            if (parsed.FirstOrDefault() == null)
            {
                await AddNewUser(email);
                result = await getResult();
                parsed = ParseResults(result);
            }
            
            return parsed.First();
        }

        public async Task AddNewUser(string email)
        {
            var userId = Guid.NewGuid().ToString();
            var name = string.Empty;
            var parameters = new List<(string, object)>{
                ("text", userId),
                ("text", name),
                ("text", email)
            };
            await DbClient.ExecuteQueryAsync(
                """
                INSERT INTO User (userId, name, email)
                VALUES (?, ?, ?)
                """,
                parameters);
        }

        protected override User ParseSingleResult(Column[] cols, Row[] row)
        {
            Guid id = Guid.Parse(row[0].Value);
            string name = row[1].Value;
            string email = row[2].Value;
            bool isAdmin = row[3].Value == "1";
            
            return new User(id, name, email, isAdmin);
        }
    }
}
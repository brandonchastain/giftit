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
            var result = await DbClient.ExecuteQueryAsync(
                """
                SELECT u.userId, u.name, u.email, u.Admin
                FROM User u
                WHERE u.email = ?
                """,
                parameters);
            return ParseResults(result).First();
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
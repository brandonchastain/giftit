namespace GiftServer
{
    public class PersonRepository : Repository<Person>
    {
        public PersonRepository(TursoClient dbClient)
        : base(dbClient)
        {
        }

        public async Task<Person[]> GetMyPeople(Guid userId)
        {
            var parameters = new List<(string, object)>(){("text", userId)};
            var result = await DbClient.ExecuteQueryAsync(
                """
                SELECT p.id, p.name, p.birthday FROM People2 p
                JOIN UserPeople up ON up.personId = p.id
                JOIN User u ON up.userId = u.userId
                WHERE u.userId = ?
                """
                , parameters);

            return ParseResults(result);
        }

        public async Task<Person?> GetPerson(Guid id)
        {
            var parameters = new List<(string, object)>(){("text", id.ToString())};
            var queryResult = await DbClient.ExecuteQueryAsync(
                """
                SELECT p.id, p.name, p.birthday FROM People2 p
                WHERE p.id = ?
                """
                , parameters);

            var result = ParseResults(queryResult);
            if (result.Length == 0)
            {
                return null;
            }

            return result[0];
        }

        public async Task AddNewPerson(string name, DateTime? birthday, Guid userId)
        {
            var query = "INSERT INTO People2 VALUES (?, ?, ?)";
            var idStr = Guid.NewGuid().ToString();
            var birthdayStr = birthday.ToString();

            var parameters = new List<(string, object)>(){ 
                ("text", idStr),
                ("text", name),
                ("text", birthdayStr) };
            await DbClient.ExecuteQueryAsync(query, parameters); // todo: check status?


            query = "INSERT INTO UserPeople VALUES (?, ?)";
            parameters = new List<(string, object)>(){ 
                ("text", userId.ToString()),
                ("text", idStr)};
            await DbClient.ExecuteQueryAsync(query, parameters); // todo: check status?
        }

        public async Task DeletePerson(Guid id)
        {
            var query =
            """
            DELETE FROM UserPeople
            WHERE personId = ?
            """;
            var idStr = id.ToString();
            var parameters = new List<(string, object)>(){ ("text", idStr) };
            await DbClient.ExecuteQueryAsync(query, parameters);

            query =
            """
            DELETE FROM People2
            WHERE id = ?
            """;
            parameters = new List<(string, object)>(){ ("text", idStr) };
            await DbClient.ExecuteQueryAsync(query, parameters);
        }

        protected override Person ParseSingleResult(Column[] cols, Row[] row)
        {
            string name = string.Empty;
            DateTime? birthday = null;
            Guid id = Guid.Empty;

            for (int i = 0; i < cols.Length; i++)
            {
                string field = cols[i].Name;
                if (StrEquals(field, nameof(Person.Name)))
                {
                    name = row[i].Value;
                }
                else if (StrEquals(field, nameof(Person.Id)))
                {
                    var tempId = row[i].Value;
                    id = Guid.Parse(tempId);
                }
                else if (StrEquals(field, nameof(Person.Birthday)))
                {
                    birthday = DateTime.Parse(row[i].Value);
                }
            }
            
            return new Person(id, name, birthday);
        }
    }
}
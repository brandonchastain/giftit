namespace GiftServer
{
    public class DataAccess
    {
        private readonly TursoClient dbClient;
        public DataAccess(TursoClient dbClient)
        {
            this.dbClient = dbClient;
        }

        public async Task<Person[]> GetMyPeople(Guid userId)
        {
            var result = await dbClient.ExecuteQueryAsync<Person[]>("SELECT * FROM People2"); // where id=?", userId);
            return ParseResults(result);
        }

        private Person[] ParseResults(IEnumerable<Result> results)
        {
            var people = new List<Person>();
            foreach (var result in results)
            {
                string name = string.Empty;
                DateTime? birthday = null;
                Guid id = Guid.Empty;
                var cols = result.Cols;
                var rows = result.Rows;
                for (int i = 0; i < cols.Length; i++)
                {
                    for (int j = 0; j < rows.Length; j++)
                    {
                        string field = cols[i].Name;
                        if (StrEquals(field, nameof(Person.Name)))
                        {
                            name = rows[j][i].Value;
                        }
                        else if (StrEquals(field, nameof(Person.Id)))
                        {
                            var tempId = rows[j][i].Value;
                            id = Guid.Parse(tempId);
                        }
                        else if (StrEquals(field, nameof(Person.Birthday)))
                        {
                            birthday = DateTime.Parse(rows[j][i].Value);
                        }
                    }
                }

                people.Add(new Person(id, name, birthday));
            }

            return people.ToArray();
        }

        private bool StrEquals(string a, string b)
        {
            if (a == b)
            {
                return true;
            }

            if (a == null || b == null)
            {
                return false;
            }

            return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
        }
    }
}
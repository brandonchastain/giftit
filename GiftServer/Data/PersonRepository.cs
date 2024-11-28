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
            var result = await DbClient.ExecuteQueryAsync("SELECT * FROM People2"); // where id=?", userId);
            return ParseResults(result);
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
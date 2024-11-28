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
            var result = await DbClient.ExecuteQueryAsync("SELECT * FROM Gift3 WHERE personId = ?", parameters);
            return ParseResults(result);
        }

        protected override Gift ParseSingleResult(Column[] cols, Row[] row)
        {
            Guid id = Guid.Empty;
            string name = string.Empty;
            Guid personId = Guid.Empty;

            for (int i = 0; i < cols.Length; i++)
            {
                string field = cols[i].Name;
                if (StrEquals(field, nameof(Gift.Name)))
                {
                    name = row[i].Value;
                }
                else if (StrEquals(field, nameof(Gift.Id)))
                {
                    id = Guid.Parse(row[i].Value);
                }
                else if (StrEquals(field, nameof(Gift.PersonId)))
                {
                    personId = Guid.Parse(row[i].Value);
                }
            }
            
            return new Gift(id, name, personId);
        }
    }
}
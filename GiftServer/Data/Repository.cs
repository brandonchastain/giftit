namespace GiftServer
{
    public abstract class Repository<T>
    {
        public Repository(TursoClient dbClient)
        {
            this.DbClient = dbClient;
        }

        protected TursoClient DbClient { get; private set; }

        public T[] ParseResults(IEnumerable<Result> results)
        {
            var list = new List<T>();
            foreach (var result in results)
            {
                var cols = result.Cols;
                var rows = result.Rows;
                for (int j = 0; j < rows.Length; j++)
                {
                    list.Add(ParseSingleResult(cols, rows[j]));
                }
            }

            return list.ToArray();
        }

        protected abstract T ParseSingleResult(Column[] cols, Row[] row);

        protected bool StrEquals(string a, string b)
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
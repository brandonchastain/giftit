namespace GiftServer
{
    using System.Net.Http.Headers;
    using System.Text.Json;

    public class TursoClient
    {
        private readonly HttpClient httpClient;
        private readonly string dbUrl;
        private readonly string authToken;

        public TursoClient(string dbUrl, string authToken)
        {
            httpClient = new HttpClient(); // TODO: use httpclientfactory
            this.dbUrl = dbUrl;
            this.authToken = authToken;
        }

        public async Task<IEnumerable<Result>> ExecuteQueryAsync<T>(string sql, List<object> parameters = null)
        {
            var request = new
            {
                requests = new[]
                {
                    new
                    {
                        type = "execute",
                        stmt = new
                        {
                            sql = sql,
                            args = parameters ?? new List<object>()
                        }
                    }
                }
            };
            var jsonString = JsonSerializer.Serialize(request);
            Console.WriteLine(jsonString);
            var content = new StringContent(
                jsonString,
                System.Text.Encoding.UTF8,
                "application/json");

            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", authToken);

            var response = await httpClient.PostAsync($"{dbUrl}/v2/pipeline", content);
            Console.WriteLine(await response.Content.ReadAsStringAsync());
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var outerResponse = JsonSerializer.Deserialize<OuterResponse>(jsonResponse, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
            });

            var res = new List<Result>();
            foreach (var outerResult in outerResponse.Results)
            {
                res.Add(outerResult.Response.Result);
            }
            return res;
        }
    }

    public record OuterResponse(List<Results> Results);
    public record Results(string Type, InnerResponse Response);
    public record InnerResponse(string Type, Result Result);
    public record Result(Column[] Cols, Row[][] Rows);
    public record Column(string Decltype, string Name);
    public record Row(string Type, string Value);
}
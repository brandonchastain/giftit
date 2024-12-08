namespace GiftServer
{
    using System.Diagnostics;
    using System.Net.Http.Headers;
    using System.Text.Json;

    public class TursoClient
    {
        private readonly HttpClient httpClient;
        private readonly string dbUrl;

        public TursoClient(string dbUrl, string authToken)
        {
            this.dbUrl = dbUrl;
            httpClient = new HttpClient(); // TODO: use httpclientfactory
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", authToken);
        }

        public async Task<IEnumerable<Result>> ExecuteQueryAsync(string sql, List<(string, object)> parameters = null)
        {
            var request = CreateDbRequest(sql, parameters);
            string jsonString = JsonSerializer.Serialize(request);
            Trace.TraceInformation(jsonString);

            var content = new StringContent(
                jsonString,
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await httpClient.PostAsync($"{dbUrl}/v2/pipeline", content);
            string jsonResponse = await response.Content.ReadAsStringAsync();
            Trace.TraceInformation(jsonResponse);

            response.EnsureSuccessStatusCode();
            var outerResponse = JsonSerializer.Deserialize<OuterResponse>(jsonResponse, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
            });

            var res = new List<Result>();
            if (outerResponse != null && outerResponse.Results != null)
            {    
                foreach (var outerResult in outerResponse.Results)
                {
                    res.Add(outerResult.Response.Result);
                }
            }
            return res;
        }

        private object CreateDbRequest(string sql, List<(string, object)> parameters)
        {
            return new
            {
                requests = new[]
                {
                    new
                    {
                        type = "execute",
                        stmt = new
                        {
                            sql = sql,
                            args = (parameters ?? new List<(string, object)>()).Select(p =>
                                new {
                                    // "integer", "text"
                                    type = p.Item1,
                                    value = p.Item2
                                }
                            )
                        }
                    }
                }
            };
        }
    }

    // Records used for deserializing JSON response
    public record OuterResponse(List<Results> Results);
    public record Results(string Type, InnerResponse Response);
    public record InnerResponse(string Type, Result Result);
    public record Result(Column[] Cols, Row[][] Rows);
    public record Column(string Decltype, string Name);
    public record Row(string Type, string Value);
}
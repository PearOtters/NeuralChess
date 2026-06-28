using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NeuralChess.Engine
{
    public static class SyzygyAPI
    {
        private static readonly HttpClient client = new HttpClient();
        private const string BaseUrl = "https://tablebase.lichess.ovh/standard?fen=";

        public static async Task<SyzygyResponse?> ProbePositionAsync(string fen)
        {
            try
            {
                string requestUrl = BaseUrl + Uri.EscapeDataString(fen);
                HttpResponseMessage response = await client.GetAsync(requestUrl);
                
                if (!response.IsSuccessStatusCode)
                {
                    return null; 
                }

                string jsonResponse = await response.Content.ReadAsStringAsync();
                
                return JsonSerializer.Deserialize<SyzygyResponse>(jsonResponse, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Syzygy API error: {ex.Message}");
                return null;
            }
        }
    }

    public class SyzygyResponse
    {
        [JsonPropertyName("wdl")]
        public int Wdl { get; set; }

        [JsonPropertyName("dtz")]
        public int Dtz { get; set; }
        
        [JsonPropertyName("moves")]
        public required SyzygyMove[] Moves { get; set; }
    }

    public class SyzygyMove
    {
        [JsonPropertyName("uci")]
        public required string Uci { get; set; }

        [JsonPropertyName("wdl")]
        public int Wdl { get; set; }

        [JsonPropertyName("dtz")]
        public int Dtz { get; set; }
    }
}
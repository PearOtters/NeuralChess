using System.Text.Json;
using System.Text.Json.Serialization;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace NeuralChess.Engine
{
    public class UnexpectedCategoryException(string? message) : Exception(message) { }
    public static class SyzygyAPI
    {
        private const string BaseUrl = "https://tablebase.lichess.ovh/standard?fen=";

        public static async Task<SyzygyResponse?> ProbePositionAsync(string fen)
        {
            try
            {
                string formattedFen = fen.Replace(" ", "_");
                string requestUrl = BaseUrl + formattedFen;

                bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
                ProcessStartInfo psi = new()
                {
                    FileName = isWindows ? "curl.exe" : "curl",
                    Arguments = $"-s -A \"NeuralChess/1.0\" \"{requestUrl}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                psi.EnvironmentVariables.Remove("LD_LIBRARY_PATH");

                using Process? process = Process.Start(psi);
                if (process == null) return null;

                string jsonResponse = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();

                if (string.IsNullOrWhiteSpace(jsonResponse) || process.ExitCode != 0)
                {
                    return null;
                }

                return JsonSerializer.Deserialize<SyzygyResponse>(jsonResponse, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });
            }
            catch (Exception ex)
            {
                File.AppendAllText("log.txt", $"Syzygy API error: {ex.Message}\n");
                File.AppendAllText("log.txt", $"Syzygy API error: {ex.InnerException}\n");
                return null;
            }
        }
    }

    public class SyzygyResponse
    {
        [JsonPropertyName("category")]
        public required string Category { get; set; }

        [JsonPropertyName("dtz")]
        public int Dtz { get; set; }
        
        [JsonPropertyName("moves")]
        public required SyzygyMove[] Moves { get; set; }

        public int Wdl => Category switch
        {
            "win" => 2,
            "cursed-win" => 1,
            "draw" => 0,
            "blessed-loss" => -1,
            "loss" => -2,
            _ => throw new UnexpectedCategoryException($"Unexpected category received from API call got {Category}")
        };

        public SyzygyMove? GetBestMove()
        {
            if (Moves.Length == 0) return null;
            SyzygyMove bestMove = Moves[0];

            int rootWdl = Wdl; 

            if (rootWdl <= 0)
            {
                int biggestWasteOfTime = -1;
                
                for (int i = 0; i < Moves.Length; i++)
                {
                    SyzygyMove move = Moves[i];
                    
                    if (move.Wdl == rootWdl)
                    {
                        int absoluteDtz = Math.Abs(move.Dtz);
                        
                        if (absoluteDtz > biggestWasteOfTime)
                        {
                            biggestWasteOfTime = absoluteDtz;
                            bestMove = move;
                        }
                    }
                }
            }
            else
            {
                int smallestWasteOfTime = int.MaxValue;
                
                for (int i = 0; i < Moves.Length; i++)
                {
                    SyzygyMove move = Moves[i];
                    
                    if (move.Wdl == rootWdl)
                    {
                        int absoluteDtz = Math.Abs(move.Dtz);
                        
                        if (absoluteDtz < smallestWasteOfTime)
                        {
                            smallestWasteOfTime = absoluteDtz;
                            bestMove = move;
                        }
                    }
                }
            }
            return bestMove;
        }
    }

    public class SyzygyMove
    {
        [JsonPropertyName("uci")]
        public required string Uci { get; set; }

        [JsonPropertyName("category")]
        public required string Category { get; set; }

        [JsonPropertyName("dtz")]
        public int Dtz { get; set; }

        public int Wdl => Category switch
        {
            "win" => -2,
            "cursed-win" => -1,
            "draw" => 0,
            "blessed-loss" => 1,
            "loss" => 2,
            _ => throw new UnexpectedCategoryException($"Unexpected category received from API call got {Category}")

        };
    }
}
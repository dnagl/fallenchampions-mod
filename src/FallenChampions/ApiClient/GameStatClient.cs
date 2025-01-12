using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using FallenChampions.Configuration;
using FallenChampions.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RestSharp;

namespace FallenChampions.ApiClient
{
    public class GameStatClient
    {
        private ModConfiguration _configuration;
        private string _statBackupDirectory;
        private JsonSerializerSettings _jsonSerializerSettings;

        public GameStatClient()
        {
            Initialize();
        }

        public void SendStats(GameStats gameStats)
        {
            var response = ExecuteRequest(gameStats);

            if (!response.IsSuccessful)
            {
                BackupStats(gameStats);
                return;
            }
            
            var statesToBePublished = Directory.GetFiles(_statBackupDirectory, "*.json", SearchOption.TopDirectoryOnly).ToList();
            foreach (var stat in statesToBePublished)
            {
                var gameStat = JsonConvert.DeserializeObject<GameStats>(File.ReadAllText(stat));
                response = ExecuteRequest(gameStat);
                if (response.IsSuccessful)
                    File.Delete(stat);
            }
        }

        private RestResponse ExecuteRequest(GameStats gameStats)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            
            var client = new RestClient(_configuration.ApiUrl);
            var request = new RestRequest("/api/v1/client/gamestats", Method.Post);
            request.AddJsonBody(gameStats);
            request.AddHeader("Authorization", $"Bearer {_configuration.ApiKey}");
            request.AddStringBody(JsonConvert.SerializeObject(gameStats, _jsonSerializerSettings), ContentType.Json);
            return client.Execute(request);
        }
        
        private void BackupStats(GameStats gameStats)
        {
            var backupPath = Path.Combine(_statBackupDirectory, $"{gameStats.GameId}.json");
            File.WriteAllText(backupPath, JsonConvert.SerializeObject(gameStats, _jsonSerializerSettings));
        }

        private void Initialize()
        {
            _configuration = ConfigurationManager.Instance.Configuration;
            _jsonSerializerSettings = new JsonSerializerSettings
            {
                Converters = { new StringEnumConverter() }
            };
            
            var assembly = Assembly.GetAssembly(typeof(FallenChampionsModule));
            _statBackupDirectory = Path.Combine(Path.GetDirectoryName(assembly.Location), "stats");
            if (!Directory.Exists(_statBackupDirectory))
                Directory.CreateDirectory(_statBackupDirectory);
        }
    }
}
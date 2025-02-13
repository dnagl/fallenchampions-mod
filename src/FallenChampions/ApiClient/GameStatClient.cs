﻿using System.IO;
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
                FortRise.Logger.Error($"Failed to send game stats to API. Backup stats for later publishing. Status: {response.ResponseStatus} Message: {response.ErrorMessage} Exception: {response.ErrorException}");
                BackupStats(gameStats);
                return;
            }
            else
                FortRise.Logger.Info($"Game stats sent to API. {response.Content}");
            
            FortRise.Logger.Verbose($"Searching for past stats to be published.");
            var statesToBePublished = Directory.GetFiles(_statBackupDirectory, "*.json", SearchOption.TopDirectoryOnly).ToList();
            FortRise.Logger.Verbose($"Found {statesToBePublished.Count} stats to be published.");
            foreach (var stat in statesToBePublished)
            {
                FortRise.Logger.Verbose($"Publishing {stat}");
                var gameStat = JsonConvert.DeserializeObject<GameStats>(File.ReadAllText(stat));
                response = ExecuteRequest(gameStat);
                if (response.IsSuccessful)
                    File.Delete(stat);
                else
                    FortRise.Logger.Error($"Failed to send game stats to API. Backup stats for later publishing. Status: {response.ResponseStatus} Message: {response.ErrorMessage} Exception: {response.ErrorException}");
            }
        }

        private RestResponse ExecuteRequest(GameStats gameStats)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            
            FortRise.Logger.Verbose($"Sending game stats to API. MatchId: {gameStats.GameId}");
            var client = new RestClient(_configuration.ApiUrl);
            var request = new RestRequest("/api/v1/client/gamestats", Method.Post);
            request.AddJsonBody(gameStats);
            request.AddHeader("Authorization", $"Bearer {_configuration.ApiKey}");
            request.AddStringBody(JsonConvert.SerializeObject(gameStats, _jsonSerializerSettings), ContentType.Json);
            return client.Execute(request);
        }
        
        private void BackupStats(GameStats gameStats)
        {
            FortRise.Logger.Info($"Backup game stats for later publishing. MatchId: {gameStats.GameId}");
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
            
            var callingAssembly = Assembly.GetEntryAssembly();
            _statBackupDirectory = Path.Combine(Path.GetDirectoryName(callingAssembly.Location), "Mods", "FallenChampions", "stats");
            if (!Directory.Exists(_statBackupDirectory))
                Directory.CreateDirectory(_statBackupDirectory);
        }
    } 
}
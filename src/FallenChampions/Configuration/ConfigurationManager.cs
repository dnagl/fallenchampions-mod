using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace FallenChampions.Configuration
{
    public class ConfigurationManager
    {
        private static ConfigurationManager _instance;
        private static readonly object Lock = new object();

        public ConfigurationManager()
        {
            Initialize();
        }

        public ModConfiguration Configuration { get; private set; }

        public static ConfigurationManager Instance
        {
            get
            {
                if (_instance != null) return _instance;
                lock (Lock)
                {
                    if (_instance == null)
                        _instance = new ConfigurationManager();
                }

                return _instance;
            }
        }

        private void Initialize()
        {
            var assembly = Assembly.GetAssembly(typeof(FallenChampionsModule));
            var configDirectory = Path.Combine(Path.GetDirectoryName(assembly.Location), "config");
            var configFile = Path.Combine(configDirectory, "config.json");

            if (!File.Exists(configFile))
            {
                // Create default config file
                var defaultConfig = new ModConfiguration
                {
                    ApiUrl = "https://api.fallenchampions.com",
                    ApiKey = ""
                };

                if (!Directory.Exists(configDirectory))
                    Directory.CreateDirectory(configDirectory);

                File.WriteAllText(configFile, JsonConvert.SerializeObject(defaultConfig));
            }

            Configuration = JsonConvert.DeserializeObject<ModConfiguration>(File.ReadAllText(configFile));
        }
    }
}
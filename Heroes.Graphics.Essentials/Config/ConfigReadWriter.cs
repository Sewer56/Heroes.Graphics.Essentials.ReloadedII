using Heroes.Graphics.Essentials.Shared.Serialization;

namespace Heroes.Graphics.Essentials.Config
{
    public class ConfigReadWriter : JsonSerializable<Config>
    {
        /* Instances */
        private string _configFolder;

        public ConfigReadWriter(string configFolder)
        {
            _configFolder = configFolder;
        }

        public string GetFilePath()         => $"{_configFolder}\\Graphics.json";
        public Config FromJson()            => FromPath(GetFilePath());
        public void ToJson(Config config)   => ToPath(config, GetFilePath());
    }
}

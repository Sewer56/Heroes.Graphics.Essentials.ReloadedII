using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Heroes.Graphics.Essentials.Shared
{
    public abstract class JsonSerializable<TType> where TType : new()
    {
        private static JsonSerializerOptions _options = new JsonSerializerOptions()
        {
            Converters = { new JsonStringEnumConverter() },
            WriteIndented = true
        };

        public static TType FromPath(string filePath)
        {
            if (File.Exists(filePath))
            {
                string jsonFile = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<TType>(jsonFile, _options);
            }
            else
            {
                var newFile = new TType();
                ToPath(newFile, filePath);
                return newFile;
            }
        }

        public static void ToPath(TType config, string filePath)
        {
            string fullPath = Path.GetFullPath(filePath);
            string directoryOfPath = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(directoryOfPath))
                Directory.CreateDirectory(directoryOfPath);

            string jsonFile = JsonSerializer.Serialize(config, _options);
            File.WriteAllText(fullPath, jsonFile);
        }
    }
}

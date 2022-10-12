using Reloaded.Mod.Interfaces;

namespace Heroes.Graphics.Essentials.Configuration
{
    public class Configurator : IConfiguratorV2
    {
        private const string ConfigFileName = "Graphics.json";

        /// <summary>
        /// The folder where the modification files are stored.
        /// </summary>
        public string ModFolder { get; private set; }

        /// <summary>
        /// Full path to the config folder.
        /// </summary>
        public string ConfigFolder { get; private set; }

        /// <summary>
        /// Returns a list of configurations.
        /// </summary> 
        public IUpdatableConfigurable[] Configurations => _configurations ?? MakeConfigurations();
        private IUpdatableConfigurable[] _configurations;

        private IUpdatableConfigurable[] MakeConfigurations()
        {
            _configurations = new IUpdatableConfigurable[]
            {
                // Add more configurations here if needed.
                Configurable<Config.Config>.FromFile(Path.Combine(ConfigFolder, ConfigFileName), "Graphics Essentials Config")
            };

            // Add self-updating to configurations.
            for (int x = 0; x < Configurations.Length; x++)
            {
                var xCopy = x;
                Configurations[x].ConfigurationUpdated += configurable =>
                {
                    Configurations[xCopy] = configurable;
                };
            }

            return _configurations;
        }

        public Configurator() { }
        public Configurator(string configDirectory) : this()
        {
            ConfigFolder = configDirectory;
        }

        /* Configurator V2 */

        /// <summary>
        /// Migrates from the old config location to the newer config location.
        /// </summary>
        /// <param name="oldDirectory">Old directory containing the mod configs.</param>
        /// <param name="newDirectory">New directory pointing to user config folder.</param>
        public void Migrate(string oldDirectory, string newDirectory)
        {
            TryMoveFile(ConfigFileName);

            void TryMoveFile(string fileName)
            {
                try { File.Move(Path.Combine(oldDirectory, fileName), Path.Combine(newDirectory, fileName), true); }
                catch (Exception) { /* Ignored */ }
            }
        }

        /* Configurator */

        /// <summary>
        /// Gets an individual user configuration.
        /// </summary>
        public TType GetConfiguration<TType>(int index) => (TType)Configurations[index];

        /* IConfigurator. */

        /// <summary>
        /// Sets the config directory for the Configurator.
        /// </summary>
        public void SetConfigDirectory(string configDirectory) => ConfigFolder = configDirectory;

        /// <summary>
        /// Returns a list of user configurations.
        /// </summary>
        public IConfigurable[] GetConfigurations() => Configurations;

        /// <summary>
        /// Allows for custom launcher/configurator implementation.
        /// If you have your own configuration program/code, run that code here and return true, else return false.
        /// </summary>
        public bool TryRunCustomConfiguration() => false;

        /// <summary>
        /// Sets the mod directory for the Configurator.
        /// </summary>
        public void SetModDirectory(string modDirectory) { ModFolder = modDirectory; }
    }
}

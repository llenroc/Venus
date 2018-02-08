using System;
using System.IO;
using Venus.Infrastructure.Events;
using Venus.Infrastructure.Events.EventClasses;
using Venus.Model;

namespace Venus.Infrastructure
{
    public class ConfigurationManager
    {
        private const string ConfigFilename = "Data.dat";
        private readonly string _configFilePath;

        private readonly IEventPublisher _publisher;
        private readonly IFileService _fileService;
        public Configuration CurrentConfiguration { get; private set; }

        public ConfigurationManager(IEventPublisher publisher, IFileService fileService)
        {
            _publisher = publisher;
            _fileService = fileService;

            _configFilePath = $"{Environment.CurrentDirectory}\\{ConfigFilename}";

            _publisher.Publish(new UserSettingsUpdatedEvent(this, CurrentConfiguration));
            _publisher.GetEvent<UserSettingsUpdatedEvent>().Subscribe(OnSettingsUpdate);
        }

        public void Init()
        {
            CurrentConfiguration = new Configuration
            {
                UpdateInterval = 30
            };

            var configArray = _fileService.ReadData(_configFilePath);
            var config = Configuration.Deserialize(configArray);

            if (config == null) {return;}

            CurrentConfiguration = config;
        }

        private void OnSettingsUpdate(UserSettingsUpdatedEvent e)
        {
            if(e.Sender == this) { return; }
            if(e.NewConfiguration == null) { return; }

            CurrentConfiguration = e.NewConfiguration;

            _fileService.WriteData(ConfigFilename, CurrentConfiguration.Serialize());
        }
    }
}
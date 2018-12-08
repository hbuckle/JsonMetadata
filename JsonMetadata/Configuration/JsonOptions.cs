using MediaBrowser.Common.Configuration;
using MediaBrowser.Model.Configuration;
using System.Collections.Generic;

namespace JsonMetadata.Configuration
{
    public class ConfigurationFactory : IConfigurationFactory
    {
        public IEnumerable<ConfigurationStore> GetConfigurations()
        {
            return new[]
            {
                new ConfigurationStore
                {
                     ConfigurationType = typeof(XbmcMetadataOptions),
                     Key = "xbmcmetadata"
                }
            };
        }
    }

    public static class ConfigurationExtension
    {
        public static XbmcMetadataOptions GetJsonConfiguration(this IConfigurationManager manager)
        {
            return manager.GetConfiguration<XbmcMetadataOptions>("xbmcmetadata");
        }
    }
}

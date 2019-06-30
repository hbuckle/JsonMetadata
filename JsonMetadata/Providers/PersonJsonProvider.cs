using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Logging;
using JsonMetadata.Parsers;
using System.Threading;
using System.IO;
using MediaBrowser.Model.IO;

namespace JsonMetadata.Providers {
  public class PersonJsonProvider : BaseJsonProvider<Person> {
    private readonly ILogger _logger;
    private readonly IServerConfigurationManager _config;
    private readonly IProviderManager _providerManager;

    public PersonJsonProvider(IFileSystem fileSystem, ILogger logger, IServerConfigurationManager config, IProviderManager providerManager, ILibraryManager libraryManager)
        : base(fileSystem, libraryManager) {
      _logger = logger;
      _config = config;
      _providerManager = providerManager;
    }

    protected override void Fetch(MetadataResult<Person> result, string path, CancellationToken cancellationToken) {
      var tmpItem = new MetadataResult<Person>
      {
        Item = result.Item
      };
      new PersonJsonParser(_logger, _config, _providerManager, FileSystem, LibraryManager).Fetch(tmpItem, path, cancellationToken);

      result.Item = (Person)tmpItem.Item;

      if (tmpItem.UserDataList != null) {
        result.UserDataList = tmpItem.UserDataList;
      }
    }

    protected override FileSystemMetadata GetJsonFile(ItemInfo info, IDirectoryService directoryService) {
      return directoryService.GetFile(Path.Combine(info.Path, "person.json"));
    }
  }
}
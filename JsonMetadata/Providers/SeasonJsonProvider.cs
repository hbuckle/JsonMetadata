using System.IO;
using System.Linq;
using System.Threading;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Controller.IO;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Logging;
using JsonMetadata.Parsers;
using JsonMetadata.Savers;

namespace JsonMetadata.Providers
{
  public class SeasonJsonProvider : BaseJsonProvider<Season>
  {
    private readonly ILogger _logger;
    private readonly IConfigurationManager _config;
    private readonly IProviderManager _providerManager;

    public SeasonJsonProvider(IFileSystem fileSystem, ILogger logger, IConfigurationManager config, IProviderManager providerManager, ILibraryManager libraryManager)
        : base(fileSystem, libraryManager)
    {
      _logger = logger;
      _config = config;
      _providerManager = providerManager;
    }

    protected override void Fetch(MetadataResult<Season> result, string path, CancellationToken cancellationToken)
    {
      var tmpItem = new MetadataResult<Season>
      {
        Item = result.Item
      };
      new SeasonJsonParser(_logger, _config, _providerManager, FileSystem, LibraryManager).Fetch(tmpItem, path, cancellationToken);

      result.Item = (Season)tmpItem.Item;
      result.People = tmpItem.People;

      if (tmpItem.UserDataList != null)
      {
        result.UserDataList = tmpItem.UserDataList;
      }
    }

    protected override FileSystemMetadata GetJsonFile(ItemInfo info, IDirectoryService directoryService)
    {
      return directoryService.GetFile(Path.Combine(info.Path, "season.json"));
    }
  }
}
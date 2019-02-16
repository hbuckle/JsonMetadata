using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Logging;
using JsonMetadata.Parsers;
using JsonMetadata.Savers;
using System.Linq;
using System.Threading;
using MediaBrowser.Controller.IO;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Xml;

namespace JsonMetadata.Providers
{
  public class MovieJsonProvider : BaseJsonProvider<Movie>
  {
    private readonly ILogger _logger;
    private readonly IConfigurationManager _config;
    private readonly IProviderManager _providerManager;

    public MovieJsonProvider(IFileSystem fileSystem, ILogger logger, IConfigurationManager config, IProviderManager providerManager, ILibraryManager libraryManager)
        : base(fileSystem, libraryManager)
    {
      _logger = logger;
      _config = config;
      _providerManager = providerManager;
    }

    protected override void Fetch(MetadataResult<Movie> result, string path, CancellationToken cancellationToken)
    {
      var tmpItem = new MetadataResult<Movie>
      {
        Item = result.Item
      };
      new MovieJsonParser(_logger, _config, _providerManager, FileSystem, LibraryManager).Fetch(tmpItem, path, cancellationToken);

      result.Item = (Movie)tmpItem.Item;
      result.People = tmpItem.People;

      if (tmpItem.UserDataList != null)
      {
        result.UserDataList = tmpItem.UserDataList;
      }
    }

    protected override FileSystemMetadata GetJsonFile(ItemInfo info, IDirectoryService directoryService)
    {
      return MovieJsonSaver.GetMovieSavePaths(info, FileSystem)
          .Select(directoryService.GetFile)
          .FirstOrDefault(i => i != null);
    }
  }
}
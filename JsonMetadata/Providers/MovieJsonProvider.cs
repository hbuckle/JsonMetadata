using System.IO;
using System.Threading;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Logging;
using JsonMetadata.Parsers;

namespace JsonMetadata.Providers {
  public class MovieJsonProvider : BaseJsonProvider<Movie> {
    private readonly ILogger _logger;
    private readonly IConfigurationManager _config;
    private readonly IProviderManager _providerManager;

    public MovieJsonProvider(IFileSystem fileSystem, ILogger logger, IConfigurationManager config, IProviderManager providerManager, ILibraryManager libraryManager)
        : base(fileSystem, libraryManager) {
      _logger = logger;
      _config = config;
      _providerManager = providerManager;
    }

    protected override void Fetch(MetadataResult<Movie> result, string path, CancellationToken cancellationToken) {
      new MovieJsonParser(
        _logger, _config, _providerManager, FileSystem, LibraryManager).Fetch(
          result, path, cancellationToken);
    }

    protected override FileSystemMetadata GetJsonFile(ItemInfo info, IDirectoryService directoryService) {
      return directoryService.GetFile(
        Path.ChangeExtension(info.Path, ".json")
      );
    }
  }
}

using System.IO;
using System.Threading;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Logging;
using JsonMetadata.Parsers;

namespace JsonMetadata.Providers {
  public class SeriesJsonProvider : BaseJsonProvider<Series> {
    private readonly ILogger _logger;
    private readonly IConfigurationManager _config;
    private readonly IProviderManager _providerManager;

    public SeriesJsonProvider(IFileSystem fileSystem, ILogger logger, IConfigurationManager config, IProviderManager providerManager, ILibraryManager libraryManager)
        : base(fileSystem, libraryManager) {
      _logger = logger;
      _config = config;
      _providerManager = providerManager;
    }

    protected override void Fetch(MetadataResult<Series> result, string path, CancellationToken cancellationToken) {
      new SeriesJsonParser(
        _logger, _config, _providerManager, FileSystem, LibraryManager).Fetch(
          result, path, cancellationToken);
    }

    protected override FileSystemMetadata GetJsonFile(ItemInfo info, IDirectoryService directoryService) {
      return directoryService.GetFile(
        Path.Combine(info.Path, "tvshow.json")
      );
    }

    public override string Name => "SeriesJsonProvider";
  }
}

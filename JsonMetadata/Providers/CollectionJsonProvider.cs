using System.IO;
using System.Threading;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Logging;
using JsonMetadata.Parsers;

namespace JsonMetadata.Providers {
  public class CollectionJsonProvider : BaseJsonProvider<BoxSet> {
    private readonly ILogger _logger;
    private readonly IConfigurationManager _config;
    private readonly IProviderManager _providerManager;

    public CollectionJsonProvider(IFileSystem fileSystem, ILogger logger, IConfigurationManager config, IProviderManager providerManager, ILibraryManager libraryManager)
        : base(fileSystem, libraryManager) {
      _logger = logger;
      _config = config;
      _providerManager = providerManager;
    }

    protected override void Fetch(MetadataResult<BoxSet> result, string path, CancellationToken cancellationToken) {
      new CollectionJsonParser(
        _logger, _config, _providerManager, FileSystem, LibraryManager).Fetch(
          result, path, cancellationToken);
    }

    protected override FileSystemMetadata GetJsonFile(ItemInfo info, IDirectoryService directoryService) {
      return directoryService.GetFile(
        Path.Combine(info.GetInternalMetadataPath(), "collection.json")
      );
    }
  }
}

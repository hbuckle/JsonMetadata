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
  public class ExtraJsonProvider : BaseJsonProvider<Video> {
    private readonly ILogger _logger;
    private readonly IConfigurationManager _config;
    private readonly IProviderManager _providerManager;

    public ExtraJsonProvider(IFileSystem fileSystem, ILogger logger, IConfigurationManager config, IProviderManager providerManager, ILibraryManager libraryManager)
        : base(fileSystem, libraryManager) {
      _logger = logger;
      _config = config;
      _providerManager = providerManager;
    }

    protected override void Fetch(MetadataResult<Video> result, string path, CancellationToken cancellationToken) {
      new VideoJsonParser(
        _logger, _config, _providerManager, FileSystem, LibraryManager).Fetch(
          result, path, cancellationToken);
    }

    protected override FileSystemMetadata GetJsonFile(ItemInfo info, IDirectoryService directoryService) {
      var directory = new DirectoryInfo(info.ContainingFolderPath).Name;
      var extrasSubFolders = new System.Collections.Generic.List<string>() {
        "extras", "specials", "shorts", "scenes", "featurettes",
        "behind the scenes", "deleted scenes", "interviews",
      };
      if (extrasSubFolders.Contains(directory.ToLower())) {
        return directoryService.GetFile(
          Path.ChangeExtension(info.Path, ".json")
        );
      }
      else {
        return new FileSystemMetadata { Exists = false };
      }
    }

    public override string Name => "ExtraJsonProvider";
  }
}

using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
      _logger.Log(LogSeverity.Info, $"JsonMetadata: GetJsonFile {info.Name} {info.Path}");
      string extension = Path.GetExtension(info.Path);
      string name = Path.GetFileNameWithoutExtension(info.Path);
      string file = Path.ChangeExtension(info.Path, ".json");
      if (Regex.IsMatch(name, $"^[\\w\\s]+Part [1-9]$")) {
        string trimmedName = name.Substring(0, name.Length - " Part x".Length);
        if (Directory.EnumerateFiles(info.ContainingFolderPath, $"{trimmedName} Part *{extension}").Count() > 1) {
          file = Path.Combine(info.ContainingFolderPath, $"{trimmedName}.json");
        }
      }
      _logger.Log(LogSeverity.Info, $"JsonMetadata: GetJsonFile {file}");
      return directoryService.GetFile(file);
    }

    public override string Name => "MovieJsonProvider";
  }
}

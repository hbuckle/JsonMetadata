using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Extensions;
using MediaBrowser.Model.Logging;
using JsonMetadata.Configuration;
using JsonMetadata.Savers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Model.IO;

namespace JsonMetadata.Parsers
{
  public class BaseJsonParser<T>
      where T : BaseItem
  {
    /// <summary>
    /// The logger
    /// </summary>
    protected ILogger Logger { get; private set; }
    protected IFileSystem FileSystem { get; private set; }
    protected IProviderManager ProviderManager { get; private set; }

    private readonly IConfigurationManager _config;
    private Dictionary<string, string> _validProviderIds;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseJsonParser{T}" /> class.
    /// </summary>
    public BaseJsonParser(ILogger logger, IConfigurationManager config, IProviderManager providerManager, IFileSystem fileSystem)
    {
      Logger = logger;
      _config = config;
      ProviderManager = providerManager;
      FileSystem = fileSystem;
    }

    /// <summary>
    /// Fetches metadata for an item from one json file
    /// </summary>
    /// <param name="metadataResult">The item.</param>
    /// <param name="metadataFile">The metadata file.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <exception cref="System.ArgumentNullException">
    /// </exception>
    public void Fetch(MetadataResult<T> metadataResult, string metadataFile, CancellationToken cancellationToken)
    {
      if (metadataResult == null)
      {
        throw new ArgumentNullException();
      }

      if (string.IsNullOrEmpty(metadataFile))
      {
        throw new ArgumentNullException();
      }

      _validProviderIds = _validProviderIds = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

      var idInfos = ProviderManager.GetExternalIdInfos(metadataResult.Item);

      foreach (var info in idInfos)
      {
        var id = info.Key + "Id";
        if (!_validProviderIds.ContainsKey(id))
        {
          _validProviderIds.Add(id, info.Key);
        }
      }

      //Additional Mappings
      _validProviderIds.Add("collectionnumber", "TmdbCollection");
      _validProviderIds.Add("tmdbcolid", "TmdbCollection");
      _validProviderIds.Add("imdb_id", "Imdb");

      DeserializeItem(metadataResult, metadataFile);
    }

    protected virtual void DeserializeItem(MetadataResult<T> metadataResult, string metadataFile)
    {
    }
  }
}

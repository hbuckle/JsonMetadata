using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.IO;
using JsonMetadata.Models;

namespace JsonMetadata.Parsers
{
  class MovieJsonParser : BaseJsonParser<Movie>
  {
    public MovieJsonParser(
      ILogger logger, IConfigurationManager config,
      IProviderManager providerManager, IFileSystem fileSystem) :
      base(logger, config, providerManager, fileSystem) {}

    protected override void DeserializeItem(MetadataResult<Movie> metadataResult, string metadataFile, ILogger logger)
    {
      logger.Log(LogSeverity.Info, $"JsonMetadata: Deserializing {metadataFile}");
      var item = metadataResult.Item;
      using (var stream = FileSystem.OpenRead(metadataFile))
      {
        using (var reader = JsonReaderWriterFactory.CreateJsonReader(stream, Encoding.UTF8, XmlDictionaryReaderQuotas.Max, null))
        {
          var json = DeserializeToObject(reader, typeof(JsonMovie)) as JsonMovie;
          item.Name = json.title;
          item.OriginalTitle = json.originaltitle;
          item.ForcedSortName = json.sorttitle;
          // item.DateCreated = new DateTimeOffset(jsonmovie.dateadded);
          item.CommunityRating = json.communityrating;
          item.CriticRating = json.criticrating;
          item.Tagline = json.tagline;
          item.Overview = json.overview;
          item.PremiereDate = json.releasedate;
          item.ProductionYear = json.year;
          // parentalrating
          item.CustomRating = json.customrating;
          // originalaspectratio
          // 3dformat
          item.SetProviderId(MetadataProviders.Imdb, json.imdbid);
          item.SetProviderId(MetadataProviders.Tmdb, json.tmdbid);
          item.SetProviderId(MetadataProviders.TmdbCollection, json.tmdbcollectionid);
          item.IsLocked = json.lockdata;
          item.Genres = json.genres;
          AddPeople(metadataResult, json.people);
          item.Studios = json.studios;
          item.Tags = json.tags;
        }
      }
    }
  }
}
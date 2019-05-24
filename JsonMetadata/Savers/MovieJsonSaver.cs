using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Serialization;
using System.Collections.Generic;
using System.IO;
using System;
using System.Xml;
using System.Linq;
using MediaBrowser.Controller.IO;
using MediaBrowser.Model.IO;
using JsonMetadata.Configuration;
using JsonMetadata.Models;

namespace JsonMetadata.Savers
{
  public class MovieJsonSaver : BaseJsonSaver
  {
    public MovieJsonSaver(IFileSystem fileSystem, IServerConfigurationManager configurationManager, ILibraryManager libraryManager, IUserManager userManager, IUserDataManager userDataManager, ILogger logger) : base(fileSystem, configurationManager, libraryManager, userManager, userDataManager, logger)
    {
    }
    protected override string GetLocalSavePath(BaseItem item)
    {
      var paths = GetMovieSavePaths(new ItemInfo(item), FileSystem);
      return paths.Count == 0 ? null : paths[0];
    }

    public static List<string> GetMovieSavePaths(ItemInfo item, IFileSystem fileSystem)
    {
      var list = new List<string>();

      var isDvd = string.Equals(item.Container, MediaContainer.Dvd.ToString(), StringComparison.OrdinalIgnoreCase);

      if (isDvd)
      {
        var path = item.ContainingFolderPath;

        list.Add(Path.Combine(path, "VIDEO_TS", "VIDEO_TS.json"));
      }

      if (isDvd || string.Equals(item.Container, MediaContainer.Bluray.ToString(), StringComparison.OrdinalIgnoreCase))
      {
        var path = item.ContainingFolderPath;

        list.Add(Path.Combine(path, Path.GetFileName(path) + ".json"));
      }
      else
      {
        list.Add(Path.ChangeExtension(item.Path, ".json"));

        if (!item.IsInMixedFolder)
        {
          list.Add(Path.Combine(item.ContainingFolderPath, "movie.json"));
        }
      }

      return list;
    }

    public override bool IsEnabledFor(BaseItem item, ItemUpdateType updateType)
    {
      if (!item.SupportsLocalMetadata)
      {
        return false;
      }
      if (item is Movie)
      {
        return updateType >= MinimumUpdateType;
      }
      return false;
    }

    protected override JsonObject SerializeItem(BaseItem item, IServerConfigurationManager options, ILibraryManager libraryManager)
    {
      var hasAspectRatio = item as IHasAspectRatio;
      var output = new JsonMovie()
      {
        id = item.InternalId,
        title = item.Name ?? string.Empty,
        originaltitle = item.OriginalTitle ?? string.Empty,
        sorttitle = item.ForcedSortName ?? string.Empty,
        // dateadded = item.DateCreated.LocalDateTime,
        communityrating = item.CommunityRating,
        criticrating = item.CriticRating,
        tagline = item.Tagline ?? string.Empty,
        overview = item.Overview ?? string.Empty,
        releasedate = item.PremiereDate.HasValue ? item.PremiereDate.Value.LocalDateTime : new DateTime?(),
        year = item.ProductionYear,
        parentalrating = item.OfficialRating ?? string.Empty,
        customrating = item.CustomRating ?? string.Empty,
        originalaspectratio = hasAspectRatio != null ? hasAspectRatio.AspectRatio : string.Empty,
        // threedformat
        imdbid = item.GetProviderId(MetadataProviders.Imdb) ?? string.Empty,
        tmdbid = item.GetProviderId(MetadataProviders.Tmdb) ?? string.Empty,
        tmdbcollectionid = item.GetProviderId(MetadataProviders.TmdbCollection) ?? string.Empty,
        lockdata = item.IsLocked
      };
      output.genres = item.Genres;
      var people = item.SupportsPeople ? libraryManager.GetItemPeople(new InternalPeopleQuery
      {
        ItemIds = new[] { item.InternalId },
        EnableImages = options.GetJsonConfiguration().SaveImagePathsInNfo,
        EnableGuids = true,
        EnableIds = true
      }) : new List<PersonInfo>();
      output.people = new List<JsonCastCrew>();
      foreach (var person in people)
      {
        var personitem = libraryManager.GetItemById(person.Id);
        var image = person.ImageInfos.FirstOrDefault(i => i.Type == ImageType.Primary);
        var jsonperson = new JsonCastCrew();
        jsonperson.thumb = image != null ? GetImagePathToSave(image, libraryManager, options) : string.Empty;
        jsonperson.name = person.Name ?? string.Empty;
        jsonperson.id = person.Id;
        jsonperson.tmdbid = personitem.GetProviderId(MetadataProviders.Tmdb) ?? string.Empty;
        jsonperson.imdbid = personitem.GetProviderId(MetadataProviders.Imdb) ?? string.Empty;
        jsonperson.type = person.Type.ToString();
        switch (person.Type)
        {
          case PersonType.Actor:
            jsonperson.role = person.Role ?? string.Empty;
            break;
          case PersonType.Director:
            jsonperson.role = string.Empty;
            break;
          default:
            break;
        }
        output.people.Add(jsonperson);
      }
      output.studios = item.Studios;
      output.tags = item.Tags;
      return output;
    }
  }
}

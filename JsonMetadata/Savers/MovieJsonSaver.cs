using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using System.Collections.Generic;
using System.IO;
using System;
using System.Xml;
using System.Linq;
using MediaBrowser.Controller.IO;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Xml;
using JsonMetadata.Configuration;
using JsonMetadata.Models;

namespace JsonMetadata.Savers
{
  public class MovieJsonSaver : BaseJsonSaver
  {
    protected override string GetLocalSavePath(BaseItem item)
    {
      var paths = GetMovieSavePaths(new ItemInfo(item), FileSystem);
      return paths.Count == 0 ? null : paths[0];
    }

    public static List<string> GetMovieSavePaths(ItemInfo item, IFileSystem fileSystem)
    {
      var list = new List<string>();

      var isDvd = string.Equals(item.Container, MediaContainer.Dvd, StringComparison.OrdinalIgnoreCase);

      if (isDvd)
      {
        var path = item.ContainingFolderPath;

        list.Add(Path.Combine(path, "VIDEO_TS", "VIDEO_TS.json"));
      }

      if (isDvd || string.Equals(item.Container, MediaContainer.Bluray, StringComparison.OrdinalIgnoreCase))
      {
        var path = item.ContainingFolderPath;

        list.Add(Path.Combine(path, Path.GetFileName(path) + ".json"));
      }
      else
      {
        // http://kodi.wiki/view/Json_files/Movies
        // movie.json will override all and any .json files in the same folder as the media files if you use the "Use foldernames for lookups" setting. If you don't, then moviename.json is used
        //if (!item.IsInMixedFolder && item.ItemType == typeof(Movie))
        //{
        //    list.Add(Path.Combine(item.ContainingFolderPath, "movie.json"));
        //}

        list.Add(Path.ChangeExtension(item.Path, ".json"));

        if (!item.IsInMixedFolder)
        {
          list.Add(Path.Combine(item.ContainingFolderPath, "movie.json"));
        }
      }

      return list;
    }

    protected override string GetRootElementName(BaseItem item)
    {
      return item is MusicVideo ? "musicvideo" : "movie";
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

    protected override void WriteCustomElements(BaseItem item, XmlWriter writer)
    {
      throw new NotImplementedException();
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
        parentalrating = item.GetParentalRatingValue(),
        customrating = item.CustomRating ?? string.Empty,
        originalaspectratio = hasAspectRatio != null ? hasAspectRatio.AspectRatio : string.Empty,
        // threedformat
        imdbid = item.GetProviderId(MetadataProviders.Imdb) ?? string.Empty,
        tmdbid = item.GetProviderId(MetadataProviders.Tmdb) ?? string.Empty,
        tmdbcollectionid = item.GetProviderId(MetadataProviders.TmdbCollection) ?? string.Empty,
        lockdata = item.IsLocked
      };
      output.genres = new List<string>();
      foreach (var genre in item.Genres)
      {
        output.genres.Add(genre);
      }
      var people = item.SupportsPeople ? libraryManager.GetItemPeople(new InternalPeopleQuery
      {
        ItemIds = new[] { item.InternalId },
        EnableImages = options.GetJsonConfiguration().SaveImagePathsInNfo,
        EnableGuids = true,
        EnableIds = true
      }) : new List<PersonInfo>();
      output.people = new List<JsonPerson>();
      foreach (var person in people)
      {
        var personitem = libraryManager.GetItemById(person.Id);
        var image = person.ImageInfos.FirstOrDefault(i => i.Type == ImageType.Primary);
        var jsonperson = new JsonPerson();
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
      output.studios = new List<string>();
      foreach (var studio in item.Studios)
      {
        output.studios.Add(studio);
      }
      output.tags = new List<string>();
      foreach (var tag in item.Tags)
      {
        output.tags.Add(tag);
      }
      return output;
    }

    protected override List<string> GetTagsUsed(BaseItem item)
    {
      var list = base.GetTagsUsed(item);
      list.AddRange(new string[]
      {
        "album",
        "artist",
        "set",
        "id"
      });
      return list;
    }

    public MovieJsonSaver(IFileSystem fileSystem, IServerConfigurationManager configurationManager, ILibraryManager libraryManager, IUserManager userManager, IUserDataManager userDataManager, ILogger logger, IXmlReaderSettingsFactory xmlReaderSettingsFactory) : base(fileSystem, configurationManager, libraryManager, userManager, userDataManager, logger)
    {
    }
  }
}
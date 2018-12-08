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
      var output = new JsonMovie()
      {
        title = item.Name ?? string.Empty,
        sorttitle = item.ForcedSortName ?? string.Empty,
        overview = item.Overview ?? string.Empty,
        tmdbid = item.GetProviderId(MetadataProviders.Tmdb),
        imdbid = item.GetProviderId(MetadataProviders.Imdb),
        year = item.ProductionYear,
        lockdata = item.IsLocked
      };
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
        if (image != null)
        {
          jsonperson.thumb = GetImagePathToSave(image, libraryManager, options);
        }
        jsonperson.name = person.Name ?? string.Empty;
        jsonperson.id = person.Id;
        jsonperson.tmdbid = personitem.GetProviderId(MetadataProviders.Tmdb);
        jsonperson.imdbid = personitem.GetProviderId(MetadataProviders.Imdb);
        jsonperson.type = person.Type.ToString();
        switch (person.Type)
        {
          case PersonType.Actor:
            jsonperson.role = person.Role ?? string.Empty;
            break;
          case PersonType.Director:
            break;
          default:
            break;
        }
        output.people.Add(jsonperson);
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

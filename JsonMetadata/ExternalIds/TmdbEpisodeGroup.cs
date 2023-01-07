using MediaBrowser.Controller.Providers;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Model.Entities;

namespace JsonMetadata.ExternalIds {
  public class TmdbEpisodeGroup : IExternalId {
    public string Name => "TheMovieDb Episode Group";

    public string Key => "TmdbEpisodeGroup";

    public string UrlFormatString => "https://www.themoviedb.org/tv/{0}";

    public bool Supports(IHasProviderIds item) => item is Season;
  }
}

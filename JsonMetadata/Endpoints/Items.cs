using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Net;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Services;

namespace JsonMetadata.Endpoints {
  [Route("/Items/{Id}", "GET")]
  public class GetItem : IReturn<BaseItemDto>, IReturn {
    [ApiMember(Name = "Id", Description = "Item Id", IsRequired = true, DataType = "string", ParameterType = "path")]
    public string Id { get; set; }
  }

  [Authenticated]
  public class ItemService : IService {
    private ILibraryManager libraryManager;
    private IDtoService dtoService;
    public ItemService(ILibraryManager LibraryManager, IDtoService DtoService) {
      this.libraryManager = LibraryManager;
      this.dtoService = DtoService;
    }
    public object Get(GetItem request) {
      try {
        var result = libraryManager.GetItemById(request.Id);
        DtoOptions dtoOptions = new DtoOptions();
        var baseItemDto = dtoService.GetBaseItemDto(result, dtoOptions);
        return baseItemDto;
      }
      catch {
        return null;
      }
    }
  }
}

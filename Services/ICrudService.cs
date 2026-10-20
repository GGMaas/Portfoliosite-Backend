
namespace Portfoliosite_Backend.Services;

public interface ICrudService<TResponse, TCreateDto>
{
    IEnumerable<TResponse> GetAll();
    TResponse? GetById(int id);
    TResponse Create(TCreateDto dto);
    bool Update(int id, TCreateDto dto);
    bool Delete(int id);
}
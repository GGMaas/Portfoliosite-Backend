using Portfoliosite_Backend.DTOs;
namespace Portfoliosite_Backend.Services;

public interface IProjectService
{
    List<ProjectResponseDto> GetAll();
    ProjectResponseDto? GetById(int id);
    ProjectResponseDto Create(CreateProjectDto dto);

    bool Update(int id, CreateProjectDto dto);
    bool Delete(int id);
}
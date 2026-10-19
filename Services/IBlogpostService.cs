using Portfoliosite_Backend.DTOs;

namespace Portfoliosite_Backend.Services;

public interface IBlogpostService
{
    List<BlogpostResponseDto> GetAll();
    BlogpostResponseDto? GetById(int id);
    BlogpostResponseDto Create(CreateBlogpostDto dto);
    bool Update(int id, CreateBlogpostDto dto);
    bool Delete(int id);
}
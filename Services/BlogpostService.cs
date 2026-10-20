using Portfoliosite_Backend.Data;
using Portfoliosite_Backend.DTOs;

namespace Portfoliosite_Backend.Services;


public class BlogpostService : ICrudService<BlogpostResponseDto, CreateBlogpostDto>
{
    private readonly AppDbContext _context;

    public BlogpostService(AppDbContext context)
    {
        _context = context;
    }

    public IEnumerable<BlogpostResponseDto> GetAll()
    {
        return _context.Blogposts.Select(b => new BlogpostResponseDto
        {
            Id = b.Id,
            Title = b.Title,
            Description = b.Description,
            Date = b.Date
        }).ToList();
    }

    public BlogpostResponseDto? GetById(int id)
        => _context.Blogposts.Select(b => new BlogpostResponseDto
        {
            Id = b.Id,
            Title = b.Title,
            Description = b.Description,
            Date = b.Date
        }).FirstOrDefault(b => b.Id == id);

    public BlogpostResponseDto Create(CreateBlogpostDto dto)
    {
        var newBlogpost = new Blogpost
        {
            Title = dto.Title,
            Description = dto.Description,
            Date = DateTime.Now
        };

        _context.Blogposts.Add(newBlogpost);
        _context.SaveChanges();

        return new BlogpostResponseDto
        {
            Id = newBlogpost.Id,
            Title = newBlogpost.Title,
            Description = newBlogpost.Description,
            Date = newBlogpost.Date
        };
    }

    public bool Update(int id, CreateBlogpostDto dto)
    {
        var existing = _context.Blogposts.Find(id);
        if (existing == null) return false;

        existing.Title = dto.Title;
        existing.Description = dto.Description;
        _context.SaveChanges();
        return true;
    }

    public bool Delete(int id)
    {
        var existing = _context.Blogposts.Find(id);
        if (existing == null) return false;

        _context.Blogposts.Remove(existing);
        _context.SaveChanges();
        return true;
    }
}


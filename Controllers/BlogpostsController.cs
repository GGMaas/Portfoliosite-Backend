using Portfoliosite_Backend.DTOs;
using Portfoliosite_Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Portfoliosite_Backend.Controllers;


[ApiController]
[Route("api/[controller]")]
public class BlogpostsController : ControllerBase
{
    private readonly ICrudService<BlogpostResponseDto, CreateBlogpostDto> _blogpostService;

    public BlogpostsController(ICrudService<BlogpostResponseDto, CreateBlogpostDto> blogpostService)
    {
        _blogpostService = blogpostService;
    }

    [HttpGet]
    public ActionResult<List<BlogpostResponseDto>> GetAll()
    {
        IEnumerable<BlogpostResponseDto> blogposts = _blogpostService.GetAll();
        return Ok(blogposts);
    }


    [HttpGet("{id}")]
    public ActionResult<BlogpostResponseDto> GetById(int id)
    {
        BlogpostResponseDto? blogpost = _blogpostService.GetById(id);
        if (blogpost == null)
        {
            return NotFound();
        }

        return Ok(blogpost);
    }

    [HttpPost]
    public ActionResult<BlogpostResponseDto> Create(CreateBlogpostDto dto)
    {
        BlogpostResponseDto created = _blogpostService.Create(dto);
        return CreatedAtAction(
            actionName: nameof(GetById),
            routeValues: new { id = created.Id },
            value: created
        );
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, CreateBlogpostDto dto)
    {
        bool succes = _blogpostService.Update(id, dto);
        if (succes == false)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        bool success = _blogpostService.Delete(id);
        if (!success) return NotFound();
        return NoContent();
    }
}
using Portfoliosite_Backend.DTOs;
using Portfoliosite_Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Portfoliosite_Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly ICrudService<ProjectResponseDto, CreateProjectDto> _projectService;

    public ProjectsController(ICrudService<ProjectResponseDto, CreateProjectDto> projectService)
    {
        _projectService = projectService;
    }

    [AllowAnonymous]
    [HttpGet]
    public ActionResult<List<ProjectResponseDto>> GetAll()
    {
        return Ok(_projectService.GetAll());
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    public ActionResult<ProjectResponseDto> GetById(int id)
    {
        var project = _projectService.GetById(id);
        if (project == null) return NotFound();
        return Ok(project);
    }

    [Authorize(Policy = "CanEditContent")]
    [HttpPost]
    public ActionResult<ProjectResponseDto> Create(CreateProjectDto dto)
    {
        var created = _projectService.Create(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [Authorize(Policy = "CanEditContent")]
    [HttpPut("{id}")]
    public IActionResult Update(int id, CreateProjectDto dto)
    {
        var success = _projectService.Update(id, dto);
        if (!success) return NotFound();
        return NoContent();
    }

    [Authorize(Policy = "CanEditContent")]
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var success = _projectService.Delete(id);
        if (!success) return NotFound();
        return NoContent();
    }
}
using C__.DTOs;

namespace C__.Services;

public class ProjectService : IProjectService
{
    private static readonly List<ProjectResponseDto> _projects = new()
    {
        new ProjectResponseDto
        {
            Id = 1,
            Name = "Project 1",
            Description = "Description of Project 1",
            img = "https://example.com/project1.jpg",
            link = "https://example.com/project1",
            year = 2022
        }
    };

    public List<ProjectResponseDto> GetAll() => _projects;
    public ProjectResponseDto? GetById(int id)
        => _projects.FirstOrDefault(p => p.Id == id);
    public ProjectResponseDto Create(CreateProjectDto dto)
    {
        var newProject = new ProjectResponseDto
        {
            Id = _projects.Count > 0 ? _projects.Max(p => p.Id) + 1 : 1,
            Name = dto.Name,
            Description = dto.Description,
            img = dto.img,
            link = dto.link,
            year = dto.year
        };
        _projects.Add(newProject);
        return newProject;
    }
    public bool Update(int id, CreateProjectDto dto)
    {
        var existing = GetById(id);
        if (existing == null) return false;

        existing.Name = dto.Name;
        existing.Description = dto.Description;
        existing.img = dto.img;
        existing.link = dto.link;
        existing.year = dto.year;
        return true;
    }

    public bool Delete(int id)
    {
        var existing = GetById(id);
        if (existing == null) return false;

        _projects.Remove(existing);
        return true;
    }

};
namespace Portfoliosite_Backend.DTOs;

public sealed record ProjectResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string img { get; set; } = string.Empty;
    public string link { get; set; } = string.Empty;
    public int year { get; set; }
}

public sealed record CreateProjectDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string img { get; set; } = string.Empty;
    public string link { get; set; } = string.Empty;
    public int year { get; set; }
}
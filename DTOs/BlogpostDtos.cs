namespace Portfoliosite_Backend.DTOs;

public sealed record BlogpostResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}

public sealed record CreateBlogpostDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
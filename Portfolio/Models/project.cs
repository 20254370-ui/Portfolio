using System.ComponentModel.DataAnnotations;

namespace Portfolio.Models;

public class Project
{
    public long ProjectId { get; set; }

    [Required(ErrorMessage = "Choose a course.")]
    public long? CourseId { get; set; }

    [Required(ErrorMessage = "Enter the project title.")]
    [StringLength(100, ErrorMessage = "The title can be at most 100 characters.")]
    public string Title { get; set; } = "";

    [StringLength(500, ErrorMessage = "The summary can be at most 500 characters.")]
    public string? Summary { get; set; }

    [StringLength(200, ErrorMessage = "The link can be at most 200 characters.")]
    [RegularExpression(@"^https?://(www\.)?(youtube\.com/(watch\?v=|shorts/)|youtu\.be/)[A-Za-z0-9_-]{11}([&?]\S*)?$", ErrorMessage = "Paste a YouTube link, like https://youtu.be/dQw4w9WgXcQ.")]
    public string? VideoUrl { get; set; }

    public DateOnly? CompletedOn { get; set; }

    [RegularExpression(@"^\s*(https?://\S+\.(png|jpe?g|gif|webp|PNG|JPE?G|GIF|WEBP)(\?\S*)?\s*)*$", ErrorMessage = "Each line must be a link to a .png, .jpg, .gif or .webp picture.")]
    public string? ImageLinks { get; set; }

    public string CourseCode { get; set; } = "";
    public string CourseTitle { get; set; } = "";
    public string? CoverUrl { get; set; }
    public long ImageCount { get; set; }
    public List<string> Images { get; set; } = new();
}
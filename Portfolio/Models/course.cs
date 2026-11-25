using System.ComponentModel.DataAnnotations;

namespace Portfolio.Models;

public class Course
{
    public long CourseId { get; set; }

    [Required(ErrorMessage = "Enter the course code.")]
    [StringLength(12, ErrorMessage = "The code can be at most 12 characters.")]
    [RegularExpression(@"^[A-Z]{2,8}[0-9]{1,2}$", ErrorMessage = "Use capital letters then numbers, like DBMWEB1.")]
    public string Code { get; set; } = "";

    [Required(ErrorMessage = "Enter the course title.")]
    [StringLength(80, ErrorMessage = "The title can be at most 80 characters.")]
    public string Title { get; set; } = "";

    [Required(ErrorMessage = "Enter the school year.")]
    [RegularExpression(@"^[0-9]{4}-[0-9]{4}$", ErrorMessage = "Write the school year like 2026-2027.")]
    public string SchoolYear { get; set; } = "";

    [Required(ErrorMessage = "Choose a semester.")]
    public string Semester { get; set; } = "";

    public long ProjectCount { get; set; }
}
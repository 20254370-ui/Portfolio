namespace Portfolio.Models;

public class HomeViewModel
{
    public List<Course> Courses { get; set; } = new();
    public List<Project> Projects { get; set; } = new();
}
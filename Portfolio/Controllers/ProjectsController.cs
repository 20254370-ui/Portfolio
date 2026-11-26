using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Npgsql;
using Portfolio.Data;
using Portfolio.Models;

namespace Portfolio.Controllers;

public class ProjectsController : Controller
{
    private const string ImagePattern = @"^https?://\S+\.(png|jpe?g|gif|webp)(\?\S*)?$";

    private readonly ProjectRepository _projects;
    private readonly CourseRepository _courses;
    private readonly ProjectImageRepository _images;

    public ProjectsController(ProjectRepository projects, CourseRepository courses, ProjectImageRepository images)
    {
        _projects = projects;
        _courses = courses;
        _images = images;
    }

    private async Task FillCoursesAsync()
    {
        var courses = await _courses.GetAllAsync();
        ViewBag.Courses = courses
            .Select(c => new SelectListItem($"{c.Code} - {c.Title}", c.CourseId.ToString()))
            .ToList();
    }

    private List<string> ReadImageLinks(string? text)
    {
        var urls = new List<string>();
        var lines = (text ?? "").Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        for (int i = 0; i < lines.Length; i++)
        {
            if (!Regex.IsMatch(lines[i], ImagePattern, RegexOptions.IgnoreCase))
            {
                var entry = ModelState[nameof(Project.ImageLinks)];
                if (entry == null || entry.Errors.Count == 0)
                {
                    ModelState.AddModelError(nameof(Project.ImageLinks), $"Line {i + 1} is not a link to a .png, .jpg, .gif or .webp picture.");
                }
                break;
            }
            urls.Add(lines[i]);
        }

        return urls;
    }

    private bool ShowDatabaseError(PostgresException ex)
    {
        if (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            if (ex.ConstraintName == "pk_project_image")
            {
                ModelState.AddModelError(nameof(Project.ImageLinks), "The same picture link appears more than once, so nothing was saved.");
            }
            else
            {
                ModelState.AddModelError(nameof(Project.Title), "This course already has a project with that title.");
            }
            return true;
        }

        if (ex.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            ModelState.AddModelError(nameof(Project.CourseId), "That course does not exist.");
            return true;
        }

        if (ex.SqlState == PostgresErrorCodes.CheckViolation)
        {
            ModelState.AddModelError("", "A rule in the database refused this data, so nothing was saved.");
            return true;
        }

        return false;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _projects.GetAllAsync());
    }

    public async Task<IActionResult> Create()
    {
        await FillCoursesAsync();
        return View(new Project());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Project project)
    {
        var urls = ReadImageLinks(project.ImageLinks);

        if (!ModelState.IsValid)
        {
            await FillCoursesAsync();
            return View(project);
        }

        try
        {
            await _projects.AddAsync(project, urls);
        }
        catch (PostgresException ex)
        {
            if (!ShowDatabaseError(ex))
            {
                throw;
            }
            await FillCoursesAsync();
            return View(project);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(long id)
    {
        var project = await _projects.GetByIdAsync(id);
        if (project == null)
        {
            return NotFound();
        }

        var urls = await _images.GetUrlsAsync(id);
        project.ImageLinks = string.Join(Environment.NewLine, urls);

        await FillCoursesAsync();
        return View(project);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Project project)
    {
        var urls = ReadImageLinks(project.ImageLinks);

        if (!ModelState.IsValid)
        {
            await FillCoursesAsync();
            return View(project);
        }

        if (await _projects.GetByIdAsync(project.ProjectId) == null)
        {
            return NotFound();
        }

        try
        {
            await _projects.UpdateAsync(project, urls);
        }
        catch (PostgresException ex)
        {
            if (!ShowDatabaseError(ex))
            {
                throw;
            }
            await FillCoursesAsync();
            return View(project);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(long id)
    {
        var project = await _projects.GetByIdAsync(id);
        if (project == null)
        {
            return NotFound();
        }
        return View(project);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(long id)
    {
        await _projects.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
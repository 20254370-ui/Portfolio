using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Portfolio.Data;
using Portfolio.Models;

namespace Portfolio.Controllers;

public class CoursesController : Controller
{
    private readonly CourseRepository _courses;

    public CoursesController(CourseRepository courses)
    {
        _courses = courses;
    }

    private bool ShowDatabaseError(PostgresException ex)
    {
        if (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            ModelState.AddModelError(nameof(Course.Code), "Another course already uses this code.");
            return true;
        }

        if (ex.SqlState == PostgresErrorCodes.CheckViolation)
        {
            ModelState.AddModelError(nameof(Course.Semester), "The semester must be 1st semester, 2nd semester or Summer.");
            return true;
        }

        return false;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _courses.GetAllAsync());
    }

    public IActionResult Create()
    {
        return View(new Course());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Course course)
    {
        if (!ModelState.IsValid)
        {
            return View(course);
        }

        try
        {
            await _courses.AddAsync(course);
        }
        catch (PostgresException ex)
        {
            if (!ShowDatabaseError(ex))
            {
                throw;
            }
            return View(course);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(long id)
    {
        var course = await _courses.GetByIdAsync(id);
        if (course == null)
        {
            return NotFound();
        }
        return View(course);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Course course)
    {
        if (!ModelState.IsValid)
        {
            return View(course);
        }

        if (await _courses.GetByIdAsync(course.CourseId) == null)
        {
            return NotFound();
        }

        try
        {
            await _courses.UpdateAsync(course);
        }
        catch (PostgresException ex)
        {
            if (!ShowDatabaseError(ex))
            {
                throw;
            }
            return View(course);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(long id)
    {
        var course = await _courses.GetByIdAsync(id);
        if (course == null)
        {
            return NotFound();
        }
        return View(course);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(long id)
    {
        try
        {
            await _courses.DeleteAsync(id);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            var course = await _courses.GetByIdAsync(id);
            if (course == null)
            {
                return NotFound();
            }
            ViewBag.Error = "This course still has projects, so the database will not delete it. Delete its projects first.";
            return View("Delete", course);
        }

        return RedirectToAction(nameof(Index));
    }
}
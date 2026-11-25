using Npgsql;
using Portfolio.Models;

namespace Portfolio.Data;

public class CourseRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public CourseRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    private static Course ReadCourse(NpgsqlDataReader reader)
    {
        return new Course
        {
            CourseId = reader.GetInt64(0),
            Code = reader.GetString(1),
            Title = reader.GetString(2),
            SchoolYear = reader.GetString(3),
            Semester = reader.GetString(4)
        };
    }

    public async Task<List<Course>> GetAllAsync()
    {
        const string sql =
            "SELECT c.course_id, c.code, c.title, c.school_year, c.semester, COUNT(p.project_id) " +
            "FROM portfolio.course c " +
            "LEFT JOIN portfolio.project p ON p.course_id = c.course_id " +
            "GROUP BY c.course_id " +
            "ORDER BY c.school_year, c.semester, c.code;";

        var courses = new List<Course>();

        await using var command = _dataSource.CreateCommand(sql);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var course = ReadCourse(reader);
            course.ProjectCount = reader.GetInt64(5);
            courses.Add(course);
        }

        return courses;
    }

    public async Task<Course?> GetByIdAsync(long id)
    {
        const string sql =
            "SELECT course_id, code, title, school_year, semester FROM portfolio.course WHERE course_id = @id;";

        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("id", id);
        await using var reader = await command.ExecuteReaderAsync();

        return await reader.ReadAsync() ? ReadCourse(reader) : null;
    }

    public async Task AddAsync(Course course)
    {
        const string sql =
            "INSERT INTO portfolio.course (code, title, school_year, semester) " +
            "VALUES (@code, @title, @school_year, @semester);";

        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("code", course.Code);
        command.Parameters.AddWithValue("title", course.Title);
        command.Parameters.AddWithValue("school_year", course.SchoolYear);
        command.Parameters.AddWithValue("semester", course.Semester);
        await command.ExecuteNonQueryAsync();
    }

    public async Task UpdateAsync(Course course)
    {
        const string sql =
            "UPDATE portfolio.course SET code = @code, title = @title, school_year = @school_year, semester = @semester " +
            "WHERE course_id = @id;";

        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("code", course.Code);
        command.Parameters.AddWithValue("title", course.Title);
        command.Parameters.AddWithValue("school_year", course.SchoolYear);
        command.Parameters.AddWithValue("semester", course.Semester);
        command.Parameters.AddWithValue("id", course.CourseId);
        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(long id)
    {
        const string sql = "DELETE FROM portfolio.course WHERE course_id = @id;";

        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("id", id);
        await command.ExecuteNonQueryAsync();
    }
}
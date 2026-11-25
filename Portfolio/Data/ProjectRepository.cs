using Npgsql;
using Portfolio.Models;

namespace Portfolio.Data;

public class ProjectRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public ProjectRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    private const string SelectProject =
        "SELECT p.project_id, p.course_id, p.title, p.summary, p.video_url, p.completed_on, c.code, c.title, " +
        "       (SELECT i.image_url FROM portfolio.project_image i WHERE i.project_id = p.project_id ORDER BY i.display_order LIMIT 1), " +
        "       (SELECT COUNT(*) FROM portfolio.project_image i WHERE i.project_id = p.project_id) " +
        "FROM portfolio.project p " +
        "JOIN portfolio.course c ON c.course_id = p.course_id ";

    private static Project ReadProject(NpgsqlDataReader reader)
    {
        return new Project
        {
            ProjectId = reader.GetInt64(0),
            CourseId = reader.GetInt64(1),
            Title = reader.GetString(2),
            Summary = reader.IsDBNull(3) ? null : reader.GetString(3),
            VideoUrl = reader.IsDBNull(4) ? null : reader.GetString(4),
            CompletedOn = reader.IsDBNull(5) ? null : reader.GetFieldValue<DateOnly>(5),
            CourseCode = reader.GetString(6),
            CourseTitle = reader.GetString(7),
            CoverUrl = reader.IsDBNull(8) ? null : reader.GetString(8),
            ImageCount = reader.GetInt64(9)
        };
    }

    public async Task<List<Project>> GetAllAsync()
    {
        const string order =
            "ORDER BY c.school_year, c.semester, c.code, p.completed_on DESC NULLS LAST, p.title;";

        var projects = new List<Project>();

        await using var command = _dataSource.CreateCommand(SelectProject + order);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            projects.Add(ReadProject(reader));
        }

        return projects;
    }

    public async Task<Project?> GetByIdAsync(long id)
    {
        await using var command = _dataSource.CreateCommand(SelectProject + "WHERE p.project_id = @id;");
        command.Parameters.AddWithValue("id", id);
        await using var reader = await command.ExecuteReaderAsync();

        return await reader.ReadAsync() ? ReadProject(reader) : null;
    }

    public async Task<long> AddAsync(Project project, List<string> imageUrls)
    {
        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        long projectId;
        await using (var insert = new NpgsqlCommand(
            "INSERT INTO portfolio.project (course_id, title, summary, video_url, completed_on) " +
            "VALUES (@course_id, @title, @summary, @video_url, @completed_on) RETURNING project_id;",
            connection, transaction))
        {
            insert.Parameters.AddWithValue("course_id", project.CourseId!.Value);
            insert.Parameters.AddWithValue("title", project.Title);
            insert.Parameters.AddWithValue("summary", (object?)project.Summary ?? DBNull.Value);
            insert.Parameters.AddWithValue("video_url", (object?)project.VideoUrl ?? DBNull.Value);
            insert.Parameters.AddWithValue("completed_on", (object?)project.CompletedOn ?? DBNull.Value);
            projectId = Convert.ToInt64(await insert.ExecuteScalarAsync());
        }

        await ProjectImageRepository.InsertAsync(connection, transaction, projectId, imageUrls);

        await transaction.CommitAsync();
        return projectId;
    }

    public async Task UpdateAsync(Project project, List<string> imageUrls)
    {
        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        await using (var update = new NpgsqlCommand(
            "UPDATE portfolio.project SET course_id = @course_id, title = @title, summary = @summary, " +
            "video_url = @video_url, completed_on = @completed_on WHERE project_id = @id;",
            connection, transaction))
        {
            update.Parameters.AddWithValue("course_id", project.CourseId!.Value);
            update.Parameters.AddWithValue("title", project.Title);
            update.Parameters.AddWithValue("summary", (object?)project.Summary ?? DBNull.Value);
            update.Parameters.AddWithValue("video_url", (object?)project.VideoUrl ?? DBNull.Value);
            update.Parameters.AddWithValue("completed_on", (object?)project.CompletedOn ?? DBNull.Value);
            update.Parameters.AddWithValue("id", project.ProjectId);
            await update.ExecuteNonQueryAsync();
        }

        await ProjectImageRepository.DeleteAllAsync(connection, transaction, project.ProjectId);
        await ProjectImageRepository.InsertAsync(connection, transaction, project.ProjectId, imageUrls);

        await transaction.CommitAsync();
    }

    public async Task DeleteAsync(long id)
    {
        await using var command = _dataSource.CreateCommand("DELETE FROM portfolio.project WHERE project_id = @id;");
        command.Parameters.AddWithValue("id", id);
        await command.ExecuteNonQueryAsync();
    }
}
using Npgsql;

namespace Portfolio.Data;

public class ProjectImageRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public ProjectImageRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<List<string>> GetUrlsAsync(long projectId)
    {
        const string sql =
            "SELECT image_url FROM portfolio.project_image WHERE project_id = @project_id ORDER BY display_order;";

        var urls = new List<string>();

        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("project_id", projectId);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            urls.Add(reader.GetString(0));
        }

        return urls;
    }

    public static async Task DeleteAllAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, long projectId)
    {
        await using var command = new NpgsqlCommand(
            "DELETE FROM portfolio.project_image WHERE project_id = @project_id;", connection, transaction);
        command.Parameters.AddWithValue("project_id", projectId);
        await command.ExecuteNonQueryAsync();
    }

    public static async Task InsertAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, long projectId, List<string> urls)
    {
        int order = 1;

        foreach (var url in urls)
        {
            await using var command = new NpgsqlCommand(
                "INSERT INTO portfolio.project_image (project_id, image_url, display_order) " +
                "VALUES (@project_id, @image_url, @display_order);", connection, transaction);
            command.Parameters.AddWithValue("project_id", projectId);
            command.Parameters.AddWithValue("image_url", url);
            command.Parameters.AddWithValue("display_order", order);
            await command.ExecuteNonQueryAsync();
            order++;
        }
    }
}
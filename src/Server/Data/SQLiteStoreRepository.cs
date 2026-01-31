
using Microsoft.Data.Sqlite;
using GiftServer.Contracts;

namespace GiftServer.Data;

public class SQLiteStoreRepository : IStoreRepository
{
    private readonly string connectionString;
    private readonly ILogger<SQLiteStoreRepository> logger;
    public SQLiteStoreRepository(
        string connectionString,
        ILogger<SQLiteStoreRepository> logger)
    {
        this.connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using (var connection = new SqliteConnection(this.connectionString))
        {
            connection.Open();
            
            // Enable WAL mode for better concurrency on network file systems
            var pragmaCommand = connection.CreateCommand();
            pragmaCommand.CommandText = @"
                PRAGMA journal_mode = WAL;
                PRAGMA busy_timeout = 5000;
                PRAGMA synchronous = NORMAL;";
            pragmaCommand.ExecuteNonQuery();
            
            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Stores (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    PersonId INTEGER NOT NULL,
                    Name TEXT NOT NULL,
                    Url TEXT,
                    FOREIGN KEY (PersonId) REFERENCES People(PersonId) ON DELETE CASCADE
                )";
            command.ExecuteNonQuery();
        }
    }

    public async Task<Store[]> GetStoresForPerson(int personId)
    {
        using var connection = new SqliteConnection(this.connectionString);
        await connection.OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = """
            SELECT s.Id, s.PersonId, s.Name, s.Url
            FROM Stores s
            WHERE s.PersonId = @personId
        """;
        command.Parameters.AddWithValue("@personId", personId);
        
        var results = ReadAllRecordsAsync(command);
        return results.ToBlockingEnumerable().ToArray();
    }

    public async Task AddStoreAsync(int personId, string name, string link)
    {
        using var connection = new SqliteConnection(this.connectionString);
        await connection.OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Stores (PersonId, Name, Url)
            VALUES (@personId, @name, @url)
        """;   
        command.Parameters.AddWithValue("@personId", personId);
        command.Parameters.AddWithValue("@name", name);
        command.Parameters.AddWithValue("@url", link);
        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteStoreAsync(int id)
    {
        using var connection = new SqliteConnection(this.connectionString);
        await connection.OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = """
            DELETE FROM Stores
            WHERE Id = @id
        """;   
        command.Parameters.AddWithValue("@id", id);
        await command.ExecuteNonQueryAsync();
    }
    
    private async IAsyncEnumerable<Store> ReadAllRecordsAsync(SqliteCommand command)
    {
        using var reader = await command.ExecuteReaderAsync();
        {
            while (await reader.ReadAsync())
            {
                var id = await reader.IsDBNullAsync(reader.GetOrdinal("Id")) ? 0 : reader.GetInt32(reader.GetOrdinal("Id"));
                var personId = await reader.IsDBNullAsync(reader.GetOrdinal("PersonId")) ? 0 : reader.GetInt32(reader.GetOrdinal("PersonId"));
                var name = await reader.IsDBNullAsync(reader.GetOrdinal("Name")) ? "" : reader.GetString(reader.GetOrdinal("Name"));
                var url = await reader.IsDBNullAsync(reader.GetOrdinal("Url")) ? "" : reader.GetString(reader.GetOrdinal("Url"));
                
                yield return new Store(id, personId, name, url);
            }
        }
    }
}

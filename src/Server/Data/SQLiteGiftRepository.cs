using Microsoft.Data.Sqlite;
using GiftServer.Contracts;

namespace GiftServer.Data;

public class SQLiteGiftRepository : IGiftRepository
{
    private readonly string connectionString;
    private readonly ILogger<SQLiteGiftRepository> logger;
    public SQLiteGiftRepository(
        string connectionString,
        ILogger<SQLiteGiftRepository> logger)
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
                CREATE TABLE IF NOT EXISTS Gifts (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    PersonId INTEGER NOT NULL,
                    Link TEXT,
                    Date TEXT,
                    IsPurchased BOOLEAN NOT NULL DEFAULT 0
                )";
            command.ExecuteNonQuery();
        }
    }

    public async Task<Gift[]> GetGiftIdeasForPerson(int personId)
    {
        using var connection = new SqliteConnection(this.connectionString);
        await connection.OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = """
            SELECT g.Id, g.Name, p.Name AS PersonName, g.Link, g.Date, g.IsPurchased
            FROM Gifts g
            JOIN People p on p.Id = g.PersonId
            WHERE PersonId = @personId
        """;
        command.Parameters.AddWithValue("@personId", personId);
        
        var results = ReadAllRecordsAsync(command);
        return results.ToBlockingEnumerable().ToArray();
    }

    public async Task<Gift> GetGift(int giftId)
    {
        using var connection = new SqliteConnection(this.connectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = """
            SELECT g.Id, g.Name, p.Name AS PersonName, g.Link, g.Date, g.IsPurchased
            FROM Gifts g
            JOIN People p on p.Id = g.PersonId
            WHERE g.Id = @id
        """;

        command.Parameters.AddWithValue("@id", giftId);

        return ReadSingleRecord(command);
    }

    public async Task SetIsPurchased(int giftId)
    {
        using var connection = new SqliteConnection(this.connectionString);
        await connection.OpenAsync();
        var command = connection.CreateCommand();

        command.CommandText = """
            UPDATE Gifts
            SET IsPurchased = 1
            WHERE Id = @id
        """;
        command.Parameters.AddWithValue("@id", giftId);
        await command.ExecuteNonQueryAsync();
    }

    public async Task<Gift> AddNewGift(string name, int personId, string link, string date)
    {
        using var connection = new SqliteConnection(this.connectionString);
        await connection.OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Gifts (PersonId, Name, Link, Date)
            VALUES (@personId, @name, @link, @date);
            SELECT last_insert_rowid();
        """;   
        command.Parameters.AddWithValue("@personId", personId);
        command.Parameters.AddWithValue("@name", name);
        command.Parameters.AddWithValue("@link", link);
        command.Parameters.AddWithValue("@date", date);
        var id = await command.ExecuteScalarAsync();
        return await GetGift(Convert.ToInt32(id));
    }

    public async Task DeleteGift(int id)
    {
        using var connection = new SqliteConnection(this.connectionString);
        await connection.OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = """
            DELETE FROM Gifts
            WHERE Id = @id
        """;   
        command.Parameters.AddWithValue("@id", id);
        await command.ExecuteNonQueryAsync();
    }

    private Gift ReadSingleRecord(SqliteCommand command)
    {
        using var reader = command.ExecuteReader();
        {
            if (!reader.Read())
            {
                return null;
            }

            var id = reader.IsDBNull(reader.GetOrdinal("Id")) ? 0 : reader.GetInt32(reader.GetOrdinal("Id"));
            var name = reader.IsDBNull(reader.GetOrdinal("Name")) ? "" : reader.GetString(reader.GetOrdinal("Name"));
            var personName = reader.IsDBNull(reader.GetOrdinal("PersonName")) ? "" : reader.GetString(reader.GetOrdinal("PersonName"));
            var link = reader.IsDBNull(reader.GetOrdinal("Link")) ? "" : reader.GetString(reader.GetOrdinal("Link"));
            var date = reader.IsDBNull(reader.GetOrdinal("Date")) ? "" : reader.GetString(reader.GetOrdinal("Date"));
            var isPurchased = reader.IsDBNull(reader.GetOrdinal("IsPurchased")) ? false : reader.GetBoolean(reader.GetOrdinal("IsPurchased"));
            
            return new Gift(id, name, personName, link, date, isPurchased);
        }
    }

    private async IAsyncEnumerable<Gift> ReadAllRecordsAsync(SqliteCommand command)
    {
        using var reader = await command.ExecuteReaderAsync();
        {
            while (await reader.ReadAsync())
            {
                var id = await reader.IsDBNullAsync(reader.GetOrdinal("Id")) ? 0 : reader.GetInt32(reader.GetOrdinal("Id"));
                var name = await reader.IsDBNullAsync(reader.GetOrdinal("Name")) ? "" : reader.GetString(reader.GetOrdinal("Name"));
                var personName = await reader.IsDBNullAsync(reader.GetOrdinal("PersonName")) ? "" : reader.GetString(reader.GetOrdinal("PersonName"));
                var link = await reader.IsDBNullAsync(reader.GetOrdinal("Link")) ? "" : reader.GetString(reader.GetOrdinal("Link"));
                var date = await reader.IsDBNullAsync(reader.GetOrdinal("Date")) ? "" : reader.GetString(reader.GetOrdinal("Date"));
                var isPurchased = await reader.IsDBNullAsync(reader.GetOrdinal("IsPurchased")) ? false : reader.GetBoolean(reader.GetOrdinal("IsPurchased"));
                
                yield return new Gift(id, name, personName, link, date, isPurchased);
            }
        }
    }
}

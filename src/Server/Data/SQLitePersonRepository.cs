using Microsoft.Data.Sqlite;
using GiftServer.Contracts;

namespace GiftServer.Data;

public class SQLitePersonRepository : IPersonRepository
{
    private readonly string connectionString;
    private readonly ILogger<SQLitePersonRepository> logger;
    public SQLitePersonRepository(
        string connectionString,
        ILogger<SQLitePersonRepository> logger)
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
                CREATE TABLE IF NOT EXISTS People (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Birthday TEXT
                )";
            command.ExecuteNonQuery();

            command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS UserPeople (
                    UserId INTEGER NOT NULL,
                    PersonId INTEGER NOT NULL,
                    UNIQUE (UserId, PersonId),
                    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
                    FOREIGN KEY (PersonId) REFERENCES People(Id) ON DELETE CASCADE
                )";
            command.ExecuteNonQuery();
        }
    }
    
    public async Task<Person[]> GetMyPeople(int userId)
    {
        using var connection = new SqliteConnection(this.connectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = """
            SELECT p.Id, p.Name, p.Birthday FROM People p
            JOIN UserPeople up ON up.PersonId = p.Id
            JOIN User u ON up.UserId = u.UserId
            WHERE u.UserId = @id
        """;

        command.Parameters.AddWithValue("@id", userId);

        var results = ReadAllRecordsAsync(command);
        return results.ToBlockingEnumerable().ToArray();
    }

    public async Task<Person?> GetPerson(int id)
    {
        using var connection = new SqliteConnection(this.connectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = """
            SELECT p.Id, p.Name, p.Birthday FROM People p
            WHERE p.Id = @id
        """;

        command.Parameters.AddWithValue("@id", id);

        var results = ReadAllRecordsAsync(command);
        return results.ToBlockingEnumerable().ToArray()[0];
    }

    public async Task AddNewPerson(string name, string birthday, int userId)
    {
        using var connection = new SqliteConnection(this.connectionString);
        await connection.OpenAsync();
        var command = connection.CreateCommand();

        command.CommandText = """
            INSERT INTO People (Name, Birthday)
            VALUES (@name, @birthday);
            SELECT last_insert_rowid();
        """;
        command.Parameters.AddWithValue("@name", name);
        command.Parameters.AddWithValue("@birthday", birthday);
        var id = await command.ExecuteScalarAsync();

        command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO UserPeople (UserId, PersonId)
            VALUES (@userId, @personId)
        """;   
        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@personId", Convert.ToInt32(id));
        await command.ExecuteNonQueryAsync();
    }

    public async Task DeletePerson(int id)
    {
        using var connection = new SqliteConnection(this.connectionString);
        await connection.OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = """
            DELETE FROM People
            WHERE Id = @id
        """;   
        command.Parameters.AddWithValue("@id", id);
        await command.ExecuteNonQueryAsync();
    }
    
    private async IAsyncEnumerable<Person> ReadAllRecordsAsync(SqliteCommand command)
    {
        using var reader = await command.ExecuteReaderAsync();
        {
            while (await reader.ReadAsync())
            {
                var id = await reader.IsDBNullAsync(reader.GetOrdinal("Id")) ? 0 : reader.GetInt32(reader.GetOrdinal("Id"));
                var name = await reader.IsDBNullAsync(reader.GetOrdinal("Name")) ? "" : reader.GetString(reader.GetOrdinal("Name"));
                var birthday = await reader.IsDBNullAsync(reader.GetOrdinal("Birthday")) ? "" : reader.GetString(reader.GetOrdinal("Birthday"));
                
                if (DateTime.TryParse(birthday, out var parsedBirthday))
                {
                    yield return new Person(id, name, parsedBirthday);
                    continue;
                }

                yield return new Person(id, name, null);
            }
        }
    }
}

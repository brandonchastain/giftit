
using Microsoft.Data.Sqlite;
using GiftServer.Contracts;

namespace GiftServer.Data;

public class SQLiteUserRepository : IUserRepository
{
    private readonly string connectionString;
    private readonly ILogger<SQLiteUserRepository> logger;
    private readonly SemaphoreSlim locker = new SemaphoreSlim(1, 1);

    public SQLiteUserRepository(
        string connectionString,
        ILogger<SQLiteUserRepository> logger)
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
                CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Email TEXT NOT NULL,
                    UNIQUE (Email)
                )";
            command.ExecuteNonQuery();
        }
    }

    public async Task<User> GetUser(string email)
    {
        using var connection = new SqliteConnection(this.connectionString);
        await connection.OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = """
            SELECT u.Id, u.Name, u.Email
            FROM Users u
            WHERE u.Email LIKE @email
        """;
        command.Parameters.AddWithValue("@email", email);
        
        var user = ReadSingleRecord(command);

        if (user == null)
        {
            // auto-create the user if they don't exist
            await locker.WaitAsync();
            try
            {
                // Double-check after acquiring lock
                command = connection.CreateCommand();
                command.CommandText = """
                    SELECT u.Id, u.Name, u.Email
                    FROM Users u
                    WHERE u.Email LIKE @email
                """;
                command.Parameters.AddWithValue("@email", email);
                user = ReadSingleRecord(command);

                if (user == null)
                {
                    await this.AddNewUser(email, "New User");
                    
                    // Double-check after acquiring lock
                    command = connection.CreateCommand();
                    command.CommandText = """
                        SELECT u.Id, u.Name, u.Email
                        FROM Users u
                        WHERE u.Email LIKE @email
                    """;
                    command.Parameters.AddWithValue("@email", email);
                    user = ReadSingleRecord(command);
                }
            }
            finally
            {
                locker.Release();
            }
        }

        return user;
    }

    public async Task AddNewUser(string email, string name)
    {
        using var connection = new SqliteConnection(this.connectionString);
        await connection.OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Users (Name, Email)
            VALUES (@name, @email)
        """;   
        command.Parameters.AddWithValue("@name", name);
        command.Parameters.AddWithValue("@email", email);
        await command.ExecuteNonQueryAsync();
    }

    public Task SetReminderDurAsync(Guid Id, TimeSpan reminderDur)
    {
        throw new NotImplementedException();
    }

    private User ReadSingleRecord(SqliteCommand command)
    {
        using var reader = command.ExecuteReader();
        {
            if (!reader.Read())
            {
                return null;
            }

            var id = reader.IsDBNull(reader.GetOrdinal("Id")) ? 0 : reader.GetInt32(reader.GetOrdinal("Id"));
            var name = reader.IsDBNull(reader.GetOrdinal("Name")) ? "" : reader.GetString(reader.GetOrdinal("Name"));
            var email = reader.IsDBNull(reader.GetOrdinal("Email")) ? "" : reader.GetString(reader.GetOrdinal("Email"));
            
            return new User(id, name, email, false, TimeSpan.Zero);
        }
    }
}

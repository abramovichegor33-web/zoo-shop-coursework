using MySqlConnector;

namespace ZooShop;

public static class Database
{
    public const string ConnectionString =
        "Server=localhost;Port=3306;Database=zoo_shop;User ID=root;Password=root1234;";

    public static string? Login(string login, string password)
    {
        using var conn = new MySqlConnection(ConnectionString);
        conn.Open();

        using var cmd = new MySqlCommand(
            "SELECT r.name FROM users u JOIN roles r ON r.id = u.role_id " +
            "WHERE u.login = @login AND u.password = @password", conn);
        cmd.Parameters.AddWithValue("@login", login);
        cmd.Parameters.AddWithValue("@password", password);

        return cmd.ExecuteScalar() as string;
    }
}
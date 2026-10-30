using System.Collections.Generic;
using MySqlConnector;

namespace ZooShop;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string Description { get; set; } = "";
}

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

    public static List<Product> GetProducts()
    {
        var list = new List<Product>();
        using var conn = new MySqlConnection(ConnectionString);
        conn.Open();

        using var cmd = new MySqlCommand(
            "SELECT p.id, p.name, c.name, p.price, p.quantity, IFNULL(p.description, '') " +
            "FROM products p JOIN categories c ON c.id = p.category_id ORDER BY p.id", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new Product
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Category = reader.GetString(2),
                Price = reader.GetDecimal(3),
                Quantity = reader.GetInt32(4),
                Description = reader.GetString(5)
            });
        }
        return list;
    }
}
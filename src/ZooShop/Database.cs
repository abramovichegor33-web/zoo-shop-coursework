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

public class User
{
    public int Id { get; set; }
    public string Login { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Role { get; set; } = "";
    public bool IsAdmin => Role == "admin";
}

public static class Session
{
    public static User? Current { get; set; }
}

public static class Database
{
    public const string ConnectionString =
        "Server=localhost;Port=3306;Database=zoo_shop;User ID=root;Password=root1234;";

    public static User? Login(string login, string password)
    {
        using var conn = new MySqlConnection(ConnectionString);
        conn.Open();

        using var cmd = new MySqlCommand(
            "SELECT u.id, u.login, u.full_name, r.name FROM users u JOIN roles r ON r.id = u.role_id " +
            "WHERE u.login = @login AND u.password = @password", conn);
        cmd.Parameters.AddWithValue("@login", login);
        cmd.Parameters.AddWithValue("@password", password);

        using var reader = cmd.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }
        return new User
        {
            Id = reader.GetInt32(0),
            Login = reader.GetString(1),
            FullName = reader.GetString(2),
            Role = reader.GetString(3)
        };
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

    public static List<string> GetCategories()
    {
        var list = new List<string>();
        using var conn = new MySqlConnection(ConnectionString);
        conn.Open();

        using var cmd = new MySqlCommand("SELECT name FROM categories ORDER BY name", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(reader.GetString(0));
        }
        return list;
    }

    public static void AddProduct(string name, string category, decimal price, int quantity, string description)
    {
        using var conn = new MySqlConnection(ConnectionString);
        conn.Open();

        using var cmd = new MySqlCommand(
            "INSERT INTO products (name, category_id, price, quantity, description) " +
            "SELECT @name, id, @price, @quantity, @description FROM categories WHERE name = @category", conn);
        cmd.Parameters.AddWithValue("@name", name);
        cmd.Parameters.AddWithValue("@category", category);
        cmd.Parameters.AddWithValue("@price", price);
        cmd.Parameters.AddWithValue("@quantity", quantity);
        cmd.Parameters.AddWithValue("@description", description);
        cmd.ExecuteNonQuery();
    }

    public static void DeleteProduct(int id)
    {
        using var conn = new MySqlConnection(ConnectionString);
        conn.Open();

        using var cmd = new MySqlCommand("DELETE FROM products WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    public static void UpdateFullName(int userId, string fullName)
    {
        using var conn = new MySqlConnection(ConnectionString);
        conn.Open();

        using var cmd = new MySqlCommand("UPDATE users SET full_name = @name WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("@name", fullName);
        cmd.Parameters.AddWithValue("@id", userId);
        cmd.ExecuteNonQuery();
    }

    public static bool ChangePassword(int userId, string oldPassword, string newPassword)
    {
        using var conn = new MySqlConnection(ConnectionString);
        conn.Open();

        using var cmd = new MySqlCommand(
            "UPDATE users SET password = @new WHERE id = @id AND password = @old", conn);
        cmd.Parameters.AddWithValue("@new", newPassword);
        cmd.Parameters.AddWithValue("@id", userId);
        cmd.Parameters.AddWithValue("@old", oldPassword);
        return cmd.ExecuteNonQuery() > 0;
    }
}

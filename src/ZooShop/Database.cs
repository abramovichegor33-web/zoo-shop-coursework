using System;
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

public class CartItem
{
    public int ProductId { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public int MaxQuantity { get; set; }
    public decimal Sum => Price * Quantity;
}

public class OrderRow
{
    public int Id { get; set; }
    public string Date { get; set; } = "";
    public decimal Total { get; set; }
    public string Payment { get; set; } = "";
    public string Status { get; set; } = "";
    public string Items { get; set; } = "";
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
    public static List<CartItem> Cart { get; } = new();
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

    public static int PlaceOrder(int userId, List<CartItem> items, string paymentMethod = "Картой")
    {
        using var conn = new MySqlConnection(ConnectionString);
        conn.Open();
        using var tx = conn.BeginTransaction();

        try
        {
            decimal total = 0;
            var prices = new Dictionary<int, decimal>();

            foreach (var item in items)
            {
                using var check = new MySqlCommand(
                    "SELECT price, quantity, name FROM products WHERE id = @id FOR UPDATE", conn, tx);
                check.Parameters.AddWithValue("@id", item.ProductId);
                using var r = check.ExecuteReader();
                if (!r.Read())
                {
                    throw new Exception("Товар «" + item.Name + "» больше не продаётся");
                }
                var price = r.GetDecimal(0);
                var stock = r.GetInt32(1);
                if (stock < item.Quantity)
                {
                    throw new Exception("Недостаточно товара «" + item.Name + "» на складе (осталось " + stock + ")");
                }
                prices[item.ProductId] = price;
                total += price * item.Quantity;
            }

            long orderId;
            using (var ins = new MySqlCommand(
                "INSERT INTO orders (user_id, total, payment_method, status) VALUES (@u, @t, @pm, 'Оплачен')", conn, tx))
            {
                ins.Parameters.AddWithValue("@u", userId);
                ins.Parameters.AddWithValue("@t", total);
                ins.Parameters.AddWithValue("@pm", paymentMethod);
                ins.ExecuteNonQuery();
                orderId = ins.LastInsertedId;
            }

            foreach (var item in items)
            {
                using var line = new MySqlCommand(
                    "INSERT INTO order_items (order_id, product_id, quantity, price) VALUES (@o, @p, @q, @pr)", conn, tx);
                line.Parameters.AddWithValue("@o", orderId);
                line.Parameters.AddWithValue("@p", item.ProductId);
                line.Parameters.AddWithValue("@q", item.Quantity);
                line.Parameters.AddWithValue("@pr", prices[item.ProductId]);
                line.ExecuteNonQuery();

                using var upd = new MySqlCommand(
                    "UPDATE products SET quantity = quantity - @q WHERE id = @p", conn, tx);
                upd.Parameters.AddWithValue("@q", item.Quantity);
                upd.Parameters.AddWithValue("@p", item.ProductId);
                upd.ExecuteNonQuery();
            }

            tx.Commit();
            return (int)orderId;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public static List<OrderRow> GetOrders(int userId)
    {
        var list = new List<OrderRow>();
        using var conn = new MySqlConnection(ConnectionString);
        conn.Open();

        using var cmd = new MySqlCommand(
            "SELECT o.id, o.created_at, o.total, o.payment_method, o.status, " +
            "IFNULL(GROUP_CONCAT(CONCAT(p.name, ' x', i.quantity) SEPARATOR ', '), '') " +
            "FROM orders o " +
            "LEFT JOIN order_items i ON i.order_id = o.id " +
            "LEFT JOIN products p ON p.id = i.product_id " +
            "WHERE o.user_id = @u " +
            "GROUP BY o.id, o.created_at, o.total, o.payment_method, o.status ORDER BY o.id DESC", conn);
        cmd.Parameters.AddWithValue("@u", userId);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new OrderRow
            {
                Id = reader.GetInt32(0),
                Date = reader.GetDateTime(1).ToString("dd.MM.yyyy HH:mm"),
                Total = reader.GetDecimal(2),
                Payment = reader.GetString(3),
                Status = reader.GetString(4),
                Items = reader.GetString(5)
            });
        }
        return list;
    }
}
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.Sqlite;
using project.Models;

namespace project.Data
{
    public class SqlDatabaseService
    {
        private readonly string _connectionString;

        public SqlDatabaseService(IConfiguration configuration, IWebHostEnvironment env)
        {
            var dbFile = configuration.GetConnectionString("DefaultConnection") ?? "Data Source=TechZoneStore.db";
            _connectionString = dbFile;
        }

        private SqliteConnection GetConnection() => new SqliteConnection(_connectionString);

        public static string HashPassword(string password)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes);
        }

        public void InitializeDatabase()
        {
            using var conn = GetConnection();
            conn.Open();

            // Create Users & Products tables using SQL DDL
            const string createTablesSql = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    FullName TEXT NOT NULL,
                    Username TEXT NOT NULL UNIQUE,
                    Email TEXT NOT NULL UNIQUE,
                    PasswordHash TEXT NOT NULL,
                    Role TEXT NOT NULL DEFAULT 'User',
                    CreatedAt TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS Products (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Category TEXT NOT NULL,
                    Brand TEXT NOT NULL,
                    Price REAL NOT NULL,
                    StockQuantity INTEGER NOT NULL DEFAULT 0,
                    Description TEXT NOT NULL,
                    ImageUrl TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL
                );";

            using (var cmd = new SqliteCommand(createTablesSql, conn))
            {
                cmd.ExecuteNonQuery();
            }

            // Seed default Admin and User accounts if Users table is empty
            using (var checkUsersCmd = new SqliteCommand("SELECT COUNT(*) FROM Users;", conn))
            {
                long userCount = (long)(checkUsersCmd.ExecuteScalar() ?? 0L);
                if (userCount == 0)
                {
                    InsertUserInternal(conn, new AppUser
                    {
                        FullName = "مدير النظام (Admin)",
                        Username = "admin",
                        Email = "admin@techzone.sa",
                        PasswordHash = HashPassword("Admin@123"),
                        Role = "Admin",
                        CreatedAt = DateTime.Now
                    });

                    InsertUserInternal(conn, new AppUser
                    {
                        FullName = "أحمد محمد (مستخدم قياسي)",
                        Username = "user",
                        Email = "user@techzone.sa",
                        PasswordHash = HashPassword("User@123"),
                        Role = "User",
                        CreatedAt = DateTime.Now
                    });
                }
            }

            // Seed initial tech products if Products table is empty
            using (var checkProductsCmd = new SqliteCommand("SELECT COUNT(*) FROM Products;", conn))
            {
                long productCount = (long)(checkProductsCmd.ExecuteScalar() ?? 0L);
                if (productCount == 0)
                {
                    var seedProducts = new List<Product>
                    {
                        new Product
                        {
                            Name = "لابتوب MacBook Pro 16 M3 Max",
                            Category = "حواسيب محمولة",
                            Brand = "Apple",
                            Price = 12499.00m,
                            StockQuantity = 15,
                            Description = "معالج Apple M3 Max بـ 16 نواة، ذاكرة موحدة 48GB RAM، تخزين 1TB SSD فائق السرعة، شاشة Liquid Retina XDR مقاس 16.2 بوصة مناسبة للمبرمجين والمصممين.",
                            ImageUrl = "https://images.unsplash.com/photo-1517336714731-489689fd1ca8?auto=format&fit=crop&w=800&q=80",
                            CreatedAt = DateTime.Now.AddDays(-10)
                        },
                        new Product
                        {
                            Name = "هاتف Samsung Galaxy S24 Ultra 5G",
                            Category = "هواتف ذكية",
                            Brand = "Samsung",
                            Price = 4899.00m,
                            StockQuantity = 28,
                            Description = "شاشة Dynamic AMOLED 2X مقاس 6.8 بوصة، إطار من التيتانيوم، كاميرا احترافية بدقة 200 ميجابكسل مع ميزات الذكاء الاصطناعي Galaxy AI وقلم S-Pen مدمج.",
                            ImageUrl = "https://images.unsplash.com/photo-1610945415295-d9bbf067e59c?auto=format&fit=crop&w=800&q=80",
                            CreatedAt = DateTime.Now.AddDays(-8)
                        },
                        new Product
                        {
                            Name = "سماعة رأس لاسلكية Sony WH-1000XM5",
                            Category = "صوتيات وملحقات",
                            Brand = "Sony",
                            Price = 1399.00m,
                            StockQuantity = 42,
                            Description = "سماعة رأس احترافية بأفضل تقنية عزل ضوضاء نشط (ANC) في فئتها، بطارية تدوم حتى 30 ساعة، صوت عالي الدقة Hi-Res، وتصميم مريح لساعات العمل الطويلة.",
                            ImageUrl = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?auto=format&fit=crop&w=800&q=80",
                            CreatedAt = DateTime.Now.AddDays(-6)
                        },
                        new Product
                        {
                            Name = "شاشة الألعاب والتصميم LG UltraGear OLED 34",
                            Category = "شاشات العرض",
                            Brand = "LG",
                            Price = 3650.00m,
                            StockQuantity = 9,
                            Description = "شاشة منحنية UltraWide مقاس 34 بوصة بدقة WQHD وتقنية OLED، معدل تحديث 240Hz وزمن استجابة 0.03ms مع تغطية لونية DCI-P3 بنسبة 98.5%.",
                            ImageUrl = "https://images.unsplash.com/photo-1527443224154-c4a3942d3acf?auto=format&fit=crop&w=800&q=80",
                            CreatedAt = DateTime.Now.AddDays(-4)
                        },
                        new Product
                        {
                            Name = "لوحة مفاتيح ميكانيكية لاسلكية Keychron K8 Pro",
                            Category = "صوتيات وملحقات",
                            Brand = "Keychron",
                            Price = 490.00m,
                            StockQuantity = 35,
                            Description = "لوحة مفاتيح ميكانيكية لاسلكية تدعم برمجة QMK/VIA، هيكل ألومنيوم متين، إضاءة RGB خلفية، ومفاتيح قابلة للتبديل السريع متوافقة مع Windows و Mac.",
                            ImageUrl = "https://images.unsplash.com/photo-1587829741301-dc798b83add3?auto=format&fit=crop&w=800&q=80",
                            CreatedAt = DateTime.Now.AddDays(-2)
                        },
                        new Product
                        {
                            Name = "جهاز لوحي iPad Pro M4 مقاس 13 بوصة",
                            Category = "أجهزة لوحية",
                            Brand = "Apple",
                            Price = 5499.00m,
                            StockQuantity = 18,
                            Description = "أنحف جهاز من Apple بشاشة Ultra Retina XDR OLED ثنائية الطبقة، شريحة M4 الخارقة للذكاء الاصطناعي والرسوميات، ودعم قلم Apple Pencil Pro.",
                            ImageUrl = "https://images.unsplash.com/photo-1544244015-0df4b3ffc6b0?auto=format&fit=crop&w=800&q=80",
                            CreatedAt = DateTime.Now.AddDays(-1)
                        }
                    };

                    foreach (var p in seedProducts)
                    {
                        CreateProductInternal(conn, p);
                    }
                }
            }
        }

        // ==================== PRODUCTS SQL OPERATIONS ====================

        public List<Product> GetAllProducts(string? search = null, string? category = null)
        {
            var products = new List<Product>();
            using var conn = GetConnection();
            conn.Open();

            var sql = "SELECT Id, Name, Category, Brand, Price, StockQuantity, Description, ImageUrl, CreatedAt FROM Products WHERE 1=1";
            if (!string.IsNullOrWhiteSpace(search))
            {
                sql += " AND (Name LIKE @Search OR Brand LIKE @Search OR Description LIKE @Search)";
            }
            if (!string.IsNullOrWhiteSpace(category))
            {
                sql += " AND Category = @Category";
            }
            sql += " ORDER BY Id DESC;";

            using var cmd = new SqliteCommand(sql, conn);
            if (!string.IsNullOrWhiteSpace(search))
            {
                cmd.Parameters.AddWithValue("@Search", $"%{search.Trim()}%");
            }
            if (!string.IsNullOrWhiteSpace(category))
            {
                cmd.Parameters.AddWithValue("@Category", category.Trim());
            }

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                products.Add(MapProduct(reader));
            }
            return products;
        }

        public List<string> GetCategories()
        {
            var categories = new List<string>();
            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqliteCommand("SELECT DISTINCT Category FROM Products ORDER BY Category;", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                categories.Add(reader.GetString(0));
            }
            return categories;
        }

        public Product? GetProductById(int id)
        {
            using var conn = GetConnection();
            conn.Open();
            const string sql = "SELECT Id, Name, Category, Brand, Price, StockQuantity, Description, ImageUrl, CreatedAt FROM Products WHERE Id = @Id;";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return MapProduct(reader);
            }
            return null;
        }

        public void CreateProduct(Product product)
        {
            using var conn = GetConnection();
            conn.Open();
            CreateProductInternal(conn, product);
        }

        private static void CreateProductInternal(SqliteConnection conn, Product product)
        {
            const string sql = @"
                INSERT INTO Products (Name, Category, Brand, Price, StockQuantity, Description, ImageUrl, CreatedAt)
                VALUES (@Name, @Category, @Brand, @Price, @StockQuantity, @Description, @ImageUrl, @CreatedAt);";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Name", product.Name);
            cmd.Parameters.AddWithValue("@Category", product.Category);
            cmd.Parameters.AddWithValue("@Brand", product.Brand);
            cmd.Parameters.AddWithValue("@Price", (double)product.Price);
            cmd.Parameters.AddWithValue("@StockQuantity", product.StockQuantity);
            cmd.Parameters.AddWithValue("@Description", product.Description);
            cmd.Parameters.AddWithValue("@ImageUrl", string.IsNullOrWhiteSpace(product.ImageUrl) ? "https://images.unsplash.com/photo-1526738549149-8e07eca6c147?auto=format&fit=crop&w=800&q=80" : product.ImageUrl);
            cmd.Parameters.AddWithValue("@CreatedAt", product.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
            cmd.ExecuteNonQuery();
        }

        public void UpdateProduct(Product product)
        {
            using var conn = GetConnection();
            conn.Open();
            const string sql = @"
                UPDATE Products
                SET Name = @Name,
                    Category = @Category,
                    Brand = @Brand,
                    Price = @Price,
                    StockQuantity = @StockQuantity,
                    Description = @Description,
                    ImageUrl = @ImageUrl
                WHERE Id = @Id;";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", product.Id);
            cmd.Parameters.AddWithValue("@Name", product.Name);
            cmd.Parameters.AddWithValue("@Category", product.Category);
            cmd.Parameters.AddWithValue("@Brand", product.Brand);
            cmd.Parameters.AddWithValue("@Price", (double)product.Price);
            cmd.Parameters.AddWithValue("@StockQuantity", product.StockQuantity);
            cmd.Parameters.AddWithValue("@Description", product.Description);
            cmd.Parameters.AddWithValue("@ImageUrl", string.IsNullOrWhiteSpace(product.ImageUrl) ? "https://images.unsplash.com/photo-1526738549149-8e07eca6c147?auto=format&fit=crop&w=800&q=80" : product.ImageUrl);
            cmd.ExecuteNonQuery();
        }

        public void DeleteProduct(int id)
        {
            using var conn = GetConnection();
            conn.Open();
            const string sql = "DELETE FROM Products WHERE Id = @Id;";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();
        }

        private static Product MapProduct(SqliteDataReader reader)
        {
            return new Product
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Category = reader.GetString(2),
                Brand = reader.GetString(3),
                Price = Convert.ToDecimal(reader.GetDouble(4)),
                StockQuantity = reader.GetInt32(5),
                Description = reader.GetString(6),
                ImageUrl = reader.GetString(7),
                CreatedAt = DateTime.TryParse(reader.GetString(8), out var dt) ? dt : DateTime.Now
            };
        }

        // ==================== USERS & AUTH SQL OPERATIONS ====================

        public AppUser? AuthenticateUser(string usernameOrEmail, string password)
        {
            using var conn = GetConnection();
            conn.Open();
            var hash = HashPassword(password);
            const string sql = @"
                SELECT Id, FullName, Username, Email, PasswordHash, Role, CreatedAt
                FROM Users
                WHERE (LOWER(Username) = LOWER(@Identifier) OR LOWER(Email) = LOWER(@Identifier))
                  AND PasswordHash = @PasswordHash;";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Identifier", usernameOrEmail.Trim());
            cmd.Parameters.AddWithValue("@PasswordHash", hash);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return MapUser(reader);
            }
            return null;
        }

        public bool UsernameOrEmailExists(string username, string email)
        {
            using var conn = GetConnection();
            conn.Open();
            const string sql = "SELECT COUNT(*) FROM Users WHERE LOWER(Username) = LOWER(@Username) OR LOWER(Email) = LOWER(@Email);";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Username", username.Trim());
            cmd.Parameters.AddWithValue("@Email", email.Trim());
            long count = (long)(cmd.ExecuteScalar() ?? 0L);
            return count > 0;
        }

        public AppUser RegisterUser(RegisterViewModel model)
        {
            using var conn = GetConnection();
            conn.Open();
            var newUser = new AppUser
            {
                FullName = model.FullName.Trim(),
                Username = model.Username.Trim(),
                Email = model.Email.Trim(),
                PasswordHash = HashPassword(model.Password),
                Role = "User", // Requirement #3 & #7: Newly registered accounts get 'User' (read-only) role
                CreatedAt = DateTime.Now
            };
            newUser.Id = InsertUserInternal(conn, newUser);
            return newUser;
        }

        public List<AppUser> GetAllUsers()
        {
            var users = new List<AppUser>();
            using var conn = GetConnection();
            conn.Open();
            using var cmd = new SqliteCommand("SELECT Id, FullName, Username, Email, PasswordHash, Role, CreatedAt FROM Users ORDER BY Id;", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                users.Add(MapUser(reader));
            }
            return users;
        }

        private static int InsertUserInternal(SqliteConnection conn, AppUser user)
        {
            const string sql = @"
                INSERT INTO Users (FullName, Username, Email, PasswordHash, Role, CreatedAt)
                VALUES (@FullName, @Username, @Email, @PasswordHash, @Role, @CreatedAt);
                SELECT last_insert_rowid();";
            using var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@FullName", user.FullName);
            cmd.Parameters.AddWithValue("@Username", user.Username);
            cmd.Parameters.AddWithValue("@Email", user.Email);
            cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
            cmd.Parameters.AddWithValue("@Role", user.Role);
            cmd.Parameters.AddWithValue("@CreatedAt", user.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        private static AppUser MapUser(SqliteDataReader reader)
        {
            return new AppUser
            {
                Id = reader.GetInt32(0),
                FullName = reader.GetString(1),
                Username = reader.GetString(2),
                Email = reader.GetString(3),
                PasswordHash = reader.GetString(4),
                Role = reader.GetString(5),
                CreatedAt = DateTime.TryParse(reader.GetString(6), out var dt) ? dt : DateTime.Now
            };
        }
    }
}

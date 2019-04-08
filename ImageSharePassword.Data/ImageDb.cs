using System.Collections.Generic;
using System.Data.SqlClient;


namespace ImageSharePassword.Data
{
    public class Image
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string Password { get; set; }
        public int Views { get; set; }
        public int UserId { get; set; }
    }
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }

    }
    public class ImageDb
    {
        private string _connectionString;

        public ImageDb(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void Add(Image image)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO Images (FileName, Password, Views,userid) " +
                                  "VALUES (@fileName, @password, 0,@userid) SELECT SCOPE_IDENTITY()";
                cmd.Parameters.AddWithValue("@fileName", image.FileName);
                cmd.Parameters.AddWithValue("@password", image.Password);
                cmd.Parameters.AddWithValue("@userid", image.UserId);
                connection.Open();
                image.Id = (int)(decimal)cmd.ExecuteScalar();
            }
        }

        public Image GetById(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT TOP 1 * FROM Images WHERE Id = @id";
                cmd.Parameters.AddWithValue("@id", id);
                connection.Open();
                var reader = cmd.ExecuteReader();
                if (!reader.Read())
                {
                    return null;
                }

                return new Image
                {
                    Id = (int)reader["Id"],
                    Password = (string)reader["Password"],
                    FileName = (string)reader["FileName"],
                    Views = (int)reader["Views"]
                };
            }
        }

        public void IncrementViewCount(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "UPDATE Images SET Views = Views + 1 WHERE Id = @id";
                cmd.Parameters.AddWithValue("@id", id);
                connection.Open();
                cmd.ExecuteNonQuery();
            }
        }
        public void AddUser(User user, string password)
        {
            string hash = BCrypt.Net.BCrypt.HashPassword(password);
            user.PasswordHash = hash;
            using (var connection = new SqlConnection(_connectionString))
            {
                var cmd = connection.CreateCommand();
                cmd.CommandText = "INSERT INTO Users (Name, Email, PasswordHash) " +
                                  "VALUES (@name, @email, @hash)";
                cmd.Parameters.AddWithValue("@name", user.Name);
                cmd.Parameters.AddWithValue("@email", user.Email);
                cmd.Parameters.AddWithValue("@hash", user.PasswordHash);
                connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public User Login(string email, string password)
        {
            var user = GetByEmail(email);
            if (user == null)
            {
                return null; //incorrect email
            }

            bool isValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            if (!isValid)
            {
                return null;
            }

            return user;
        }

        public User GetByEmail(string email)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT TOP 1 * FROM Users WHERE Email = @email";
                cmd.Parameters.AddWithValue("@email", email);
                connection.Open();
                var reader = cmd.ExecuteReader();
                if (!reader.Read())
                {
                    return null;
                }

                return new User
                {
                    Id = (int)reader["Id"],
                    Name = (string)reader["Name"],
                    PasswordHash = (string)reader["PasswordHash"],
                    Email = (string)reader["Email"]
                };
            }
        }
        public IEnumerable<Image> GetImagesForUser(int userId)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT  * FROM Images WHERE userid = @id";
                cmd.Parameters.AddWithValue("@id", userId);
                connection.Open();
                var reader = cmd.ExecuteReader();
                List<Image> images = new List<Image>();
                while(reader.Read())
                {
                    images.Add(new Image
                    {
                        Id = (int)reader["Id"],
                        Password = (string)reader["Password"],
                        FileName = (string)reader["FileName"],
                        Views = (int)reader["Views"]
                    });
                }

                return images;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Model;

namespace DAL
{
    public class userDal
    {
        private readonly string _connectionString;

        public userDal(string connectionString)
        {
            _connectionString = connectionString;
        }

        // 1. שליפת כל המשתמשים הפעילים
        public async Task<List<User>> GetAllUsersAsync()
        {
            List<User> users = new List<User>();

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string query = "SELECT Id, Username, FullName, Email, Password, RoleId, IsActive FROM users WHERE IsActive = 1";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            users.Add(new User
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Username = reader["Username"] != DBNull.Value ? reader["Username"].ToString()! : "",
                                FullName = reader["FullName"] != DBNull.Value ? reader["FullName"].ToString()! : "",
                                Email = reader["Email"] != DBNull.Value ? reader["Email"].ToString()! : "",
                                Password = reader["Password"] != DBNull.Value ? reader["Password"].ToString()! : "",
                                RoleId = reader["RoleId"] != DBNull.Value ? Convert.ToInt32(reader["RoleId"]) : 1,
                                IsActive = Convert.ToBoolean(reader["IsActive"])
                            });
                        }
                    }
                }
            }
            return users;
        }

        // 2. בדיקת כפילות אימייל
        public async Task<bool> IsEmailExistsAsync(string email)
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string query = "SELECT COUNT(*) FROM users WHERE Email = @Email";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", email);
                    long count = Convert.ToInt64(await cmd.ExecuteScalarAsync() ?? 0);
                    return count > 0;
                }
            }
        }

        // בדיקת כפילות שם משתמש
        public async Task<bool> IsUsernameExistsAsync(string username)
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string query = "SELECT COUNT(*) FROM users WHERE Username = @Username";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    long count = Convert.ToInt64(await cmd.ExecuteScalarAsync() ?? 0);
                    return count > 0;
                }
            }
        }

        // 3. הוספת משתמש חדש (כולל Username!)
        public async Task<bool> AddUserAsync(User user)
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string query = @"INSERT INTO users (Username, FullName, Email, Password, RoleId, IsActive) 
                                 VALUES (@Username, @FullName, @Email, @Password, @RoleId, 1)";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", user.Username);
                    cmd.Parameters.AddWithValue("@FullName", user.FullName);
                    cmd.Parameters.AddWithValue("@Email", user.Email);
                    cmd.Parameters.AddWithValue("@Password", user.Password);
                    cmd.Parameters.AddWithValue("@RoleId", user.RoleId > 0 ? user.RoleId : 1);

                    int rows = await cmd.ExecuteNonQueryAsync();
                    return rows > 0;
                }
            }
        }

        // 4. אימות התחברות (עובד גם עם אימייל וגם עם שם משתמש!)
        public async Task<User?> LoginAsync(string identifier, string password)
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string query = @"SELECT Id, Username, FullName, Email, Password, RoleId, IsActive 
                                 FROM users 
                                 WHERE (Email = @Identifier OR Username = @Identifier) 
                                   AND Password = @Password 
                                   AND IsActive = 1";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Identifier", identifier);
                    cmd.Parameters.AddWithValue("@Password", password);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new User
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Username = reader["Username"] != DBNull.Value ? reader["Username"].ToString()! : "",
                                FullName = reader["FullName"] != DBNull.Value ? reader["FullName"].ToString()! : "",
                                Email = reader["Email"] != DBNull.Value ? reader["Email"].ToString()! : "",
                                Password = reader["Password"] != DBNull.Value ? reader["Password"].ToString()! : "",
                                RoleId = reader["RoleId"] != DBNull.Value ? Convert.ToInt32(reader["RoleId"]) : 1,
                                IsActive = Convert.ToBoolean(reader["IsActive"])
                            };
                        }
                    }
                }
            }
            return null;
        }

        // 5. מחיקה רכה
        public async Task<bool> DeleteUserAsync(int id)
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string query = "UPDATE users SET IsActive = 0 WHERE Id = @Id";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    return await cmd.ExecuteNonQueryAsync() > 0;
                }
            }
        }
    }
}
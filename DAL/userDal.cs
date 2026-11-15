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
                string query = "SELECT Id, FullName, Email, Password, RoleId, IsActive FROM users WHERE IsActive = 1";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            users.Add(new User
                            {
                                Id = Convert.ToInt32(reader["Id"]),
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

        // 3. הוספת משתמש חדש
        public async Task<bool> AddUserAsync(User user)
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string query = @"INSERT INTO users (FullName, Email, Password, RoleId, IsActive) 
                                VALUES (@FullName, @Email, @Password, @RoleId, 1)";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@FullName", user.FullName);
                    cmd.Parameters.AddWithValue("@Email", user.Email);
                    cmd.Parameters.AddWithValue("@Password", user.Password);
                    cmd.Parameters.AddWithValue("@RoleId", user.RoleId > 0 ? user.RoleId : 1);

                    int rows = await cmd.ExecuteNonQueryAsync();
                    return rows > 0;
                }
            }
        }

        // 4. אימות התחברות (Login)
        public async Task<User?> LoginAsync(string email, string password)
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string query = "SELECT Id, FullName, Email, Password, RoleId, IsActive FROM users WHERE Email = @Email AND Password = @Password AND IsActive = 1";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@Password", password);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new User
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                FullName = reader["FullName"].ToString()!,
                                Email = reader["Email"].ToString()!,
                                Password = reader["Password"].ToString()!,
                                RoleId = Convert.ToInt32(reader["RoleId"]),
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
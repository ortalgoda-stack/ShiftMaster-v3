using System;
using MySql.Data.MySqlClient;

namespace DAL
{
    public class DatabaseInitializer
    {
        private readonly string _connectionString;

        public DatabaseInitializer(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void Initialize()
        {
            // 1. התחברות לשרת MySQL ליצירת מסד הנתונים אם אינו קיים
            var builder = new MySqlConnectionStringBuilder(_connectionString);
            string databaseName = builder.Database;
            builder.Database = ""; // התחברות ללא בסיס נתונים ספציפי

            using (var conn = new MySqlConnection(builder.ConnectionString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"CREATE DATABASE IF NOT EXISTS `{databaseName}` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;";
                    cmd.ExecuteNonQuery();
                }
            }

            // 2. יצירת הטבלאות בתוך מסד הנתונים
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    // טבלת תפקידים
                    cmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS roles (
                            Id INT AUTO_INCREMENT PRIMARY KEY,
                            RoleName VARCHAR(100) NOT NULL
                        );";
                    cmd.ExecuteNonQuery();

                    // טבלת משתמשים
                    cmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS users (
                            Id INT AUTO_INCREMENT PRIMARY KEY,
                            FullName VARCHAR(100) NOT NULL,
                            Email VARCHAR(150) NOT NULL UNIQUE,
                            Password VARCHAR(255) NOT NULL,
                            RoleId INT DEFAULT 1,
                            IsActive TINYINT(1) DEFAULT 1,
                            FOREIGN KEY (RoleId) REFERENCES roles(Id) ON DELETE SET NULL
                        );";
                    cmd.ExecuteNonQuery();

                    // טבלת משמרות
                    cmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS shifts (
                            Id INT AUTO_INCREMENT PRIMARY KEY,
                            ShiftName VARCHAR(100) NOT NULL,
                            StartTime TIME NOT NULL,
                            EndTime TIME NOT NULL,
                            RequiredEmployees INT DEFAULT 1,
                            IsActive TINYINT(1) DEFAULT 1
                        );";
                    cmd.ExecuteNonQuery();

                    // טבלת אילוצים וזמינות
                    cmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS constraints (
                            Id INT AUTO_INCREMENT PRIMARY KEY,
                            UserId INT NOT NULL,
                            ShiftId INT NOT NULL,
                            ConstraintDate DATE NOT NULL,
                            Reason VARCHAR(255),
                            Status VARCHAR(50) DEFAULT 'Pending',
                            FOREIGN KEY (UserId) REFERENCES users(Id) ON DELETE CASCADE,
                            FOREIGN KEY (ShiftId) REFERENCES shifts(Id) ON DELETE CASCADE
                        );";
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
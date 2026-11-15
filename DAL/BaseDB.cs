using System;
using MySql.Data.MySqlClient;

namespace DAL
{
    public abstract class BaseDB
    {
        protected readonly string connectionString;

        public BaseDB(string connectionString)
        {
            this.connectionString = connectionString;
        }
    }

    public class DatabaseInitializer
    {
        private readonly string _connectionString;

        public DatabaseInitializer(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void Initialize()
        {
            var builder = new MySqlConnectionStringBuilder(_connectionString);
            string dbName = builder.Database;
            builder.Database = ""; // מתחברים תחילה לשרת ללא בחירת DB ספציפי כדי ליצור אותו

            using (var conn = new MySqlConnection(builder.ConnectionString))
            {
                conn.Open();

                // 1. יצירת מסד הנתונים אם אינו קיים
                using (var cmd = new MySqlCommand($"CREATE DATABASE IF NOT EXISTS `{dbName}` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;", conn))
                {
                    cmd.ExecuteNonQuery();
                }

                // 2. מעבר לעבודה מול מסד הנתונים החדש
                conn.ChangeDatabase(dbName);

                // 3. יצירת הטבלאות לפי סדר התלויות
                string[] createTablesQueries = new string[]
                {
                    // טבלת תפקידים
                    @"CREATE TABLE IF NOT EXISTS Roles (
                        Id INT AUTO_INCREMENT PRIMARY KEY,
                        RoleName VARCHAR(50) NOT NULL UNIQUE
                    );",

                    // טבלת משתמשים
                    @"CREATE TABLE IF NOT EXISTS Users (
                        Id INT AUTO_INCREMENT PRIMARY KEY,
                        Username VARCHAR(50) NULL,
                        Password VARCHAR(255) NOT NULL,
                        FullName VARCHAR(100) NOT NULL,
                        Email VARCHAR(100) NOT NULL UNIQUE,
                        Phone VARCHAR(20),
                        RoleId INT NOT NULL DEFAULT 2,
                        IsActive BOOLEAN NOT NULL DEFAULT TRUE,
                        FOREIGN KEY (RoleId) REFERENCES Roles(Id)
                    );",

                    // טבלת משמרות
                    @"CREATE TABLE IF NOT EXISTS Shifts (
                        Id INT AUTO_INCREMENT PRIMARY KEY,
                        ShiftDate DATE NOT NULL,
                        StartTime TIME NOT NULL,
                        EndTime TIME NOT NULL,
                        ShiftType VARCHAR(50) NOT NULL,
                        RequiredWorkers INT NOT NULL DEFAULT 1
                    );",

                    // טבלת זמינות עובדים
                    @"CREATE TABLE IF NOT EXISTS Availability (
                        Id INT AUTO_INCREMENT PRIMARY KEY,
                        UserId INT NOT NULL,
                        AvailableDate DATE NOT NULL,
                        ShiftType VARCHAR(50) NOT NULL,
                        IsAvailable BOOLEAN NOT NULL DEFAULT TRUE,
                        FOREIGN KEY (UserId) REFERENCES Users(Id)
                    );",

                    // טבלת אילוצים
                    @"CREATE TABLE IF NOT EXISTS Constraints (
                        Id INT AUTO_INCREMENT PRIMARY KEY,
                        UserId INT NOT NULL,
                        ConstraintDate DATE NOT NULL,
                        Reason VARCHAR(255),
                        Status VARCHAR(50) DEFAULT 'Pending',
                        FOREIGN KEY (UserId) REFERENCES Users(Id)
                    );",

                    // טבלת שיבוצים למשמרות
                    @"CREATE TABLE IF NOT EXISTS ScheduleAssignments (
                        Id INT AUTO_INCREMENT PRIMARY KEY,
                        ShiftId INT NOT NULL,
                        UserId INT NOT NULL,
                        AssignedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                        FOREIGN KEY (ShiftId) REFERENCES Shifts(Id),
                        FOREIGN KEY (UserId) REFERENCES Users(Id)
                    );",

                    // הוספת תפקידי ברירת מחדל
                    @"INSERT IGNORE INTO Roles (Id, RoleName) VALUES (1, 'Admin'), (2, 'Employee');"
                };

                foreach (var query in createTablesQueries)
                {
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}
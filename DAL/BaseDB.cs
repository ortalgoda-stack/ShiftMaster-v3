using System;
using MySql.Data.MySqlClient;

namespace DAL
{
    // מחלקת בסיס עבור כל מחלקות הגישה לנתונים השומרת את מחרוזת החיבור
    public abstract class BaseDB
    {
        protected readonly string connectionString;

        public BaseDB(string connectionString)
        {
            this.connectionString = connectionString;
        }
    }

    // מחלקה האחראית על יצירה ואתחול ראשוני של מסד הנתונים והטבלאות
    public class DatabaseInitializer
    {
        private readonly string _connectionString;

        public DatabaseInitializer(string connectionString)
        {
            _connectionString = connectionString;
        }

        // פעולה היוצרת את מסד הנתונים, הטבלאות, התפקידים ומשתמשי הבדיקה אם אינם קיימים
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

                    // הוספת שלושה תפקידים למערכת: 1=מנהל מערכת, 2=עובד, 3=מנהל משמרת
                    @"INSERT IGNORE INTO Roles (Id, RoleName) VALUES (1, 'Admin'), (2, 'Employee'), (3, 'ShiftManager');"
                };

                foreach (var query in createTablesQueries)
                {
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                // 4. הוספת שלושה משתמשים התחלתיים (אחד לכל תפקיד) עם סיסמאות מגובבות
                using (var checkCmd = new MySqlCommand("SELECT COUNT(*) FROM Users;", conn))
                {
                    long userCount = Convert.ToInt64(checkCmd.ExecuteScalar() ?? 0);
                    if (userCount == 0)
                    {
                        string insertSeedUser = @"
                            INSERT INTO Users (Username, Password, FullName, Email, RoleId, IsActive)
                            VALUES (@Username, @Password, @FullName, @Email, (SELECT Id FROM Roles WHERE RoleName = @RoleName), TRUE);";

                        // משתמש בדיקה לתפקיד מנהל מערכת
                        using (var adminCmd = new MySqlCommand(insertSeedUser, conn))
                        {
                            adminCmd.Parameters.AddWithValue("@Username", "admin");
                            adminCmd.Parameters.AddWithValue("@Password", BCrypt.Net.BCrypt.HashPassword("admin"));
                            adminCmd.Parameters.AddWithValue("@FullName", "מנהל מערכת");
                            adminCmd.Parameters.AddWithValue("@Email", "admin@shiftmaster.local");
                            adminCmd.Parameters.AddWithValue("@RoleName", "Admin");
                            adminCmd.ExecuteNonQuery();
                        }

                        // משתמש בדיקה לתפקיד עובד רגיל
                        using (var userCmd = new MySqlCommand(insertSeedUser, conn))
                        {
                            userCmd.Parameters.AddWithValue("@Username", "user");
                            userCmd.Parameters.AddWithValue("@Password", BCrypt.Net.BCrypt.HashPassword("user"));
                            userCmd.Parameters.AddWithValue("@FullName", "עובד מערכת");
                            userCmd.Parameters.AddWithValue("@Email", "user@shiftmaster.local");
                            userCmd.Parameters.AddWithValue("@RoleName", "Employee");
                            userCmd.ExecuteNonQuery();
                        }

                        // משתמש בדיקה לתפקיד מנהל משמרת (אחמ"ש)
                        using (var managerCmd = new MySqlCommand(insertSeedUser, conn))
                        {
                            managerCmd.Parameters.AddWithValue("@Username", "manager");
                            managerCmd.Parameters.AddWithValue("@Password", BCrypt.Net.BCrypt.HashPassword("manager"));
                            managerCmd.Parameters.AddWithValue("@FullName", "מנהל משמרת");
                            managerCmd.Parameters.AddWithValue("@Email", "manager@shiftmaster.local");
                            managerCmd.Parameters.AddWithValue("@RoleName", "ShiftManager");
                            managerCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
    }
}
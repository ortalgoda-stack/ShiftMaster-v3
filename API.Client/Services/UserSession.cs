using Model;

namespace API.Client.Services
{
    public class UserSession
    {
        // פרטי המשתמש המחובר - קריאה מכל דף, שינוי רק דרך Login ו-Logout
        public int UserId { get; private set; }
        public string Username { get; private set; } = string.Empty;
        public string FullName { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public int RoleId { get; private set; }

        public bool IsLoggedIn => UserId > 0;

        // אירוע שמודיע לסרגל העליון ולדפים שהמשתמש המחובר השתנה
        public event Action? OnChange;

        public void Login(User user)
        {
            UserId = user.Id;
            Username = user.Username ?? string.Empty;
            FullName = user.FullName ?? string.Empty;
            Email = user.Email ?? string.Empty;
            RoleId = user.RoleId;

            OnChange?.Invoke();
        }

        public void Logout()
        {
            UserId = 0;
            Username = string.Empty;
            FullName = string.Empty;
            Email = string.Empty;
            RoleId = 0;

            OnChange?.Invoke();
        }
    }
}
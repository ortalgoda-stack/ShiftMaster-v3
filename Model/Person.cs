using System;

namespace Model
{
    public class User : BaseEntity
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int RoleId { get; set; } = 1;
        public bool IsActive { get; set; } = true;

        public User() : base() { }

        public User(int id, string username, string password, string email, string fullName, int roleId = 1, bool isActive = true)
            : base(id)
        {
            Username = username;
            Password = password;
            Email = email;
            FullName = fullName;
            RoleId = roleId;
            IsActive = isActive;
        }
    }
}
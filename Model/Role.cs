using System;
using System.Collections.Generic;
using System.Text;


namespace Model
{
    public class Role : BaseEntity
    {
        public string RoleName { get; set; } = string.Empty;

        public Role() : base() { }

        public Role(int id, string roleName) : base(id)
        {
            RoleName = roleName;
        }
    }
}
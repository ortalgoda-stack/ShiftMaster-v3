using System;

namespace Model
{
    public class Shift : BaseEntity
    {
        public string ShiftName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int RequiredEmployees { get; set; } = 1;
        public bool IsActive { get; set; } = true;

        public Shift() : base() { }

        public Shift(int id, string shiftName, DateTime startTime, DateTime endTime, int requiredEmployees = 1, bool isActive = true)
            : base(id)
        {
            ShiftName = shiftName;
            StartTime = startTime;
            EndTime = endTime;
            RequiredEmployees = requiredEmployees;
            IsActive = isActive;
        }
    }
}
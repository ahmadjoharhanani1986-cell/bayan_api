using System;

namespace SHLAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Department { get; set; }
        public int Type { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public Guid Guid { get; set; }
        public string HrCode { get; set; }
        public string Token { get; set; }
        public int Wakeel { get; set; }
    }
}
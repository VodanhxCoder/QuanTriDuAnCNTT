﻿namespace HospitalManagement.Core.Dto
{
    public class AccessToken
    {
        public string User { get; set; }
        public string FullName { get; set; }
        public string Avatar { get; set; }
        public string Token { get; set; }
        public string Role { get; set; }
    }
}
using HospitalManagement.Core.Dto;
using System;
using System.Collections.Generic;

namespace HospitalManagement.Models
{
    public class UserChatResponse
    {
        public bool IsGroup { get; set; }
        public Guid Id { get; set; }
        public string Avatar { get; set; }
        public string FullName { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public bool Gender { get; set; }
        public string Phone { get; set; }

        public List<UserDto> Users { get; set; }
    }
}
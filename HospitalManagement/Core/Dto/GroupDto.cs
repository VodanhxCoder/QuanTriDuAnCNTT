using System;
using System.Collections.Generic;

namespace HospitalManagement.Core.Dto
{
    public class GroupDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
        public string Avatar { get; set; }
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime LastActive { get; set; }

        public string CreatorName { get; set; }
        public string CreatorAvatar { get; set; }

        public List<UserDto> Users { get; set; }

        public MessageDto LastMessage { get; set; }
    }
}
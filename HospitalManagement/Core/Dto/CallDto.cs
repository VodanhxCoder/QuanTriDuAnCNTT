using System;

namespace HospitalManagement.Core.Dto
{
    public class CallDto
    {
        public int Id { get; set; }
        public string GroupCallCode { get; set; }
        public string UserCode { get; set; }
        public string Url { get; set; }
        public string Status { get; set; }
        public DateTime Created { get; set; }

        public UserDto User { get; set; }
    }
}
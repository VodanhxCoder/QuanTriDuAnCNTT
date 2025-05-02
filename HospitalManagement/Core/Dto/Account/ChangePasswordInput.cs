using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.Core.Dto.Account
{
    public class ChangePasswordInput
    {
        [Required]
        [MinLength(3)]
        public string NewPassword { get; set; }
    }
}
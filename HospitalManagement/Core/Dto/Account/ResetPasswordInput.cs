using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.Core.Dto.Account
{
    public class ResetPasswordInput
    {
        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; }

        [Required]
        [MaxLength(328)]
        public string ResetToken { get; set; }

        [Required]
        [MinLength(3)]
        public string NewPassword { get; set; }
    }
}
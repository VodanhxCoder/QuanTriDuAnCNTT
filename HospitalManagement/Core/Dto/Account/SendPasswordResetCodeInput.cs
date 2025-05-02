using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.Core.Dto.Account
{
    public class SendPasswordResetCodeInput
    {
        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string EmailAddress { get; set; }
    }
}
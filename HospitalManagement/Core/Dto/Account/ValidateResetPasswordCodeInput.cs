using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.Core.Dto.Account
{
    public class ValidateResetPasswordCodeInput
    {
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; }

        [StringLength(328)]
        public string ResetCode { get; set; }

        [StringLength(1000)]
        public string c { get; set; }
    }
}
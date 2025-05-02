using System.ComponentModel.DataAnnotations;

namespace HospitalManagement.Core.Dto.Verification
{
    public class SendVerificationEmailModel
    {
        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; }

        public string Mode { get; set; }

        public string Locale { get; set; }
    }

    public class SendVerificationByEmailInput
    {
        public string Mode { get; set; }
        public string Locale { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; }

        public string Code { get; set; }
        public string Token { get; set; }
        public string UserName { get; set; }
        public string Link { get; set; }
    }

    public class EmailVerificationModel
    {
        public string Email { get; set; }
        public string Token { get; set; }

        [MaxLength(10)]
        public string Code { get; set; }
    }

    public class VerifyUserEmailTokenInput
    {
        /// <summary>
        /// Encrypted values for {Email}, {Token}, {Mode}
        /// </summary>
        public string c { get; set; }

        public string Mode { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }

        public VerifyUserEmailTokenInput(string c)
        {
            this.c = c;
        }
    }
}
namespace HospitalManagement.Core.Dto.Verification
{
    public class VerificationOutput
    {
        public string SessionInfo { get; set; }
    }

    public class SendVerificationEmailOutputModel
    {
        public string Email { get; set; }
        public string Phone { get; set; }
        public string UserCode { get; set; }
        public string Code { get; set; }
        public bool Sent { get; set; }
        public string Message { get; set; }
    }

    public class EmailConfirmVerificationOutput
    {
        public string Email { get; set; }
        public string UserCode { get; set; }
        public bool VerifiedEmail { get; set; }
        public long UpdatedDate { get; set; }
        public string Message { get; set; }
    }
}
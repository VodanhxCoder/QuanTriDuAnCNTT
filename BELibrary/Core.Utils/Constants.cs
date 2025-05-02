namespace VChatCore
{
    public class Constants
    {
        public class GroupType
        {
            public const string SINGLE = "single";
            public const string MULTI = "multi";
        }

        public class CallStatus
        {
            public const string IN_COMMING = "IN_COMMING";
            public const string OUT_GOING = "OUT_GOING";
            public const string MISSED = "MISSED";
        }

        public const string AVATAR_DEFAULT = "/Areas/Admin/Content/img/profile-photos/1.png";

        public class EmailTemplateNameConst
        {
            public const string ChangePwdSuccess = "changepwdsuccess";

            public const string ResetPassword = "resetpassword";

            public const string VerifyEmail = "verifyemail";
        }

        public static class UserVerificationMode
        {
            public const string ForgotPassword = "FORGOT_PASSWORD";
        }

        public static class UserVerificationStatus
        {
            public const int Active = 1;
            public const int Inactive = 0;
        }

        public static class UserRole
        {
            public const string User = "USER";
            public const string Admin = "ADMIN";
        }

        public static class MessageType
        {
            public const string Text = "text";
            public const string Media = "media";
            public const string Attachment = "attachment";
        }
    }
}
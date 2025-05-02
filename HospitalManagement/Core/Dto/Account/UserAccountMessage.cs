namespace HospitalManagement.Core.Dto.Account
{
    public class UserAccountMessage
    {
        public const string EmailNotExist = "Email không tồn tại!";
        public const string PasswordResetCodeInvalid = "Mã xác thực không chính xác!";
        public const string PasswordResetCodeExpire = "Mã cài đặt mật khẩu hết hạn!";
        public const string PasswordResetTokenInvalid = "Invalid password reset token!";
        public const string WrongCurrentPassword = "Wrong current password";
    }
}
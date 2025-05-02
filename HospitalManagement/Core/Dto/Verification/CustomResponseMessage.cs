namespace HospitalManagement.Core.Dto.Verification
{
    public class CustomResponseMessage
    {
        public const string InvalidParams = "Dữ liệu không hợp lệ";
        public const string InvalidEmailOrCode = "Email hoặc code không đúng";
        public const string CodeExpired = "Mã đã hết hạn";
        public const string NotAllowed = "Không được phép";
        public const string UserDoesNotExist = "Người dùng không tồn tại";
        public const string EmailDoesNotExist = "Email không tồn tại";
        public const string TokenExpired = "Token hết hạn";
    }
}
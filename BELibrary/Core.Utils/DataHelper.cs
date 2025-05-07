using System;
using System.IO; //import FileStream
using System.Security.Cryptography; //import SHA256
using System.Text;

namespace BELibrary.Core.Utils
{
    public class DataHelper 
    {
        public static string SHA256Hash(string input) //hàm băm SHA256
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input)); //chuyển đổi chuỗi thành mảng byte
                StringBuilder builder = new StringBuilder(); //tạo một StringBuilder để lưu trữ kết quả
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2")); //chuyển đổi từng byte thành chuỗi hex 
                }
                return builder.ToString();
            }
        }

        public static void Base64ToImage(string base64String, string filePath) //hàm chuyển đổi base64 thành ảnh
        {
            var bytes = Convert.FromBase64String(base64String);
            using (var imageFile = new FileStream(filePath, FileMode.Create)) //tạo một FileStream để ghi dữ liệu vào file
            {
                imageFile.Write(bytes, 0, bytes.Length);
                imageFile.Flush();
            }
        }
    }
}
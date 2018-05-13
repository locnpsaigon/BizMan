using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace BizMan.Models
{
    public class ChangePassViewModel
    {
        public int UserId { get; set; }

        public string Username { get; set; }

        [Required(ErrorMessage = "Mật cũ không được rỗng")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "Mật mới không được rỗng")]
        [RegularExpression(@"^.*(?=.{5,})(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[_!@#$%^&+=]).*$",
            ErrorMessage = "Mật khẩu dài 5 đến 18 ký tự. Bao gồm ít nhất 1 ký tự hoa, 1 ký tự thường và 1 ký tự đặc biệt (_!@#$%^&+=)")]
        public string NewPassword { get; set; }

        [Compare("NewPassword", ErrorMessage = "Xác nhận mật khẩu và mật khẩu không trùng khớp")]
        public string ConfirmedPassword { get; set; }

        [Required(ErrorMessage = "Họ không được rỗng")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Tên không được rỗng")]
        public string LastName { get; set; }

        [DataType(DataType.EmailAddress, ErrorMessage = "Địa chỉ email chưa hợp lệ")]
        public string Email { get; set; }

    }
}
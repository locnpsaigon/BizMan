using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace BizMan.Models
{
    public class AddUserViewModel
    {
        [Required(ErrorMessage = "Tên đăng nhập không được rỗng")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được rỗng")]
        [RegularExpression(@"^.*(?=.{5,})(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[_!@#$%^&+=]).*$",
            ErrorMessage = "Mật khẩu dài 5 đến 18 ký tự. Bao gồm ít nhất 1 ký tự hoa, 1 ký tự thường và 1 ký tự đặc biệt (_!@#$%^&+=)")]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Xác nhận mật khẩu và mật khẩu không trùng khớp")]
        public string ConfirmedPassword { get; set; }

        [Required(ErrorMessage = "Họ không được rỗng")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Tên không được rỗng")]
        public string LastName { get; set; }

        [DataType(DataType.EmailAddress, ErrorMessage = "Địa chỉ email chưa hợp lệ")]
        public string Email { get; set; }

        public bool IsActive { get; set; }

        public int[] RolesId { get; set; }
    }
}
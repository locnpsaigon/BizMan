using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace BizMan.Models
{
    public class AddRoleViewModel
    {
        [Required(ErrorMessage = "Tên chức danh không được rỗng")]
        public string RoleName { get; set; }

        public string Description { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace BizMan.Models
{
    public class AddSandViewModel
    {
        [Key]
        public int SandId { get; set; }

        [Required(ErrorMessage = "Tên không được rỗng")]
        public string SandName { get; set; }

        [Required(ErrorMessage = "Nhập giá mỏ")]
        public decimal ProviderPrice { get; set; }

        [Required(ErrorMessage = "Nhập giá khách")]
        public decimal CustomerPrice { get; set; }

        [Required(ErrorMessage = "Nhập giá gia công")]
        public decimal TransportPrice { get; set; }

        public string Description { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace BizMan.Models
{
    public class EditCustomerViewModel
    {
        [Key]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Tên đơn vị không được rỗng")]
        public string CustomerName { get; set; }

        public string Description { get; set; }
    }
}
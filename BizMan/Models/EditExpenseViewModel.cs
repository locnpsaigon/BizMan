using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace BizMan.Models
{
    public class EditExpenseViewModel
    {
        [Key]
        public int ExpenseId { get; set; }

        [Required(ErrorMessage="Nhập ngày chi")]
        public string Date { get; set; }

        [Required(ErrorMessage="Nhập tên khoản chi")]
        public string Name { get; set; }

        [Required(ErrorMessage="Nhập số tiền chi")]
        public decimal Amount { get; set; }

        public string Description { get; set; }
    }
}
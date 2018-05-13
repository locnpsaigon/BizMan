using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace BizMan.DAL
{
    public class Expense
    {
        [Key]
        public int ExpenseId { get; set; }

        public DateTime Date { get; set; }

        public string Name { get; set; }

        public decimal Amount { get; set; }

        public string Description { get; set; }
    }
}
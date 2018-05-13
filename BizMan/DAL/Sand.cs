using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace BizMan.DAL
{
    public class Sand
    {
        public enum SandTypes
        {
            GoldSand = 1,
            FillingSand = 2
        }

        [Key]
        public int SandId { get; set; }

        public string SandName { get; set; }

        public decimal ProviderPrice { get; set; }

        public decimal CustomerPrice { get; set; }

        public decimal TransportPrice { get; set; }

        public string Description { get; set; }

    }
}
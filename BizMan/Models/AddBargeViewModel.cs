using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace BizMan.Models
{
    public class AddBargeViewModel
    {
        // Constructors
        public AddBargeViewModel()
        {
            this.VolumeRevenue = 0;
            this.VolumePurchaseGoldSand = 0;
            this.VolumePurchaseFillingSand = 0;
        }

        [Required(ErrorMessage = "Nhập số hiệu sà lan")]
        public string BargeCode { get; set; }
        
        [Required(ErrorMessage="Nhập số khối tính doanh thu")]
        public double VolumeRevenue { get; set; }
        
        public double VolumePurchaseGoldSand { get; set; }
        
        public double VolumePurchaseFillingSand { get; set; }

        public string Description { get; set; }
    }
}
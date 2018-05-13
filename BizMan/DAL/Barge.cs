using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace BizMan.DAL
{
    public class Barge
    {
        [Key]
        public int BargeId { get; set; }
        public string BargeCode { get; set; }
        public double VolumeRevenue { get; set; }
        public double VolumePurchaseGoldSand { get; set; }
        public double VolumePurchaseFillingSand { get; set; }
        public string Description { get; set; }
    }
}
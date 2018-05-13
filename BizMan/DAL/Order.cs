using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace BizMan.DAL
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public int CustomerId { get; set; }
        public int BargeId { get; set; }
        public int SandId { get; set; }
        public double VolumeRevenue { get; set; }
        public double VolumePromotion { get; set; }
        public double VolumePurchase { get; set; }
        public double VolumePurchaseDecrease{ get; set; }
        public decimal CustomerPrice { get; set; }
        public decimal ProviderPrice { get; set; }
        public decimal BaseTransportPrice { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace BizMan.DAL
{
    public class OrderDetails
    {
        [Key]
        public int OrderDetailsId { get; set; }
        public int OrderId { get; set; }
        public int BoatId { get; set; }
        public int TransportTimes { get; set; }
        public double ExtraVolume { get; set; }
        public double BoatVolume { get; set; }
        public decimal TransportPrice { get; set; }
    }
}
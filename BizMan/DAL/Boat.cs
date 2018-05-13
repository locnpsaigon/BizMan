using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace BizMan.DAL
{
    public class Boat
    {
        [Key]
        public int BoatId { get; set; }
        public string BoatCode { get; set; }
        public string BoatOwner { get; set; }
        public double BoatVolume { get; set; }
    }
}
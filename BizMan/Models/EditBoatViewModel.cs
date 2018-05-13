using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace BizMan.Models
{
    public class EditBoatViewModel
    {
        [Key]
        public int BoatId { get; set; }

        [Required(ErrorMessage="Số hiệu ghe không được rỗng")]
        public string BoatCode { get; set; }

        [Required(ErrorMessage = "Tên chủ ghe không được rỗng")]
        public string BoatOwner { get; set; }

        [Required(ErrorMessage = "Tổng khối ghe không được rỗng")]
        public double BoatVolume { get; set; }
    }
}
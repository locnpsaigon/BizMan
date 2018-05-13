using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace BizMan.Models
{
    public class EditOrderDetailsViewModel
    {
        public EditOrderDetailsViewModel()
        {
            this.BoatVolume = 0;
            this.TransportTimes = 0;
            this.ExtraVolume = 0;
        }

        [Key]
        public int OrderDetailsId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ghe")]
        public int BoatId { get; set; }

        [Required(ErrorMessage = "Nhập khối ghe")]
        public double BoatVolume { get; set; }

        [Required(ErrorMessage = "Số chuyến không được rỗng")]
        public int TransportTimes { get; set; }

        [Required(ErrorMessage = "Nhập khối lẻ")]
        public double ExtraVolume { get; set; }

        [Required(ErrorMessage = "Tổng khối ghe không được rỗng")]
        [Range(1, 1000, ErrorMessage = "Tổng khối ghe phải > 0")]
        public double TotalVolume { get; set; }

        [Required(ErrorMessage = "Cước vận chuyển không được rỗng")]
        public decimal TransportPrice { get; set; }

        public decimal Amount { get; set; }
    }
}
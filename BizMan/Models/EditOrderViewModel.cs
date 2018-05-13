using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace BizMan.Models
{
    public class EditOrderViewModel
    {

        public EditOrderViewModel()
        {
            this.OrderDetails = new List<EditOrderDetailsViewModel>();
        }

        [Key]
        public int OrderId { get; set; }

        [Required(ErrorMessage = "Chọn ngày")]
        public string OrderDate { get; set; }

        [Required(ErrorMessage = "Chọn đơn vị")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Chọn sà lan")]
        public int BargeId { get; set; }

        [Required(ErrorMessage = "Chọn loại cát")]
        public int SandId { get; set; }

        [Required(ErrorMessage = "Nhập khối lượng bán")]
        public double VolumeRevenue { get; set; }

        [Required(ErrorMessage = "Nhập khối lượng gia công")]
        public double VolumePromotion { get; set; }

        [Required(ErrorMessage = "Nhập khối lượng mỏ")]
        public double VolumePurchase { get; set; }

        [Required(ErrorMessage = "Nhập khối lượng tạp chất")]
        public double VolumePurchaseDecrease { get; set; }

        [Required(ErrorMessage = "Nhập giá bán")]
        public decimal CustomerPrice { get; set; }

        [Required(ErrorMessage = "Nhập giá nua")]
        public decimal ProviderPrice { get; set; }

        [Required(ErrorMessage = "Nhập tiền gia công ghe")]
        public decimal BaseTransportPrice { get; set; }

        public IList<EditOrderDetailsViewModel> OrderDetails { get; set; }
    }
}
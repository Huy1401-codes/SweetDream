using System.ComponentModel.DataAnnotations;

namespace SweetDream.Models
{
    public enum BlogStatus
    {
        [Display(Name = "Bản nháp")]
        Draft = 0,

        [Display(Name = "Xuất bản")]
        Published = 1,

        [Display(Name = "Hết hạn tạm ẩn")]
        Expired = 2
    }
}

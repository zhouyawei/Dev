using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Web;

namespace MyJD.Models
{
    [DisplayName("商品資訊")]
    [DisplayColumn("Name")]
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [DisplayName("商品类别")]
        [Required]
        public virtual ProductCategory ProductCategory { get; set; }

        [DisplayName("商品名称")]
        [Required(ErrorMessage = "请输入商品名称")]
        [MaxLength(60, ErrorMessage = "商品名称不可超过60个字")]
        public string Name { get; set; }

        [DisplayName("商品簡介")]
        [Required(ErrorMessage = "請輸入商品簡介")]
        [MaxLength(250, ErrorMessage = "商品簡介請勿輸入超過250個字")]
        public string Description { get; set; }

        [DisplayName("商品顏色")]
        [Required(ErrorMessage = "請選擇商品顏色")]
        public Color Color { get; set; }

        [DisplayName("商品售價")]
        [Required(ErrorMessage = "請輸入商品售價")]
        [Range(0, 9999999999999999999999999999999999999999999999999999999999999d, ErrorMessage = "商品售價必須介於 0 ~ 9999999999999999999999999999999999999999999999999999999999999 之間")]
        public decimal Price { get; set; }

        [DisplayName("上架時間")]
        [Description("如果不設定上架時間，代表此商品永不上架")]
        public DateTime? PublishOn { get; set; }
    }
}
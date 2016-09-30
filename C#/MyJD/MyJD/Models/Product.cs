using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Web;

namespace MyJD.Models
{
    [DisplayName("商品大全")]
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

        [DisplayName("商品简介")]
        [Required(ErrorMessage = "请输入商品简介")]
        [MaxLength(250, ErrorMessage = "商品简介请勿输入超过250个字符")]
        public string Description { get; set; }

        [DisplayName("商品颜色")]
        [Required(ErrorMessage = "请选择商品颜色")]
        public Color Color { get; set; }

        [DisplayName("商品售价")]
        [Required(ErrorMessage = "请输入商品售价")]
        [Range(0, 9999999999999999999999999999999999999999999999999999999999999d, ErrorMessage = "商品售价须介于0 ~ 9999999999999999999999999999999999999999999999999999999999999之间")]
        public decimal Price { get; set; }

        [DisplayName("上架时间")]
        [Description("如果不设置上架时间，代表此商品为下架状态")]
        public DateTime? PublishOn { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MyJD.Models
{
    [DisplayName("订单详情")]
    [DisplayColumn("DisplayName")]
    public class OrderHeader
    {
        [Key]
        public int Id { get; set; }

        [DisplayName("订购会员")]
        [Required]
        public Member Member { get; set; }

        [DisplayName("收件人姓名")]
        [Required(ErrorMessage = "请输入收件人姓名，例如: +886-2-23222480#6342")]
        [MaxLength(40, ErrorMessage = "收件人姓名长度不可超过 40 个字符")]
        [Description("订购的会员不一定就是收到商品的人")]
        public string ContactName { get; set; }

        [DisplayName("联系电话")]
        [Required(ErrorMessage = "请输入您的联系电话，例如: +886-2-23222480#6342")]
        [MaxLength(25, ErrorMessage = "电话号码长度不可超过25个字符")]
        [DataType(DataType.PhoneNumber)]
        public string ContactPhoneNO { get; set; }

        [DisplayName("运送地址")]
        [Required(ErrorMessage = "请入商品运送地址")]
        public string ContactAddress { get; set; }

        [DisplayName("订单金额")]
        [Required]
        [DataType(DataType.Currency)]
        [Description("由于订单金额可能会受商品递送方式或优惠折扣等方式引起价格波动，因此必须保留购买当时算出来的订单金额")]
        public decimal TotalPrice { get; set; }

        [DisplayName("订单备注")]
        [DataType(DataType.MultilineText)]
        public string Memo { get; set; }

        [DisplayName("订购时间")]
        public DateTime BuyOn { get; set; }

        [NotMapped]
        public string DisplayName
        {
            get { return this.Member.Name + "于" + this.BuyOn + "订购的商品"; }
        }

        public virtual ICollection<OrderDetail> OrderDetailItems { get; set; }
    }
}
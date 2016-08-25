using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MyJD.Models
{
    [DisplayName("訂單主檔")]
    [DisplayColumn("DisplayName")]
    public class OrderHeader
    {
        [Key]
        public int Id { get; set; }

        [DisplayName("訂購會員")]
        [Required]
        public Member Member { get; set; }

        [DisplayName("收件人姓名")]
        [Required(ErrorMessage = "請輸入收件人姓名，例如: +886-2-23222480#6342")]
        [MaxLength(40, ErrorMessage = "收件人姓名長度不可超過 40 個字元")]
        [Description("訂購的會員不一定就是收到商品的人")]
        public string ContactName { get; set; }

        [DisplayName("聯絡電話")]
        [Required(ErrorMessage = "請輸入您的聯絡電話，例如: +886-2-23222480#6342")]
        [MaxLength(25, ErrorMessage = "電話號碼長度不可超過 25 個字元")]
        [DataType(DataType.PhoneNumber)]
        public string ContactPhoneNO { get; set; }

        [DisplayName("遞送地址")]
        [Required(ErrorMessage = "請輸入商品遞送地址")]
        public string ContactAddress { get; set; }

        [DisplayName("訂單金額")]
        [Required]
        [DataType(DataType.Currency)]
        [Description("由於訂單金額可能會受商品遞送方式或優惠折扣等方式異動價格，因此必須保留購買當下算出來的訂單金額")]
        public decimal TotalPrice { get; set; }

        [DisplayName("訂單備註")]
        [DataType(DataType.MultilineText)]
        public string Memo { get; set; }

        [DisplayName("訂購時間")]
        public DateTime BuyOn { get; set; }

        [NotMapped]
        public string DisplayName
        {
            get { return this.Member.Name + "于" + this.BuyOn + "订购的商品"; }
        }

        public virtual ICollection<OrderDetail> OrderDetailItems { get; set; }
    }
}
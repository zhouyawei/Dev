using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MyJD.Models
{
    public class MemberLoginViewModel
    {
        [DisplayName("会员账号")]
        [DataType(DataType.EmailAddress, ErrorMessage = "请输入您的 Email 地址")]
        [Required(ErrorMessage = "请输入{0}")]
        public string Email { get; set; }

        [DisplayName("会员密码")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "请输入{0}")]
        public string Password { get; set; }
    }
}
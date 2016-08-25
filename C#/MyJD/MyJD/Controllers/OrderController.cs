using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyJD.Controllers
{
    [Authorize] //必须登录会员才能使用订单结账功能
    public class OrderController : Controller
    {
        //显示完成订单的窗体页面
        public ActionResult Complete()
        {
            return View();
        }

        //将订单信息与购物车信息写入数据库
        [HttpPost]
        public ActionResult Complete(FormCollection formCollection)
        {
            //将订单信息写入数据库

            //订单完成后必须清空现有购物车信息

            //订单完成后回到网站首页, 真实情况是不是应该跳到支付付款页面?
            RedirectToAction("Index", "Home");

            return View();
        }

    }
}
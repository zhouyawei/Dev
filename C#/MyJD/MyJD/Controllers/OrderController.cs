using MyJD.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyJD.Controllers
{
    //[Authorize] //必须登录会员才能使用订单结账功能
    public class OrderController : BaseController
    {
        //显示完成订单的窗体页面
        public ActionResult Complete()
        {
            return View();
        }

        //将订单信息与购物车信息写入数据库
        [HttpPost]
        public ActionResult Complete(OrderHeader form)
        {
            //将订单信息写入数据库
            var member = db.Members.Where(p=>p.Email == User.Identity.Name).FirstOrDefault();
            if (member == null)
            {
                return RedirectToAction("Index", "Home");
            }
            if (this.Carts.Count == 0)
            {
                return RedirectToAction("Index", "Cart");
            }
            OrderHeader orderHeader = new OrderHeader() 
            {
                Member = member,
                ContactName = form.ContactName,
                ContactAddress = form.ContactAddress,
                ContactPhoneNO = form.ContactPhoneNO,
                BuyOn = DateTime.Now,
                Memo = form.Memo,
                OrderDetailItems = new List<OrderDetail>()
            };

            decimal total_price = 0;
            foreach(var item in this.Carts)
            {
                var product = db.Products.Find(item.Product.Id);
                if (product == null)
                {
                    return RedirectToAction("Index", "Cart");
                }

                total_price += item.Product.Price * item.Amount;
                orderHeader.OrderDetailItems.Add(new OrderDetail()
                {
                    Product = product,
                    Price = product.Price,
                    Amount = item.Amount
                });

                orderHeader.TotalPrice = total_price;
                db.Orders.Add(orderHeader);
                db.SaveChanges();
            }

            //订单完成后必须清空现有购物车信息
            this.Carts.Clear();
            //订单完成后回到网站首页, 真实情况是不是应该跳到支付付款页面?
            return RedirectToAction("Index", "Home");
        }

    }
}
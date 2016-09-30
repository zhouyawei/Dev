using MyJD.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace MyJD.Controllers
{
    public class CartController : BaseController
    {
        //显示当前的购物车项目
        public ActionResult Index()
        {
            return View(this.Carts);
        }

        //添加产品项目到购物车, 如果没有传入Amount参数则默认购买数量为1
        [HttpPost]
        public ActionResult AddToCart(int productId, int amount = 1)
        {
            var product = db.Products.Find(productId);
            //验证产品是否存在
            if (product == null)
            {
                return HttpNotFound();
            }

            var existingCart = this.Carts.FirstOrDefault(p => p.Product.Id == productId);
            if (existingCart != null)
            {
                existingCart.Amount++;
            }
            else
            {
                this.Carts.Add(new Cart() { Product = product, Amount = amount });
            }

            return new HttpStatusCodeResult(HttpStatusCode.Created);
        }

        //移除购物车项目
        [HttpPost]
        public ActionResult Remove(int productId)
        {
            var existingCart = this.Carts.FirstOrDefault(p => p.Product.Id == productId);
            if (existingCart != null)
            {
                this.Carts.Remove(existingCart);
            }

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        //更新购物车中特定项目的数量
        [HttpPost]
        public ActionResult UpdateAmount(int productId, int newAmount)
        {
            var existingCart = this.Carts.FirstOrDefault(p => p.Product.Id == productId);
            if (existingCart != null)
            {
                existingCart.Amount = newAmount;
            }
            return RedirectToAction("Index", "Cart");
        }
    }
}
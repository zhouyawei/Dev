using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyJD.Controllers
{
    public class CartController : Controller
    {
        //添加产品项目到购物车, 如果没有传入Amount参数则默认购买数量为1
        [HttpPost]
        public ActionResult AddToCart(int productId, int Amount = 1)
        {
            return View();
        }

        //显示当前的购物车项目
        public ActionResult Index()
        {
            return View();
        }
    }
}
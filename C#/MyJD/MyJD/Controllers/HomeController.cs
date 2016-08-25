using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyJD.Controllers
{
    public class HomeController : Controller
    {
        //首页
        public ActionResult Index()
        {
            return View();
        }

        //商品列表
        public ActionResult ProductList(int id)
        {
            

            return View();
        }

        //商品明细
        public ActionResult ProductDetail(int id)
        {
            

            return View();
        }
    }
}
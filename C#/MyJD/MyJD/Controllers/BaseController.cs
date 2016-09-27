using MyJD.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyJD.Controllers
{
    public class BaseController : Controller
    {
        protected MyJDDBContext db = new MyJDDBContext();

        protected List<Cart> Carts
        {
            get
            {
                if (Session["Carts"] == null)
                {
                    Session["Carts"] = new List<Cart>();
                }
                return Session["Carts"] as List<Cart>;
            }
            set
            {
                Session["Carts"] = value;
            }
        }
    }
}
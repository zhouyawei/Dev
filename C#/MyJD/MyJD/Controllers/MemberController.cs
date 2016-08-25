using MyJD.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace MyJD.Controllers
{
    public class MemberController : Controller
    {
        //会员注册页面
        public ActionResult Register()
        {
            return View();
        }

        //写入会员信息
        [HttpPost]
        public ActionResult Register(Member member)
        {
            return View();
        }

        //显示会员登录页面
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            return View();
        }

        [HttpPost]
        public ActionResult Login(string email, string password, string returnUrl)
        {
            if (ValidateUser(email, password))
            {
                FormsAuthentication.SetAuthCookie(email, false);

                if (string.IsNullOrEmpty(returnUrl))
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            else
            {
                return Redirect(returnUrl);
            }

            ModelState.AddModelError("", "您输入的账号或者密码错误");
            return View();
        }

        //运行会员注销
        public ActionResult Logout()
        {
            //清楚窗体验证的Cookies
            FormsAuthentication.SignOut();

            //清除所有曾经写入过的Session信息
            Session.Clear();

            return RedirectToAction("Index", "Home");
        }

        private bool ValidateUser(string email, string password)
        {
            throw new NotImplementedException();
        }
    }
}
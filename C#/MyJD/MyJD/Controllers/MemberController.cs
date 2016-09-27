using MyJD.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace MyJD.Controllers
{
    public class MemberController : BaseController
    {
        private string pwSalt = "A1rySq1oPe2Mh784QQwG6jRAfkdPpDa90J0i";

        //会员注册页面
        public ActionResult Register()
        {
            ViewBag.Title = "会员注册";
            return View();
        }

        //写入会员信息
        [HttpPost]
        public ActionResult Register(
            [Bind(Exclude = "RegisterOn,AuthCode")]
            Member member)
        {
            var chk_member = db.Members.Where(p => p.Email == member.Email).FirstOrDefault();
            if (chk_member != null)
            {
                ModelState.AddModelError("Email", "您输入的Email已经有人注册过了");
            }

            if (ModelState.IsValid)
            {
                member.Password = FormsAuthentication.HashPasswordForStoringInConfigFile(
                    pwSalt + member.Password,
                    "SHA1");
                //会员注册时间
                member.RegisterOn = DateTime.Now;
                //会员验证码
                member.AuthCode = Guid.NewGuid().ToString();

                db.Members.Add(member);
                db.SaveChanges();

                return RedirectToAction("Index", "Home");
            }
            else
            {
                return View();
            }
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
            var hash_pw = FormsAuthentication.HashPasswordForStoringInConfigFile(
                pwSalt + password,
                "SHA1");

            var member = db.Members.Where(p => p.Email == email && p.Password == hash_pw).FirstOrDefault();

            return member != null;
        }
    }
}
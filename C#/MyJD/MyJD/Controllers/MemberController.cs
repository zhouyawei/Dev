using MyJD.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
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
                
                //保存到数据库
                db.Members.Add(member);
                db.SaveChanges();

                //发送验证
                //SendAuthCodeToMember(member);

                var routePara = new RouteValueDictionary();
                routePara.Add("userName", member.Email);
                return RedirectToAction("RegisterSuccess", routePara);

                //return RedirectToAction("Index", "Home");
            }
            else
            {
                return View();
            }
        }

        public ActionResult RegisterSuccess(string userName)
        {
            return View(userName); 
        }

        private void SendAuthCodeToMember(Member member)
        {
            string mailBody = System.IO.File.ReadAllText(Server.MapPath("~/App_Data/MemberRegisterEmailTemplate.htm"));
            mailBody = mailBody.Replace("{{Name}}", member.Name);
            mailBody = mailBody.Replace("{{RegisterOn}}", member.RegisterOn.ToString("F"));
            var auth_url = new UriBuilder(Request.Url)
            {
                Path = Url.Action("ValidateRegister", new { id = member.AuthCode}),
                Query = ""
            };
            mailBody = mailBody.Replace("{{AUTH_URL}}", auth_url.ToString());

            try
            {
                SmtpClient smtpClient = new SmtpClient("smtp.qq.com");
                smtpClient.Port = 587;
                smtpClient.Credentials = new NetworkCredential("1072817424@qq.com", "tigieejcvedwbbbh");
                smtpClient.EnableSsl = true;

                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress("1072817424@qq.com");
                mailMessage.To.Add(member.Email);
                mailMessage.Subject = "MyJD商城会员注册确认信";
                mailMessage.Body = mailBody;
                mailMessage.IsBodyHtml = true;

                smtpClient.Send(mailMessage);
            }
            catch(Exception)
            { }
        }

        public ActionResult ValidateRegister(string id)
        {
            if (string.IsNullOrEmpty(id))
                return HttpNotFound();

            var member = db.Members.Where(p => p.AuthCode == id).FirstOrDefault();
            if (member != null)
            {
                TempData["LastTempMessage"] = "会员验证成功，您可以现在登录了！";
                member.AuthCode = null;
                db.SaveChanges();
            }
            else
            {
                TempData["LastTempMessage"] = "没有找到此会员验证码，您可能已经验证过了!"; 
            }
            return RedirectToAction("Login", "Member");
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
                FormsAuthentication.SetAuthCookie(email, true);

                if (string.IsNullOrEmpty(returnUrl))
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return Redirect(returnUrl);
                }
            }
            
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

        [HttpPost]
        public ActionResult CheckDup(string email)
        {
            var member = db.Members.Where(p => p.Email == email).FirstOrDefault();

            if (member != null)
                return Json(false);
            else
                return Json(true);
        }

        private bool ValidateUser(string email, string password)
        {
            var hash_pw = FormsAuthentication.HashPasswordForStoringInConfigFile(
                pwSalt + password,
                "SHA1");

            var member = db.Members.Where(p => p.Email == email && p.Password == hash_pw).FirstOrDefault();
            if (member != null)
            {
                if (member.AuthCode == null)
                {
                    return true;
                }
                else
                {
                    ModelState.AddModelError("", "您的账号尚未激活");
                    return false;
                }
            }
            else
            {
                ModelState.AddModelError("", "您输入的账号或密码错误");
                return false;
            }
        }
    }
}
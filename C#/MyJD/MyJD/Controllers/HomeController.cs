using MyJD.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyJD.Controllers
{
    public class HomeController : BaseController
    {
        //首页
        public ActionResult Index()
        {
#if DEBUG_false
            ViewBag.Title = "首页";

            var data = new List<ProductCategory>() 
            {
                new ProductCategory() { Id = 1, Name = "文具"},
                new ProductCategory() { Id = 2, Name = "礼品"},
                new ProductCategory() { Id = 3, Name = "书籍"},
                new ProductCategory() { Id = 4, Name = "美劳用具"}
            };

            return View(data);
#else
            var data = db.ProductCategories.ToList();
            //插入演示信息（测试用）
            if (data.Count == 0)
            {
                db.ProductCategories.Add(new ProductCategory() { Id = 1, Name = "文具" });
                db.ProductCategories.Add(new ProductCategory() { Id = 2, Name = "礼品" });
                db.ProductCategories.Add(new ProductCategory() { Id = 3, Name = "书籍" });
                db.ProductCategories.Add(new ProductCategory() { Id = 4, Name = "美劳用品" });
                db.SaveChanges();
                data = db.ProductCategories.ToList();
            }

            return View(data);
#endif
        }

        //商品列表
        public ActionResult ProductList(int id)
        {
#if DEBUG_FALSE
            var productCategoty = new ProductCategory() 
            {
                Id = id,
                Name = "类别 " + id
            };

            var data = new List<Product>() 
            {
                new Product()
                {
                    Id = 1, 
                    ProductCategory  = productCategoty, 
                    Name = "原子笔",
                    Description = "圆珠笔 （Ball Point Pen），或称原子笔，是使用干稠性油墨...",
                    Price = 30,
                    PublishOn = DateTime.Now,
                    Color = Color.Black
                },
                new Product()
                {
                    Id = 2, 
                    ProductCategory  = productCategoty, 
                    Name = "铅笔",
                    Description = "铅笔（Pencil），是一种用来书写以及绘画素描专用的笔类...",
                    Price = 5,
                    PublishOn = DateTime.Now,
                    Color = Color.Black
                }
            };

            return View(data);
#else
            var productCategory = db.ProductCategories.Find(id);
            if (productCategory != null)
            {
                var data = productCategory.Products.ToList();
                if (data.Count == 0)
                {
                    productCategory.Products.Add(new Product()
                    {
                        Id = 1,
                        ProductCategory = productCategory,
                        Name = "原子笔",
                        Description = "圆珠笔 （Ball Point Pen），或称原子笔，是使用干稠性油墨...",
                        Price = 30,
                        PublishOn = DateTime.Now,
                        Color = Color.Black
                    });
                    productCategory.Products.Add(new Product()
                    {
                        Id = 2,
                        ProductCategory = productCategory,
                        Name = "铅笔",
                        Description = "铅笔（Pencil），是一种用来书写以及绘画素描专用的笔类...",
                        Price = 5,
                        PublishOn = DateTime.Now,
                        Color = Color.Black
                    });
                    db.SaveChanges();
                    data = productCategory.Products.ToList();
                }

                return View(data);
            }
            else
            {
                return HttpNotFound();
            }
#endif
        }

        //商品明细
        public ActionResult ProductDetail(int id)
        {
#if DEBUG_FALSE
            var productCategory = new ProductCategory() 
            {
                Id = 1,
                Name = "文具"
            };

            var data = new Product() 
            {
                Id = id,
                ProductCategory = productCategory,
                Name = "原子笔",
                Description = "圆珠笔 （Ball Point Pen），或称原子笔，是使用干稠性油墨，依靠笔头上自由转动的钢珠带出来转写到纸上的一种书写工具。圆珠笔具有结构简单、携带方便、书写润滑，且适宜于用来复写等优点，因而，从学校的学生到写字楼的文职人员等各界人士都乐于使用。圆珠笔是一种使用了微小旋转圆珠的笔，这种圆珠由黄铜、钢或者碳化钨制成，可在书写时将墨水释放到纸上。圆珠笔与它的前辈们——芦苇笔、羽毛笔、金属笔尖的笔和自来水笔差别很大",
                Price = 30,
                PublishOn = DateTime.Now,
                Color = Color.Black
            };

            return View(data);
        }
#else
            var data = db.Products.Find(id);
            return View(data);
#endif
        }
    }
}
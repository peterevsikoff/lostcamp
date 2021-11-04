using LostCampStore.Models.Data;
using LostCampStore.Models.ViewModels.Cart;
using LostCampStore.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LostCampStore.Controllers
{
    public class ShopController : Controller
    {
        // GET: Shop
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Pages");
        }

        public ActionResult CategoryMenuPartial()
        {
            //обьявить модель List CategoryVM
            List<CategoryVM> categoryVMList;

            //инициализировать модель данными
            using (Db db = new Db())
            {
                categoryVMList = db.Categories.ToArray().OrderBy(x => x.Sorting).Select(x => new CategoryVM(x)).ToList();
            }

            //возвращаем представление
            return PartialView("_CategoryMenuPartial", categoryVMList);
        }

        public ActionResult Category(string name)
        {
            //обьявляем список типа List
            List<ProductVM> productVMList;

            using (Db db = new Db())
            {
                //id категории
                CategoryDTO categoryDTO = db.Categories.Where(x => x.Slug == name).FirstOrDefault();
                int catId = categoryDTO.Id;

                //инициализируем список данными
                productVMList = db.Products.ToArray().Where(x => x.CategoryId == catId).Select(x => new ProductVM(x)).ToList();

                //получаем имя категории
                var productCat = db.Products.Where(x => x.CategoryId == catId).FirstOrDefault();

                //проверяем на null
                if (productCat == null)
                {
                    var catName = db.Categories.Where(x => x.Slug == name).Select(x => x.Name).FirstOrDefault();
                    ViewBag.CategoryName = catName;
                }
                else
                {
                    ViewBag.CategoryName = productCat.CategoryName;
                }
            }
            //возвращаем модель в представление
            return View(productVMList);
        }

        [ActionName("product-details")]
        public ActionResult ProductDetails(string name)
        {
            //обьявить модели dto и VM
            ProductDTO dto;
            ProductVM model;

            //инициализиркем id
            int id = 0;

            using (Db db = new Db())
            {
                //доступен ли товар
                if (!db.Products.Any(x => x.Slug.Equals(name)))
                {
                    return RedirectToAction("Index", "Shop");
                }
                //инициализируем dto данными
                dto = db.Products.Where(x => x.Slug == name).FirstOrDefault();

                //получаем id
                id = dto.Id;

                //инициализируем данными VM
                model = new ProductVM(dto);
            }
            //получить изображение из галереи
            model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                .Select(fn => Path.GetFileName(fn));

            //вернуть модель в представление
            return View("ProductDetails", model);
        }

        [HttpGet]
        public ActionResult MyOrders()
        {
            List<OrderVM> orderVM;
            string userName = User.Identity.Name;
            using (Db db = new Db())
            {
                var q = db.Users.FirstOrDefault(x => x.Username == userName);
                int userId = q.Id;
                orderVM = db.Orders.ToArray().Where(x=>x.UserId == userId).OrderByDescending(x => x.CreatedAt).Select(x => new OrderVM(x)).ToList();
            }
                return View(orderVM);
        }

        
    }
}
using LostCampStore.Models.Data;
using LostCampStore.Models.ViewModels.Shop;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace LostCampStore.Areas.Admin.Controllers
{
    public class ShopController : Controller
    {
        // GET: Admin/Shop
        public ActionResult Categories()
        {
            //объявляем модель типа List
            List<CategoryVM> categoryVMList;

            using (Db db = new Db())
            {
                //инициализируем модель данными
                categoryVMList = db.Categories
                                    .ToArray()
                                    .OrderBy(x => x.Sorting)
                                    .Select(x => new CategoryVM(x))
                                    .ToList();
            }
            //возвращаем в представление
            return View(categoryVMList);
        }

        // POST: Admin/Shop/AddNewCategory
        [HttpPost]
        public string AddNewCategory(string catName)
        {
            //объявить переменную стринг id
            string id;

            using (Db db = new Db())
            {
                //проверить имя категории на уникальность
                if (db.Categories.Any(x => x.Name == catName))
                    return "titletaken";

                //инициализируем модель dto
                CategoryDTO dto = new CategoryDTO();

                //заполняем данными
                dto.Name = catName;
                dto.Slug = catName.Replace(" ", "-").ToLower();
                dto.Sorting = 100;

                //сохраняем
                db.Categories.Add(dto);
                db.SaveChanges();

                //получить id для возврата в представление
                id = dto.Id.ToString();
            }
            //возвращаем в представление id
            return id;
        }


        //метод сортировки
        // POST: Admin/Shop/ReorderCategories
        [HttpPost]
        public void ReorderCategories(int[] id)
        {
            using (Db db = new Db())
            {
                //реализуем начальный счетчик
                int count = 1;

                //инициализируем модель данных
                CategoryDTO dto;

                //устанавливаем сортировку для каждой страницы
                foreach (var catId in id)
                {
                    dto = db.Categories.Find(catId);
                    dto.Sorting = count;
                    db.SaveChanges();
                    count++;
                }
            }
        }


        // GET: Admin/Shop/DeleteCategory/id
        public ActionResult DeleteCategory(int id)
        {
            using (Db db = new Db())
            {
                //получение категории
                CategoryDTO dto = db.Categories.Find(id);

                //удаление категории
                db.Categories.Remove(dto);

                //сохраняем изменения в бд
                db.SaveChanges();
            }
            //сообщение об успешном удалении
            TempData["SM"] = "Категория успешно удалена.";

            //переадресация 
            return RedirectToAction("Categories");
        }

        // POST: Admin/Shop/RenameCategory/id
        [HttpPost]
        public string RenameCategory(string newCatName, int id)
        {
            using (Db db = new Db())
            {
                //проверить имя на уникальность
                if (db.Categories.Any(x => x.Name == newCatName))
                    return "titletaken";

                //получаем модель dto
                CategoryDTO dto = db.Categories.Find(id);

                //редактируем модель dto
                dto.Name = newCatName;
                dto.Slug = newCatName.Replace(" ", "-").ToLower();

                //сохранить изменения
                db.SaveChanges();
            }
            //возвращаем слово
            return "ok";
        }

        //добавление товара
        //GET: Admin/Shop/AddProduct
        [HttpGet]
        public ActionResult AddProduct()
        {
            //объявить модель данных
            ProductVM model = new ProductVM();

            //добавить в модель категории
            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), dataValueField: "id", dataTextField: "Name");
            }

            //вернуть модель в представление
            return View(model);
        }

        //добавление товара
        //POST: Admin/Shop/AddProduct
        [HttpPost]
        public ActionResult AddProduct(ProductVM model, HttpPostedFileBase file)
        {
            //проверить модель на валидность
            if (!ModelState.IsValid)
            {
                using (Db db = new Db())
                {
                    model.Categories = new SelectList(db.Categories.ToList(), dataValueField:"Id", dataTextField:"Name");
                    return View(model);
                }
            }

            //проверить имя товара на уникальность
            using (Db db = new Db())
            {
                if(db.Products.Any(x=>x.Name==model.Name))
                {
                    model.Categories = new SelectList(db.Categories.ToList(), dataValueField: "Id", dataTextField: "Name");
                    ModelState.AddModelError("", "Это имя товара уже занято!");
                    return View(model);
                }
            }

            //объявлеяем переменную productid
            int id;

            //инициализируем и сохраняем в бд модель
            using (Db db = new Db())
            {
                ProductDTO product = new ProductDTO();

                product.Name = model.Name;
                product.Slug = model.Name.Replace(" ", "-").ToLower();
                product.Description = model.Description;
                product.Price = model.Price;
                product.CategoryId = model.CategoryId;

                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                product.CategoryName = catDTO.Name;

                db.Products.Add(product);
                db.SaveChanges();

                id = product.Id;
            }

            //сообщение пользователю
            TempData["SM"] = "Вы успешно добавили товар!";

            #region Upload Image
            //создаем необходимые ссылки на директории
            var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));

            var pathString1 = Path.Combine(originalDirectory.ToString(), "Products");
            var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
            var pathString3 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");
            var pathString4 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
            var pathString5 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

            //проверка наличия директории если нет то создаем
            if (!Directory.Exists(pathString1))
                Directory.CreateDirectory(pathString1);

            if (!Directory.Exists(pathString2))
                Directory.CreateDirectory(pathString2);

            if (!Directory.Exists(pathString3))
                Directory.CreateDirectory(pathString3);

            if (!Directory.Exists(pathString4))
                Directory.CreateDirectory(pathString4);

            if (!Directory.Exists(pathString5))
                Directory.CreateDirectory(pathString5);

            // проверяем был ли файл загружен
            if (file != null && file.ContentLength > 0)
            {
                //получить расширение файла
                string ext = file.ContentType.ToLower();

                    // проверяем расширение файла
                    if(ext!="image/jpg" &&
                       ext != "image/jpeg" &&
                       ext != "image/pjpeg" &&
                       ext != "image/gif" &&
                       ext != "image/x-png" &&
                       ext != "image/png")
                    {
                        using (Db db = new Db())
                        {
                            model.Categories = new SelectList(db.Categories.ToList(), dataValueField: "Id", dataTextField: "Name");
                            ModelState.AddModelError("", "Изображение не было загружено - неверный формат данных!");
                            return View(model);
                        }
                    }
            
                //объявляем переменную с именем изображения
                string imageName = file.FileName;

                //сохранить имя изображения в модель dto
                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;
                    db.SaveChanges();
                }

                //назначить пути к оригинальному и уменьшенному изображению
                var path = string.Format($"{pathString2}\\{imageName}");
                var path2 = string.Format($"{pathString3}\\{imageName}");

                //сохранить оригинальное изображение
                file.SaveAs(path);

                //создаем и сохраняем уменьшенную копию
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200).Crop(1,1);
                img.Save(path2);
            }
            #endregion

            //переадресация пользователя
            return RedirectToAction("AddProduct");
        }

        //список товаров
        //Get: Admin/Shop/Products
        [HttpGet]
        public ActionResult Products(int? page, int? catId)
        {
            //объявить List модель productVM
            List<ProductVM> listOfProductVM;

            //установить номер страницы
            var pageNumber = page ?? 1;

            //иниициализоровать лист данными
            using (Db db = new Db())
            {
                //заполняем категории для сортировки
                listOfProductVM = db.Products.ToArray()
                                    .Where(x => catId == null || catId == 0 || x.CategoryId == catId)
                                    .Select(x => new ProductVM(x))
                                    .ToList();

                ViewBag.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                //установить выбранную категорию
                ViewBag.SelectedCat = catId.ToString();
            }
            //установить постраничную навигацию
            var onePageOfProducts = listOfProductVM.ToPagedList(pageNumber, 3);//3 - сколько товаров на странице
            ViewBag.onePageOfProducts = onePageOfProducts;

            //возвращаем в представление

            return View(listOfProductVM);
        }

        //редактирование товаров
        //Get: Admin/Shop/EditProduct/id
        [HttpGet]
        public ActionResult EditProduct(int id)
        {
            //объявить модель productVM
            ProductVM model;

            using (Db db = new Db())
            {
                //получаем продукт
                ProductDTO dto = db.Products.Find(id);

                //проверить доступен ли продукт
                if (dto == null)
                {
                    return Content("Этот товар не доступен!");
                }
                //инициализируем модель данными
                model = new ProductVM(dto);

                //создать список категорий
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                //получить изображения из галереи
                model.GalleryImages = Directory
                    .EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                    .Select(fn => Path.GetFileName(fn));
            }
            //вернуть представление
            return View(model);
        }

        //редактирование товаров
        //Post: Admin/Shop/EditProduct
        [HttpPost]
        public ActionResult EditProduct(ProductVM model, HttpPostedFileBase file)
        {
            // получить id продукта
            int id = model.Id;

            //заполнить список категориями и картинками
            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }

            model.GalleryImages = Directory
                    .EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                    .Select(fn => Path.GetFileName(fn));

            //проверить модель на валидность
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //проверить имя продукта на уникальность
            using (Db db = new Db())
            {
                if(db.Products.Where(x => x.Id != id).Any(x => x.Name == model.Name))
                {
                    ModelState.AddModelError("", "Это имя товара занято!");
                    return View(model);
                }
            }

            //обновить продукт в бд
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);
                dto.Name = model.Name;
                dto.Slug = model.Name.Replace(" ", "-").ToLower();
                dto.Description = model.Description;
                dto.Price = model.Price;
                dto.CategoryId = model.CategoryId;
                dto.ImageName = model.ImageName;

                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                dto.CategoryName = catDTO.Name;

                db.SaveChanges();
            }

            //установить сообщение в tempdata
            TempData["SM"] = "Вы изменили товар!";

                //реализуем логику обработки изображений

            #region Image Upload

                //проверить загрузку файла
            if(file != null && file.ContentLength>0)
            {
                //получить расширение файла
                string ext = file.ContentType.ToLower();

                //проверить расширение файла
                if (ext != "image/jpg" &&
                   ext != "image/jpeg" &&
                   ext != "image/pjpeg" &&
                   ext != "image/gif" &&
                   ext != "image/x-png" &&
                   ext != "image/png")
                {
                    using (Db db = new Db())
                    {
                        ModelState.AddModelError("", "Изображение не было загружено - неверный формат данных!");
                        return View(model);
                    }
                }

                //установить пути для загрузки
                var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));

                var pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
                var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");

                //удаляем картинки и директории
                DirectoryInfo di1 = new DirectoryInfo(pathString1);
                DirectoryInfo di2 = new DirectoryInfo(pathString2);

                foreach (var file2 in di1.GetFiles())
                {
                    file2.Delete();
                }

                foreach (var file3 in di2.GetFiles())
                {
                    file3.Delete();
                }

                //сохранить имя изображения
                string imageName = file.FileName;
                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;
                    db.SaveChanges();
                }

                //сохранить оригинал и превью
                var path = string.Format($"{pathString1}\\{imageName}");
                var path2 = string.Format($"{pathString2}\\{imageName}");

                //сохранить оригинальное изображение
                file.SaveAs(path);

                //создаем и сохраняем уменьшенную копию
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200).Crop(1, 1);
                img.Save(path2);
            }
            #endregion

            //переадресовать пользователя
            return RedirectToAction("EditProduct");
        }

        //удаление товара
        //Post: Admin/Shop/DeleteProduct/id
        public ActionResult DeleteProduct(int id)
        {
            //удаляем товар из бд
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);
                db.Products.Remove(dto);
                db.SaveChanges();
            }

            //удалить директории изображений
            var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));
            var pathString = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());

            if (Directory.Exists(pathString))
                Directory.Delete(pathString, true);

            //переадресовываем пользователя
            return RedirectToAction("Products");
        }

        //добавление изображений в галерею
        //Post: Admin/Shop/SaveGalleryImages/id
        [HttpPost]
        public void SaveGalleryImages(int id)
        {
            //перебрать все полученные файлы из представления
            foreach (string filename in Request.Files)
            {
                //инициализировать файлы
                HttpPostedFileBase file = Request.Files[filename];

                //проверить на null
                if (file != null && file.ContentLength > 0)
                {
                    //назначить пути к директориям
                    var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));

                    string pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
                    string pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

                    //назначить пути изображений
                    var path = string.Format($"{pathString1}\\{file.FileName}");
                    var path2 = string.Format($"{pathString2}\\{file.FileName}");

                    //сохранить оригинальные и уменьшенные копии
                    file.SaveAs(path);
                    WebImage img = new WebImage(file.InputStream);
                    img.Resize(200, 200).Crop(1, 1);
                    img.Save(path2);
                }
            }
        }

        //удаление изображений из галереи
        //Post: Admin/Shop/SaveGalleryImages/id/imageName
        [HttpPost]
        public void DeleteImage(int id, string imageName)
        {
            string fullpath1 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/" + imageName);
            string fullpath2 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/Thumbs/" + imageName);

            if (System.IO.File.Exists(fullpath1))
            {
                System.IO.File.Delete(fullpath1);
            }
            if (System.IO.File.Exists(fullpath2))
            {
                System.IO.File.Delete(fullpath2);
            }
        }

    }


}
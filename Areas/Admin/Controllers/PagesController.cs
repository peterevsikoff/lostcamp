using LostCampStore.Models.Data;
using LostCampStore.Models.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LostCampStore.Areas.Admin.Controllers
{
    public class PagesController : Controller
    {
        // GET: Admin/Pages
        public ActionResult Index()
        {
            //объявляем список для представления(PageVM)
            List<PageVM> pageList;

            //заполнить список(DB)
            using (Db db = new Db())
            {
                pageList = db.Pages.ToArray().OrderBy(x => x.Sorting).Select(x => new PageVM(x)).ToList();
            }

                //вернуть в представление
                return View(pageList);
        }
        // GET: Admin/Pages/AddPage
        [HttpGet]
        public ActionResult AddPage()
        {
            return View();
        }

        // POST: Admin/Pages/AddPage
        [HttpPost]
        public ActionResult AddPage(PageVM model)
        {
            //проверка модели на валидность
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            using (Db db = new Db())
            {
                //объявляем переменную для краткого описания (slug)
                string slug;

                //инициализация класса PageDTO
                PagesDTO dto = new PagesDTO();

                //присвоить заголовок модели
                dto.Title = model.Title.ToUpper();

                //проверка есть ли описание краткое если нет, то присвоим
                if (string.IsNullOrWhiteSpace(model.Slug))
                {
                    slug = model.Title.Replace(" ", "-").ToLower();
                }
                else
                {
                    slug = model.Slug.Replace(" ", "-").ToLower();
                }

                //проверить на уникальность краткое описание и заголовок
                if (db.Pages.Any(x => x.Title == model.Title))
                {
                    ModelState.AddModelError("", "это наименование уже существует");
                    return View(model);
                }
                else if (db.Pages.Any(x => x.Slug == model.Slug))
                {
                    ModelState.AddModelError("", "это описание уже существует");
                    return View(model);
                }

                //присвоить оставшиеся значения модели
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;
                dto.Sorting = 100;

                //сохранить бд
                db.Pages.Add(dto);
                db.SaveChanges();
            }

            //передаем сообщение через tempdata
            TempData["SM"] = "Страница успешно добавлена";

            //переадресация пользователя на index
            return RedirectToAction("Index");
        }

        // GET: Admin/Pages/EditPage/id
        [HttpGet]
        public ActionResult EditPage(int id)
        {
            //объявим модель PageVM
            PageVM model;
            using (Db db = new Db())
            {
                //получить страницу по id
                PagesDTO dto = db.Pages.Find(id);

                //проверить доступна ли страница
                if (dto == null)
                {
                    return Content("Эта страница не доступна.");
                }
                //инициализация страницы данными из dto
                model = new PageVM(dto);
            }
                //возвращение представления с моделью
                return View(model);
        }
        
        // POST: Admin/Pages/EditPage
        [HttpPost]
        public ActionResult EditPage(PageVM model)
        {
            //проверить модель на валидность
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (Db db = new Db())
            {
                //получить id
                int id = model.Id;

                //объявим переменную для краткого описания
                string slug = "home";

                //получаем страницу по id
                PagesDTO dto = db.Pages.Find(id);

                //присвоить название из полученной модели в dto
                dto.Title = model.Title;

                //проверить краткое описание и присвоить его, если необходимо
                if (model.Slug != "home")
                {
                    if (string.IsNullOrWhiteSpace(model.Slug))
                    {
                        slug = model.Title.Replace(" ", "-").ToLower();
                    }
                    else
                    {
                        slug = model.Slug.Replace(" ", "-").ToLower();
                    }
                }

                //проверить краткое описание и название на уникальность
                if (db.Pages.Where(x => x.Id != id).Any(x => x.Title == model.Title))
                {
                    ModelState.AddModelError("", "Это название уже существует");
                    return View(model);
                }
                else if(db.Pages.Where(x => x.Id != id).Any(x => x.Slug == slug))
                {
                    ModelState.AddModelError("", "Это краткое описание уже существует");
                    return View(model);
                }

                //записываем остальные значения в класс dto
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;

                //сохранить в бд
                db.SaveChanges();
            }
            //дать сообщение пользователю
            TempData["SM"] = "Вы изменили страницу.";

            //переодресовать пользователя к страницам

            return RedirectToAction("EditPage");
        }

        // GET: Admin/Pages/PageDetails/id
        [HttpGet]
        public ActionResult PageDetails(int id)
        {
            //объявим модель данных Pagevw
            PageVM model;
            using (Db db = new Db())
            {
                //получить страницу
                PagesDTO dto = db.Pages.Find(id);

                //убедиться что страница доступна
                if (dto == null)
                {
                    return Content("Эта страница не доступна.");
                }

                //присвоить поля из бд
                model = new PageVM(dto);
            }
            //вернуть модель представления
            return View(model);
        }

        //метод удаления страниц
        // GET: Admin/Pages/DeletePage/id
        public ActionResult DeletePage(int id)
        {
            using (Db db = new Db())
            {
                //получение страницы
                PagesDTO dto = db.Pages.Find(id);

                //удаление страницы
                db.Pages.Remove(dto);

                //сохраняем изменения в бд
                db.SaveChanges();
            }
            //сообщение об успешном удалении
            TempData["SM"] = "Страница успешно удалена.";

                //переадресация на index
                return RedirectToAction("Index");
        }

        //метод сортировки
        // GET: Admin/Pages/ReorderPages
        [HttpPost]
        public void ReorderPages(int[] id)
        {
            using (Db db = new Db())
            {
                //реализуем начальный счетчик
                int count = 1;

                //инициализируем модель данных
                PagesDTO dto;

                //устанавливаем сортировку для каждой страницы
                foreach (var pageId in id)
                {
                    dto = db.Pages.Find(pageId);
                    dto.Sorting = count;
                    db.SaveChanges();
                    count++;
                }
            }
        }

        // GET: Admin/Pages/EditSidebar
        [HttpGet]
        public ActionResult EditSidebar()
        {
            //объявляем модель
            SidebarVM model;

            using (Db db = new Db())
            {
                //получаем данные из бд
                SidebarDTO dto = db.Sidebars.Find(1); //поменять с жесткой 1

                //заполняем модель
                model = new SidebarVM(dto);
            }
            //вернуть представление с моделью
            return View(model);
        }

        // POST: Admin/Pages/EditSidebar
        [HttpPost]
        public ActionResult EditSidebar(SidebarVM model)
        {
            using (Db db = new Db())
            {
                //получаем данные с бд
                SidebarDTO dto = db.Sidebars.Find(1); //поменять с жесткой 1

                //присвоить данные в body
                dto.Body = model.Body;

                //сохранить изменения
                db.SaveChanges();
            }
            //сообщение в tempdata
            TempData["SM"] = "Вы изменили боковую панель.";

            //переадресация пользователя
            return RedirectToAction("EditSidebar");
        }
    }
}
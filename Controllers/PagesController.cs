using LostCampStore.Models.Data;
using LostCampStore.Models.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LostCampStore.Controllers
{
    public class PagesController : Controller
    {
        // GET: Index/{page}
        public ActionResult Index(string page = "")
        {
            //получаем/ устанавливаем краткий заголовок
            if (page == "")
                page = "home";

            //обьявляем модель и данные
            PageVM model;
            PagesDTO dto;

            //проверить доступна ли текущая страница
            using (Db db = new Db())
            {
                if (!db.Pages.Any(x => x.Slug.Equals(page)))
                    return RedirectToAction("Index", new { page = ""});
            }

            //получаем контекст данных
            using (Db db = new Db())
            {
                dto = db.Pages.Where(x => x.Slug == page).FirstOrDefault();
            }

            //устанавливаем заголовок страницы
            ViewBag.PageTitle = dto.Title;

            //проверяем боковую панель sidebar
            if (dto.HasSidebar == true)
            {
                ViewBag.Sidebar = "Yes";
            }
            else
            {
                ViewBag.Sidebar = "No";
            }

            //заполняем модель данными
            model = new PageVM(dto);

                //вернуть представление
                return View(model);
        }

        public ActionResult PagesMenuPartial()
        {
            //инициализируем лист PageVM
            List<PageVM> pageVMList;

            //получить все страницы кроме home
            using (Db db = new Db())
            {
                pageVMList = db.Pages.ToArray().OrderBy(x => x.Sorting).Where(x => x.Slug != "home")
                    .Select(x => new PageVM(x)).ToList();
            }

                //возвращаем частичное представление с листом данных
                return PartialView("_PagesMenuPartial", pageVMList);
        }

        public ActionResult SidebarPartial()
        {
            //обьявить модель
            SidebarVM model;

            //инициализировать модель данными
            using (Db db = new Db())
            {
                SidebarDTO dto = db.Sidebars.Find(1);
                model = new SidebarVM(dto);
            }

                //вовращаем модель в частичное представление
                return PartialView("_SidebarPartial", model);
        }
    }

    
}
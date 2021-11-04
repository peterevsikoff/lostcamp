using LostCampStore.Models.Data;
using LostCampStore.Models.ViewModels.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace LostCampStore.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return RedirectToAction("Login");
        }
        // GET: account/create-account
        [ActionName("create-account")]
        [HttpGet]
        public ActionResult CreateAccount()
        {
            //
            return View("CreateAccount");
        }

        [ActionName("create-account")]
        [HttpPost]
        public ActionResult CreateAccount(UserVM model)
        {
            //проверить модель на валидность
            if (!ModelState.IsValid)
                return View("CreateAccount", model);

            //проверить соответствие пароля
            if (!model.Password.Equals(model.ConfirmPassword))
            {
                ModelState.AddModelError("", "Пароли не совпадают!");
                return View("CreateAccount", model);
            }

            using (Db db = new Db())
            {
                //проверить имя на уникальность
                if (db.Users.Any(x => x.Username.Equals(model.Username)))
                {
                    ModelState.AddModelError("", $"Такой пользователь {model.Username} уже существует!");
                    model.Username = "";
                    return View("CreateAccount", model);
                }

                //создать экземпляр контекста данных userdto
                UserDTO userDTO = new UserDTO()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber,
                    EmailAdress = model.EmailAdress,
                    Username = model.Username,
                    Password = model.Password
                };

                //добавить данные в экземпляр класса
                db.Users.Add(userDTO);

                //сохранить данные
                db.SaveChanges();

                //добавить роль пользователю
                int id = userDTO.Id;
                UserRoleDTO userRoleDTO = new UserRoleDTO()
                {
                    UserId = id,
                    RoleId = 2//id пользователя
                };

                db.UserRoles.Add(userRoleDTO);
                db.SaveChanges();
            }
            //сообщение в tempdata
            TempData["SM"] = "Вы теперь зарегистированы и можете войти!";

            //переадресовываем пользователя
            return RedirectToAction("Login");
        }

        [HttpGet]
        public ActionResult Login()
        {
            //подтвердить, что пользователь не авторизован
            string username = User.Identity.Name;

            if (!string.IsNullOrEmpty(username))
            {
                return RedirectToAction("user-profile");
            }

            //возвращаем представление
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        public ActionResult Login(LoginUserVM model)
        {
            // Проверяем модель на валидность
            if (!ModelState.IsValid)
                return View(model);

            // Проверяем пользователя на валидность
            bool isValid = false;

            using (Db db = new Db())
            {
                if (db.Users.Any(x => x.Username.Equals(model.Username) && x.Password.Equals(model.Password)))
                    isValid = true;

                if (!isValid)
                {
                    ModelState.AddModelError("", "Невалидный пароль или имя пользователя.");
                    return View(model);
                }
                else
                {
                    FormsAuthentication.SetAuthCookie(model.Username, model.RememberMe);
                    return Redirect(FormsAuthentication.GetRedirectUrl(model.Username, model.RememberMe));
                }
            }
        }

        [HttpGet]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }


        public ActionResult UserNavPartial()
        {
            //получить имя пользователя
            string username = User.Identity.Name;

            //объявить модель
            UserNavPartialVM model;

            using (Db db = new Db())
            {
                //получить пользователя
                UserDTO dto = db.Users.FirstOrDefault(x=>x.Username == username);

                //заполнить модель данными из dto
                model = new UserNavPartialVM()
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    PhoneNumber = dto.PhoneNumber
                };

            }
            //возвращаем частичное представление
            return PartialView("_UserNavPartial", model);
        }

        [HttpGet]
        [ActionName("user-profile")]
        public ActionResult UserProfile()
        {
            //получить имя пользователя
            string username = User.Identity.Name;

            //объявить модель
            UserProfileVM model;

            using (Db db = new Db())
            {
                //получить пользователя
                UserDTO dto = db.Users.FirstOrDefault(x => x.Username == username);

                //инициализируем модель данными
                model = new UserProfileVM(dto);
                
            }
            //возвращаем модель в представление
            return View("UserProfile", model);
        }

        [HttpPost]
        [ActionName("user-profile")]
        public ActionResult UserProfile(UserProfileVM model)
        {
            bool userNameIsChanged = false;
            //проверить модель на валидность
            if (!ModelState.IsValid)
            {
                return View("UserProfile", model);
            }

            //проверить пароль если пользователь его меняет
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                if (!model.Password.Equals(model.ConfirmPassword))
                {
                    ModelState.AddModelError("","Пароли разнятся");
                    return View("UserProfile", model);
                }
            }

            using (Db db = new Db())
            {
                //получаем имя пользователя
                string username = User.Identity.Name;

                //проверка сменилось ли имя пользователя
                if (username!=model.Username)
                {
                    username = model.Username;
                    userNameIsChanged = true;
                }

                //проверяем имя на уникальность
                if (db.Users.Where(x => x.Id != model.Id).Any(x => x.Username == username))
                {
                    ModelState.AddModelError("", $"Такое имя пользователя {model.Username} уже существует");
                    model.Username = "";
                    return View("UserProfile", model);
                }

                //изменяем контекст данных dto
                UserDTO dto = db.Users.Find(model.Id);
                dto.FirstName = model.FirstName;
                dto.LastName = model.LastName;
                dto.PhoneNumber = model.PhoneNumber;
                dto.EmailAdress = model.EmailAdress;
                dto.Username = model.Username;

                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    dto.Password = model.Password;
                }

                //сохранить изменения
                db.SaveChanges();
            }
            //установить сообщение в tempdata
            TempData["SM"] = "Профиль успешно изменен.";

            if (!userNameIsChanged)
            {
                //возвращаем представление с моделью
                return View("UserProfile", model);
            }
            else
            {
                return RedirectToAction("Logout");
            }
            
        }


    }
}
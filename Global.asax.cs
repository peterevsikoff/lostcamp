using LostCampStore.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace LostCampStore
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        //получение ролей пользователя обработка запросов аутентификации
        protected void Application_AuthenticateRequest()
        {
            //проверить что пользователь авторизован
            if (User == null)
            {
                return;
            }

            //получить имя пользователя
            string username = Context.User.Identity.Name;

            //объявить массив ролей
            string[] roles = null;

            using (Db db = new Db())
            {
                //заполнить массив ролями
                UserDTO dto = db.Users.FirstOrDefault(x => x.Username == username);

                if (dto == null)
                {
                    return;
                }

                roles = db.UserRoles.Where(x => x.UserId == dto.Id).Select(x => x.Role.Name).ToArray();
            }
            //создать объект интерфейса IPrincipal
            IIdentity userIdentity = new GenericIdentity(username);
            IPrincipal newUserObj = new GenericPrincipal(userIdentity, roles);

            //объявляем и инициализируем данными Context.User
            Context.User = newUserObj;
        }
    }
}

using LostCampStore.Areas.Admin.Models.ViewModels.Shop;
using LostCampStore.Models.Data;
using LostCampStore.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LostCampStore.Areas.Admin.Controllers
{
    public class DashboardController : Controller
    {
        // GET: Admin/Dashboard
        [HttpGet]
        public ActionResult Index()
        {
            List<OrdersAll> ordersAllVM = new List<OrdersAll>();
            using (Db db = new Db())
            {
                List<OrderDTO> o = db.Orders.OrderByDescending(x => x.CreatedAt).ToList();
                foreach (var item in o)
                {
                    OrdersAll ordersAll = new OrdersAll();
                    var userN = db.Users.FirstOrDefault(x => x.Id == item.UserId);
                    string usName = userN.Username;
                    ordersAll.OrderId = item.OrderId;
                    ordersAll.UserName = usName;
                    ordersAll.CreatedAt = item.CreatedAt;
                    ordersAllVM.Add(ordersAll);
                }
            }
            return View(ordersAllVM);
        }
        [HttpGet]
        public ActionResult ordet(int id)
        {
            List<OrderDetVMAdm> listcartvm = new List<OrderDetVMAdm>();

            string userName = User.Identity.Name;

            using (Db db = new Db())
            {
                var q = db.Users.FirstOrDefault(x => x.Username == userName);
                int userId = q.Id;

                List<OrderDetailsDTO> p = db.OrderDetails.Where(x => x.OrderId == id).ToList();
                foreach (var item in p)
                {
                    ProductDTO pname = db.Products.FirstOrDefault(x => x.Id == item.ProductId);
                    OrderDetVMAdm odvm = new OrderDetVMAdm();
                    odvm.ProductId = pname.Id;
                    odvm.ProductName = pname.Name;
                    odvm.Price = pname.Price;
                    odvm.Image = pname.ImageName;
                    odvm.Quantity = item.Quantity;
                    odvm.Total = pname.Price * item.Quantity;
                    listcartvm.Add(odvm);
                }
                decimal total = 0m;

                foreach (var item in listcartvm)
                {
                    total += item.Total;
                }
                ViewBag.GrandTotal = total;
            }
            return View(listcartvm);
        }
    }
}
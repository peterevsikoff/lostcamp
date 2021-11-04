using LostCampStore.Models.Data;
using LostCampStore.Models.ViewModels.Cart;
using LostCampStore.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace LostCampStore.Controllers
{
    public class CartController : Controller
    {
        // GET: Cart
        public ActionResult Index()
        {
            //объявить лист картВМ
            var cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            //проверить не пустая ли корзина
            if (cart.Count == 0 || Session["cart"] == null)
            {
                ViewBag.Message = "Ваша корзина пуста.";
                return View();
            }

            //если не пуста то складываем сумму и записываем во viewbag
            decimal total = 0m;

            foreach (var item in cart)
            {
                total += item.Total;
            }
            ViewBag.GrandTotal = total;

            //вернуть лист в представление
            return View(cart);
        }

        public ActionResult CartPartial()
        {
            //обьявить модель CartVM
            CartVM model = new CartVM();

            //обьявляем переменную количества
            int qty = 0;

            //обьявить переменную цены
            decimal price = 0m;

            //проверить сессию, есть ли данные
            if (Session["cart"] != null)
            {
                //Получить общее количество товаров и цену
                var list = (List<CartVM>)Session["cart"];

                foreach (var item in list)
                {
                    qty += item.Quantity;
                    price += item.Quantity * item.Price;
                }
                model.Quantity = qty;
                model.Price = price;
            }
            else
            {
                //если корзина пустая устанавливаем количество и цену 0
                model.Quantity = 0;
                model.Price = 0m;
            }
            //вернуть частичное представление с моделью
            return PartialView("_CartPartial", model);
        }

        public ActionResult AddToCartPartial(int id)
        {
            //объявить лист типа cartvm
            List<CartVM> cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            //объявить модель cartvm
            CartVM model = new CartVM();

            using (Db db = new Db())
            {
                //получить продукт по id
                ProductDTO product = db.Products.Find(id);

                //проверить есть ли такой товар уже в корзине
                var productInCart = cart.FirstOrDefault(x => x.ProductId == id);

                //есть ли нет, то добавляем этот товар
                if (productInCart == null)
                {
                    cart.Add(new CartVM()
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Quantity = 1,
                        Price = product.Price,
                        Image = product.ImageName
                    }
                        );
                }
                //ести ли есть, то добавить еще один
                else
                {
                    productInCart.Quantity++;
                }
            }
            //получить общее количество, цену и добавить модель
            int qty = 0;
            decimal price = 0m;

            foreach (var item in cart)
            {
                qty += item.Quantity;
                price += item.Quantity * item.Price;
            }
            model.Quantity = qty;
            model.Price = price;

            //сохранить состояние корзины в сессию
            Session["cart"] = cart;

            //вернуть частичное представоение с моделью
            return PartialView("_AddToCartPartial", model);
        }

        public JsonResult IncrementProduct(int productId)
        {
            //объявить лист cart
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                //получить модель cartvm из листа
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                //добавить количество
                model.Quantity++;

                //сохранить данные
                var result = new { qty = model.Quantity, price = model.Price };

                //вернуть json ответ с данными
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult DecrementProduct(int productId)
        {
            //объявить лист cart
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                //получить модель cartvm из листа
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                //отнимаем количество
                if (model.Quantity > 1)
                {
                    model.Quantity--;
                }
                else
                {
                    model.Quantity = 0;
                    cart.Remove(model);
                }

                //сохранить данные
                var result = new { qty = model.Quantity, price = model.Price };

                //вернуть json ответ с данными
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public void RemoveProduct(int productId)
        {
            //объявить лист cart
            List<CartVM> cart = Session["cart"] as List<CartVM>;


            using (Db db = new Db())
            {
                //получить модель cartvm из листа
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                cart.Remove(model);
            }
        }

        public ActionResult PaypalPartial()
        {
            //получить лист товаров в корзине
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            //вернуть чатичное представление с листом
            return PartialView(cart);
        }

        [HttpPost]
        public void PlaceOrder()
        {
            //получить лист товаров
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            //получить имя пользователя
            string userName = User.Identity.Name;

            //объявить переменную orderid
            int orderId = 0;
            DateTime dateOrder;

            using (Db db = new Db())
            {
                //объявить модель orderDTO
                OrderDTO orderDto = new OrderDTO();

                //получить id пользователя
                var q = db.Users.FirstOrDefault(x=>x.Username == userName);
                int userId = q.Id;

                //заполнить модель orderDTO данными и сохранить
                orderDto.UserId = userId;
                orderDto.CreatedAt = DateTime.Now;
                dateOrder = orderDto.CreatedAt;
                db.Orders.Add(orderDto);
                db.SaveChanges();

                //получить id заказа
                orderId = orderDto.OrderId;

                //объявляем модель ordersdetailsdto 
                OrderDetailsDTO orderDetailsDTO = new OrderDetailsDTO();

                //добавляем данные
                foreach (var item in cart)
                {
                    orderDetailsDTO.OrderId = orderId;
                    orderDetailsDTO.UserId = userId;
                    orderDetailsDTO.ProductId = item.ProductId;
                    orderDetailsDTO.Quantity = item.Quantity;

                    db.OrderDetails.Add(orderDetailsDTO);
                    db.SaveChanges();
                }
            }
            //отправить письмо на почту админа

            //текст письма
            string firstname = "";
            string lastname = "";
            string phonenumber = "";
            string email = "";
            string product = "";
            decimal tot = 0m;

            List<OrderDetailsDTO> p;
            using (Db db = new Db())
            {
                var q = db.Users.FirstOrDefault(x => x.Username == userName);
                firstname = q.FirstName;
                lastname = q.LastName;
                email = q.EmailAdress;
                phonenumber = q.PhoneNumber;
                
                p = db.OrderDetails.Where(x=>x.OrderId == orderId).ToList();
                foreach (var item in p)
                {
                    ProductDTO pname = db.Products.FirstOrDefault(x => x.Id == item.ProductId);
                    product += "<h4> Наименование: " +pname.Name + "<br/>Цена: " + pname.Price + " BYN" + "<br/>Количество: "
                        + item.Quantity.ToString() + "<br/>Сумма: " + (pname.Price * item.Quantity).ToString() + " BYN" + "</h4>";
                    tot += pname.Price * item.Quantity;
                }
            }
            
            string text_letter = "<h2>Новый заказ от пользователя " + userName + " (" + firstname + " " + lastname + " " +
                "Email: " + email + " Телефон: " + phonenumber + ").</h2>" + "<h3>Заказ № " + orderId.ToString() + "</h3>" + "<h4>Товары: " + product + "</h4>" 
                + "<hr/><h3> Общая сумма: " + tot.ToString() +" BYN" + "</h3>" + "<h2>Дата заказа: " + dateOrder.ToString();


            MailAddress from = new MailAddress("lostcamp2020@gmail.com", "lostcamp");//lostcamp_2203 lostcamp2020@gmail.com
            MailAddress to = new MailAddress("shoplostcamp@yandex.by");
            MailMessage m = new MailMessage(from, to);
            m.Subject = "Покупки";
            m.Body = text_letter;
            m.IsBodyHtml = true;

            SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential("lostcamp2020@gmail.com", "lostcamp_2203");
            smtp.EnableSsl = true;
            
            smtp.Send(m);



            //var client = new SmtpClient("smtp.mailtrap.io", 2525)
            //{
            //    Credentials = new NetworkCredential("f34e586e250b75", "1dc89255682738"),
            //    EnableSsl = true
            //};
            //client.Send("shop@example.com", "admin@example.com", "New order", $"You have a new order. Order number: {orderId}");

            //обнулить сессию
            Session["cart"] = null;
        }

        [HttpGet]
        public ActionResult myorderdet(int id)
        {
            List<OrderDetVM> listcartvm = new List<OrderDetVM>();

            string userName = User.Identity.Name;

            using (Db db = new Db())
            {
                var q = db.Users.FirstOrDefault(x => x.Username == userName);
                int userId = q.Id;

                List<OrderDetailsDTO> p = db.OrderDetails.Where(x => x.OrderId == id).ToList();
                foreach (var item in p)
                {
                    ProductDTO pname = db.Products.FirstOrDefault(x => x.Id == item.ProductId);
                    OrderDetVM odvm = new OrderDetVM();
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
using LostCampStore.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LostCampStore.Models.ViewModels.Shop
{
    public class OrderDetVM
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public int Quantity { get; set; }

        public decimal Price { get; set; }

        public decimal Total { get; set; }
       
        public string Image { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LostCampStore.Areas.Admin.Models.ViewModels.Shop
{
    public class OrdersAll
    {
        public int OrderId { get; set; }

        public string UserName { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
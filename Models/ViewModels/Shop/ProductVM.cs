using LostCampStore.Models.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LostCampStore.Models.ViewModels.Shop
{
    public class ProductVM
    {
        public ProductVM()
        { }

        public ProductVM(ProductDTO row)
        {
            Id = row.Id;
            Name = row.Name;
            Slug = row.Slug;
            Description = row.Description;
            Price = row.Price;
            CategoryName = row.CategoryName;
            CategoryId = row.CategoryId;
            ImageName = row.ImageName;
        }


        public int Id { get; set; }
        [Required]
        [Display(Name = "Наименование")]
        public string Name { get; set; }

        public string Slug { get; set; }
        [Required]
        [Display(Name = "Описание")]
        public string Description { get; set; }
        [Display(Name = "Цена")]
        public decimal Price { get; set; }
        [Display(Name = "Категория")]
        public string CategoryName { get; set; }
        [Required]
        [DisplayName("Категория")]
        public int CategoryId { get; set; }

        [DisplayName("Картинка")]
        public string ImageName { get; set; }

        public IEnumerable<SelectListItem> Categories { get; set; }

        public IEnumerable<string> GalleryImages { get; set; }
    }
}
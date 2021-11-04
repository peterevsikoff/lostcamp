using LostCampStore.Models.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LostCampStore.Models.ViewModels.Shop
{
    public class CategoryVM
    {
        public CategoryVM()
        {
        }

        public CategoryVM(CategoryDTO row)
        {
            Id = row.Id;
            Name = row.Name;
            Slug = row.Slug;
            Sorting = row.Sorting;
        }

        public int Id { get; set; }
        [Display(Name = "Наименование")]
        public string Name { get; set; }

        public string Slug { get; set; }

        public int Sorting { get; set; }
    }
}
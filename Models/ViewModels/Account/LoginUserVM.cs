using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LostCampStore.Models.ViewModels.Account
{
    public class LoginUserVM
    {
        [Required]
        [DisplayName("Имя пользователя")]
        public string Username { get; set; }

        [Required]
        [DisplayName("Пароль")]
        public string Password { get; set; }

        [DisplayName("Запомнить пароль")]
        public bool RememberMe { get; set; }
    }
}
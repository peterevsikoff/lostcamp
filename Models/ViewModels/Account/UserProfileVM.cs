using LostCampStore.Models.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LostCampStore.Models.ViewModels.Account
{
    public class UserProfileVM
    {
        public UserProfileVM()
        {
        }

        public UserProfileVM(UserDTO row)
        {
            Id = row.Id;
            FirstName = row.FirstName;
            LastName = row.LastName;
            PhoneNumber = row.PhoneNumber;
            EmailAdress = row.EmailAdress;
            Username = row.Username;
            Password = row.Password;
        }

        public int Id { get; set; }

        [Required]
        [DisplayName("Фамилия")]
        public string FirstName { get; set; }

        [Required]
        [DisplayName("Имя")]
        public string LastName { get; set; }

        [Required]
        [DisplayName("Номер телефона")]
        [DataType(DataType.PhoneNumber)]
        //[RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Неправильный формат номера.")]
        public string PhoneNumber { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [DisplayName("Адрес электронной почты")]
        public string EmailAdress { get; set; }

        [Required]
        [DisplayName("Логин")]
        public string Username { get; set; }

        
        [DisplayName("Пароль")]
        public string Password { get; set; }

        
        [DisplayName("Подтвердите пароль")]
        public string ConfirmPassword { get; set; }
    }
}
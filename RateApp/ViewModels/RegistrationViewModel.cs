using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace RateApp.ViewModels
{
    public class RegistrationViewModel
    {

        public string UserName { get; set; }
        [Required(ErrorMessage = "Sähköpostiosoite on pakollinen!")]

        public string Email { get; set; }
        [Required(ErrorMessage = "Salasana on pakollinen!")]

        public string PasswordHash { get; set; }
        [Required(ErrorMessage = "Puhelinnumero on pakollinen!")]

        public string PhoneNumber { get; set; }
        [Required(ErrorMessage = "Osoite on pakollinen!")]

        public string Address { get; set; }
        [Required(ErrorMessage = "Postitoimipaikka on pakollinen!")]

        public string City { get; set; }
        [Required(ErrorMessage = "Maa on pakollinen!")]

        public string Country { get; set; }

        public bool IsSupplier { get; set; }
        [Required(ErrorMessage = "Etunimi on pakollinen!")]

        public string FirstName { get; set; }
        [Required(ErrorMessage = "Sukunimi on pakollinen!")]

        public string LastName { get; set; }
    }
}
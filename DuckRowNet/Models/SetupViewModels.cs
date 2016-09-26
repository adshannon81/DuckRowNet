using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace DuckRowNet.Models
{

    public class SetupRegisterViewModel
    {

        [Required]
        [Display(Name = "Company")]
        public string Company { get; set; }

        [Required]
        [Display(Name = "Subscription")]
        public string Subscription { get; set; }


    }

    public class SetupCompanyViewModel
    {
        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Logo")]
        public string Logo { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Phone")]
        public string Phone { get; set; }

        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Address")]
        public string Address1 { get; set; }

        [Display(Name = "Address2")]
        public string Address2 { get; set; }

        [Display(Name = "City")]
        public string City { get; set; }

        [Display(Name = "County")]
        public string County { get; set; }

        [Display(Name = "Country")]
        public string Country { get; set; }

        [Display(Name = "Website URL")]
        public string Website { get; set; }

        [Display(Name = "Facebook URL")]
        public string Facebook { get; set; }

        [Display(Name = "Paypal Email")]
        public string Paypal { get; set; }



    }
}
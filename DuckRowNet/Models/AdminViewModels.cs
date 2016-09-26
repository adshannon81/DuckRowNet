using System;
using System.ComponentModel.DataAnnotations;

namespace DuckRowNet.Models
{
    public class AdminViewModels
    {
    }

    public class CourseModel
    {
        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Category")]
        public string Category { get; set; }

        [Required]
        [Display(Name = "SubCategory")]
        public string SubCategory { get; set; }

        [Display(Name = "Image")]
        public string Image { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "StartDate")]
        public DateTime StartDate { get; set; }

        [Display(Name = "EndDate")]
        public DateTime EndDate { get; set; }

        [Display(Name = "Duration")]
        public double Duration { get; set; }

        [Display(Name = "Repeated")]
        public double Repeated { get; set; }


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

    public class LocationModel
    {
        [Display(Name = "LocationID")]
        public string LocationID { get; set; }

        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Longitude")]
        public string Longitude { get; set; }

        [Required]
        [Display(Name = "Latitude")]
        public string Latitude { get; set; }

        [Required]
        [Display(Name = "Zoom")]
        public string Zoom { get; set; }

        [Required]
        [Display(Name = "Address1")]
        public string Address1 { get; set; }

        [Required]
        [Display(Name = "Address2")]
        public string Address2 { get; set; }

        [Required]
        [Display(Name = "Address3")]
        public string Address3 { get; set; }

        [Required]
        [Display(Name = "Company")]
        public string Company { get; set; }

        [Required]
        [Display(Name = "City")]
        public string City { get; set; }

        [Required]
        [Display(Name = "State")]
        public string State { get; set; }

        [Required]
        [Display(Name = "Country")]
        public string Country { get; set; }

        [Required]
        [Display(Name = "Postcode")]
        public string Postcode { get; set; }
        
        [Display(Name = "IncludeSearch")]
        public bool IncludeSearch { get; set; }


    }
}
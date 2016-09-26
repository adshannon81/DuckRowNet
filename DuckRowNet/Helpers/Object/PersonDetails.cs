using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DuckRowNet.Helpers.Object
{
    public class PersonDetails
    {
        public string ID { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        public string CompanyID { get; set; }
        public string CompanyName { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Postcode { get; set; }
        public string Country { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

        public Functions.PersonType Type { get; set; }



        public PersonDetails()
        {
            //Default
            FirstName = "";
            LastName = "";
            CompanyID = "";
            CompanyName = "";
            Address1 = "";
            Address2 = "";
            City = "";
            State = "";
            Postcode = "";
            Country = "IE";
            Phone = "";
            Email = "";
            Type = Functions.PersonType.Client;
        }

        public PersonDetails(string id, string firstName, string lastName, string companyID, string companyName, string address1, string address2, string address3,
            string city, string state, string postcode, string country, string phone, string email, Functions.PersonType type)
        {
            ID = id;
            FirstName = firstName;
            LastName = lastName;
            CompanyID = companyID;
            CompanyName = companyName;
            Address1 = address1;
            Address2 = address2;
            Address3 = address3;
            City = city;
            State = state;
            Postcode = postcode;
            Country = "IE";
            Phone = phone;
            Email = email;
            Type = type;
        }

        public void GetDetails(string companyName = "")
        {
            DAL db = new DAL();

            PersonDetails p = db.getClientDetails(this);

            this.FirstName = p.FirstName;
            this.LastName = p.LastName;
            this.Email = p.Email;
            this.Phone = p.Phone;
            this.Address1 = p.Address1;
            this.Address2 = p.Address2;
            this.Address3 = p.Address3;
            this.City = p.City;
            this.State = p.State;
            this.Country = p.Country;
            this.Postcode = p.Postcode;
            this.CompanyID = p.CompanyID;
            this.CompanyName = p.CompanyName;
            this.Type = p.Type;

        }

    }
}
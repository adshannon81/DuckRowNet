using DuckRowNet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;

namespace DuckRowNet.Helpers.Object
{
    public class CompanyDetails
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string URL { get; set; }
        public string ImagePath { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Postcode { get; set; }
        public string Country { get; set; }
        public string FaceBookURL { get; set; }
        public string PaypalEmail { get; set; }
        public bool PaypalAbsorbFees { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Style { get; set; }
        public int TypeID { get; set; }
        public SubscriptionType SubType { get; set; }
        public DateTime Subscription { get; set; }
        public bool IsClub { get; set; }

        public CompanyDetails()
        { }

        public CompanyDetails(string companyName)
        {
            //Default
            Name = companyName;
            Description = "";
            URL = "";
            ImagePath = "";
            Address1 = "";
            Address2 = "";
            City = "";
            State = "";
            Postcode = "";
            Country = "IE";
            FaceBookURL = "";
            PaypalEmail = "";
            PaypalAbsorbFees = true;
            Phone = "";
            Email = "";
            Style = "Default";
            TypeID = 1;
            IsClub = false;
            SubType = new SubscriptionType();
        }

        public CompanyDetails(CompanyDetails company_original)
        {
            UpdateCompanyDetails(company_original);

        }

        public CompanyDetails(Guid id, string name, string description, string url, string imagePath, string address1, string address2,
            string city, string state, string postcode, string country, string faceBookURL, string paypalEmail, bool paypalAbsorbFees,
            string phone, string email, string style, int typeID, bool isClub, SubscriptionType subType, DateTime subscription
        )
        {
            ID = id;
            Name = name;
            Description = description;
            URL = url;
            ImagePath = imagePath;
            Address1 = address1;
            Address2 = address2;
            City = city;
            State = state;
            Postcode = postcode;
            Country = country;
            FaceBookURL = faceBookURL;
            PaypalEmail = paypalEmail;
            PaypalAbsorbFees = paypalAbsorbFees;
            Phone = phone;
            Email = email;
            Style = style;
            TypeID = typeID;
            IsClub = isClub;
            SubType = subType;
            Subscription = subscription;
        }



        public void UpdateCompanyDetails(CompanyDetails company_original)
        {
            //Default
            Name = company_original.Name;
            Description = company_original.Description;
            URL = company_original.URL;
            ImagePath = company_original.ImagePath;
            Address1 = company_original.Address1;
            Address2 = company_original.Address2;
            City = company_original.City;
            State = company_original.State;
            Postcode = company_original.Postcode;
            Country = company_original.Country;
            FaceBookURL = company_original.FaceBookURL;
            PaypalEmail = company_original.PaypalEmail;
            PaypalAbsorbFees = company_original.PaypalAbsorbFees;
            Phone = company_original.Phone;
            Email = company_original.Email;
            Style = company_original.Style;
            TypeID = company_original.TypeID;
            SubType = company_original.SubType;
            IsClub = company_original.IsClub;

            Subscription = company_original.Subscription;

        }

        public void getCompanyDetails()
        {
            DAL db = new DAL();
            UpdateCompanyDetails(db.getCompanyDetails(this.Name));


        }

        public bool Update()
        {
            DAL db = new DAL();
            bool success = db.updateCompany(this);
            if (success)
            {
                this.ID = db.getCompanyID(this.Name);

                //create Advert
                GroupClass advert = new GroupClass(
                    this.ID,
                    this.Name,
                    this.Description,
                    this.ImagePath,
                    this.Email,
                    this.Phone,
                    this.Name,
                    this.URL,
                    "6",
                    "Advert",
                    "",
                    "Advert",
                    "",
                    0,
                    this.Address1,
                    this.Address2,
                    this.City,
                    this.State,
                    DateTime.Now,
                    this.Name,
                    HttpContext.Current.User.Identity.GetUserId(),
                    "",
                    true
                );

                advert.CompanyID = this.ID;                
                advert.IsActive = true;
                advert.CreateAdvert();


            }
            return success;
        }

        public void RemoveUsers()
        {
            DAL db = new DAL();
            int max = db.getMaximumUsers(this.Name);
            var users = db.getUsers(this);

            int count = 1;
            foreach (var user in users)
            {
                if (String.Compare(user.OwnerOf, this.Name, true) != 0)
                {
                    if (count > max)
                    {
                        //delete this userrole
                        db.deleteRoles(user.UserId, this.Name);
                    }
                    count++;
                }
            }
        }

        public List<LocationModel> GetCompanyLocations()
        {
            List<LocationModel> locations = new List<LocationModel>();

            DAL db = new DAL();

            var l = db.getLocationList(this.Name);

            foreach(var location in l)
            {
                LocationModel lm = new LocationModel();
                lm.LocationID = location.ID.ToString();
                lm.Name = location.Name;
                lm.Longitude = location.Longitude;
                lm.Latitude = location.Latitude;
                lm.Address1 = location.Address1;
                lm.Address2 = location.Address2;
                lm.Address3 = location.Address3;
                lm.Company = location.Company;
                lm.City = location.City;
                lm.State = location.State;
                lm.Postcode = location.PostCode;
                locations.Add(lm);
            }


            return locations;
        }
    }
}
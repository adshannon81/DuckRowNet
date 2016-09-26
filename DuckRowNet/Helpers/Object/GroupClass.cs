using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace DuckRowNet.Helpers.Object
{
    public class GroupClass
    {

        public Guid ID { get; set; }
        public string Slug { get; set; }
        public string Company { get; set; }
        public Guid CompanyID { get; set; }
        public string CompanyImage { get; set; }
        public string Name { get; set; }
        //public string Type { get; set; }
        //public string TypeID { get; set; }
        //public string Level { get; set; }
        //public string LevelID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Time { get; set; }
        public string Minute { get; set; }
        public string Hour { get; set; }
        public int Duration { get; set; }

        public Functions.Repeat Repeated { get; set; }
        public int RepeatFrequency { get; set; }
        public string RepeatDays { get; set; }

        public int NumberOfLessons { get; set; }
        public int MaxCapacity { get; set; }
        public double CostOfCourse { get; set; }
        public double CostOfSession { get; set; }
        public string[] AdminIDList { get; set; }
        public string AdminName { get; set; }
        public int LocationID { get; set; }
        public string LocationName { get; set; }

        public string LocationLat { get; set; }
        public string LocationLng { get; set; }
        public bool IsPrivate { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        //for appointment
        public string ClientID { get; set; }
        public string ClientName { get; set; }
        public string ClientEmail { get; set; }
        public string ClientType { get; set; }

        public bool IsCourse { get; set; }
        public bool IsAdvert { get; set; }

        public List<DateTime> ClassDates { get; set; }
        public List<int> RemainingCapacityList { get; set; }  //corressponds to classdates
        public int RemainingCapacity { get; set; } //no dates
        public bool AllowDropIn { get; set; }

        public bool AllowReservation { get; set; }
        public bool AutoReservation { get; set; }
        public bool AllowPayment { get; set; }
        public bool AbsorbFee { get; set; }

        public bool Favourite { get; set; }

        //used for advert
        public string UserID { get; set; }
        public string BillingID { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ContactName { get; set; }
        public string Website { get; set; }

        public string CategoryID { get; set; }
        public string CategoryName { get; set; }
        public string SubCategoryID { get; set; }
        public string SubCategoryName { get; set; }


        public string AdvertType { get; set; }
        public bool IsActive { get; set; }
        public double Cost { get; set; }

        public string ClassType { get; set; }

        public DateTime CreatedDate { get; set; }

        public GroupClass()
        {
            //Default
            ID = Guid.NewGuid();
            Slug = "";
            Name = "";
            CategoryID = "";
            CategoryName = "";
            SubCategoryID = "";
            SubCategoryName = "";
            //Level = "";
            StartDate = Convert.ToDateTime(DateTime.Now.AddHours(1).ToString("yyyy-MM-dd HH:00:00"));
            EndDate = Convert.ToDateTime(DateTime.Now.AddHours(2).ToString("yyyy-MM-dd HH:00:00"));
            Time = "";
            Minute = StartDate.ToString("mm");
            Hour = StartDate.ToString("HH");
            Duration = 60;
            Repeated = Functions.Repeat.never;
            RepeatFrequency = 1;
            RepeatDays = "";
            NumberOfLessons = 1;
            MaxCapacity = 12;
            RemainingCapacity = 12;
            CostOfCourse = 80;
            CostOfSession = 10;
            AdminIDList = new string[10];
            AdminName = "";
            //AdminList = "";
            LocationID = 0;
            LocationName = "";
            State = "";
            IsPrivate = false;
            Description = "";
            Image = "";
            ClientID = "";
            ClientName = "";
            ClientEmail = "";
            ClientType = "";
            IsCourse = true;
            AllowDropIn = false;
            AbsorbFee = false;
            AllowReservation = false;
            AutoReservation = false;
            AllowPayment = false;
            ClassType = "Class";
            Favourite = false;

            updateSlug();
        }

        public GroupClass(GroupClass gClass_original)
        {
            //Default
            ID = gClass_original.ID;
            Slug = gClass_original.Slug;
            Name = gClass_original.Name;
            Company = gClass_original.Company;
            CompanyImage = gClass_original.CompanyImage;
            CategoryID = gClass_original.CategoryID;
            CategoryName = gClass_original.CategoryName;
            SubCategoryID = gClass_original.SubCategoryID;
            SubCategoryName = gClass_original.SubCategoryName;
            //LevelID = gClass_original.LevelID;
            //Level = gClass_original.Level;
            StartDate = gClass_original.StartDate;
            EndDate = gClass_original.EndDate;
            Time = gClass_original.Time;
            Minute = gClass_original.Minute;
            Hour = gClass_original.Hour;
            Duration = gClass_original.Duration;

            Repeated = gClass_original.Repeated;
            RepeatFrequency = gClass_original.RepeatFrequency;
            RepeatDays = gClass_original.RepeatDays;

            NumberOfLessons = gClass_original.NumberOfLessons;
            MaxCapacity = gClass_original.MaxCapacity;
            CostOfCourse = gClass_original.CostOfCourse;
            CostOfSession = gClass_original.CostOfSession;
            AdminIDList = gClass_original.AdminIDList;
            AdminName = gClass_original.AdminName;
            //AdminList = gClass_original.AdminList;
            LocationID = gClass_original.LocationID;
            LocationName = gClass_original.LocationName;
            State = gClass_original.State;
            IsPrivate = gClass_original.IsPrivate;
            Description = gClass_original.Description;
            Image = gClass_original.Image;

            ClientID = gClass_original.ClientID;
            ClientName = gClass_original.ClientName;
            ClientEmail = gClass_original.ClientEmail;
            ClientType = gClass_original.ClientType;

            ClassDates = gClass_original.ClassDates;
            RemainingCapacity = gClass_original.RemainingCapacity;
            RemainingCapacityList = gClass_original.RemainingCapacityList;
            AllowDropIn = gClass_original.AllowDropIn;
            IsCourse = gClass_original.IsCourse;

            AbsorbFee = gClass_original.AbsorbFee;
            AllowReservation = gClass_original.AllowReservation;
            AutoReservation = gClass_original.AutoReservation;
            AllowPayment = gClass_original.AllowPayment;

            ClassType = gClass_original.ClassType;

            Favourite = gClass_original.Favourite;

        }

        public GroupClass(Guid id, string company, string companyImage, string name,
            string categoryID, string categoryName, string subCategoryID, string subCategoryName,
            //string levelID, string level, 
            int duration,
            string repeated, int repeatFrequency, string repeatDays, int numberOfLessons,
            DateTime startDate, int maxCapacity, double costOfCourse, double costOfSession, string[] adminIDList,
            string adminName, //string adminList,
            int locationID, string locationName, string state, string locationLng, string locationLat,
            string description, string imagePath,
            bool isCourse, bool allowDropIn,
            bool absorbFee, bool allowReservation, bool autoReservation, bool allowPayment, bool isPrivate,
            string classType, bool favourite = false)
        {
            ID = id;
            Company = company;
            CompanyImage = companyImage;
            Name = name;
            CategoryID = categoryID;
            CategoryName = categoryName;
            SubCategoryID = subCategoryID;
            SubCategoryName = subCategoryName;
            //Level = level;
            //LevelID = levelID;
            Duration = duration;

            RepeatFrequency = repeatFrequency;
            RepeatDays = repeatDays;

            NumberOfLessons = numberOfLessons;
            StartDate = startDate;

            switch (repeated)
            {
                case "never":
                    Repeated = Functions.Repeat.never;
                    break;
                case "Day":
                    Repeated = Functions.Repeat.Day;
                    break;
                case "Week":
                    Repeated = Functions.Repeat.Week;
                    break;
                case "Month":
                    Repeated = Functions.Repeat.Month;
                    break;
            }

            Minute = StartDate.ToString("mm");
            Hour = StartDate.ToString("HH");
            Time = Hour + ":" + Minute;
            MaxCapacity = maxCapacity;
            CostOfCourse = costOfCourse;
            CostOfSession = costOfSession;
            AdminIDList = adminIDList;
            AdminName = AdminName;
            //AdminList = AdminList;
            LocationID = locationID;
            LocationName = locationName;
            State = state;
            LocationLat = locationLat;
            LocationLng = locationLng;

            DAL db = new DAL();
            if (LocationID > 0 && LocationLat == "")
            {
                IEnumerable<dynamic> locationDetails = db.getLocationDetails(locationID.ToString(), company);
                if (locationDetails.Count() != 0)
                {
                    LocationName = locationDetails.ElementAt(0).Name;
                    State = locationDetails.ElementAt(0).State;
                    LocationLat = locationDetails.ElementAt(0).Latitude;
                    LocationLng = locationDetails.ElementAt(0).Longitude;
                }
            }

            Description = description;
            IsPrivate = isPrivate;
            Image = imagePath;


            ClassDates = Functions.getClassDates(this);
            EndDate = ClassDates.Last().AddMinutes(Duration);

            //updates the capacity details
            RemainingCapacity = MaxCapacity;
            //if (!String.IsNullOrEmpty(ID)) //if ID is empty this is only a new class
            //{
                db.checkMaxCapacity(this, MaxCapacity);
                foreach (int i in RemainingCapacityList)
                {
                    if (i < RemainingCapacity)
                    {
                        RemainingCapacity = i;
                    }
                }
            //}

            IsCourse = isCourse;
            AllowDropIn = allowDropIn;
            AbsorbFee = absorbFee;
            AllowReservation = allowReservation;
            AutoReservation = autoReservation;
            AllowPayment = allowPayment;
            IsActive = false;

            ClassType = classType;

            Favourite = favourite;
            
            updateSlug();
        }

        public string CreateClass()
        {
            DAL db = new DAL();
            var c = db.createClass(this);
            updateSlug();

            return c;
        }

        public string UpdateClass(GroupClass gClass_original)
        {
            if (this.Image == "")
            {
                this.Image = gClass_original.Image;
            }
            DAL db = new DAL();
            var c = db.updateClass(this, gClass_original);
            updateSlug();

            return c;

        }


        public bool CheckIfSpacesExist()
        {
            int count = 0;
            for (int i = 0; i < this.RemainingCapacityList.Count(); i++)
            {
                if (ClassDates[i] >= DateTime.Now)
                {
                    count += this.RemainingCapacityList[i];
                }
            }
            if (count <= 0)
            {
                return false;
            }
            else
            {
                return true;
            }

        }

        public DateTime NextClass()
        {
            if (IsAdvert)
            {
                //advert should slowly drop down the list.
                //
                //get the age of the advert in seconds and add that to the current time.
                //that will slowly move the adevert down the listing
                long age = DateTime.Now.Ticks - CreatedDate.Ticks;
                age = age / 10000000; //have seconds now

                return DateTime.Now.AddSeconds(age);
                //return CreatedDate;
            }
            else
            {

                foreach (DateTime dt in ClassDates)
                {
                    if (dt >= DateTime.Now)
                    {
                        return dt;
                    }
                }
                return ClassDates.Last();
                //return new DateTime(1950, 1, 1);
            }
        }

        public void CalculateNumberOfLessions()
        {
            if (this.Repeated == Functions.Repeat.never)
            {
                this.NumberOfLessons = 1;
            }
            else
            {
                this.NumberOfLessons = 1;
                int dayCount = 0;

                string[] days = this.RepeatDays.Split('|');
                if (String.IsNullOrEmpty(this.RepeatDays))
                {
                    days[0] = this.StartDate.DayOfWeek.ToString();
                }

                if (this.RepeatDays != "")
                {
                    int d = Functions.getDayOfWeekAsInt(this.StartDate.DayOfWeek.ToString());

                    foreach (string d1 in days)
                    {
                        dayCount++;
                        if (Functions.getDayOfWeekAsInt(d1) >= d)
                        {
                            this.StartDate = this.StartDate.AddDays(Functions.getDayOfWeekAsInt(d1) - d);
                            break;
                        }
                        else if (d1 == days.Last())
                        { //first day is next week.
                            this.StartDate = this.StartDate.AddDays(7 + (Functions.getDayOfWeekAsInt(days.First()) - d));
                            dayCount = 1;
                        }
                    }
                }

                DateTime dateTracker = this.StartDate;

                if (this.Repeated == Functions.Repeat.Day)
                {
                    while (dateTracker <= this.EndDate)
                    {
                        dateTracker = dateTracker.AddDays(this.RepeatFrequency);
                        this.NumberOfLessons++;
                    }
                }
                else if (this.Repeated == Functions.Repeat.Week)
                {
                    while (dateTracker <= this.EndDate)
                    {
                        if (dayCount >= days.Count())
                        {
                            dayCount = 0;
                        }

                        int currentDay = Functions.getDayOfWeekAsInt(dateTracker.DayOfWeek.ToString());
                        int nextDay = Functions.getDayOfWeekAsInt(days[dayCount]);

                        if (nextDay <= currentDay)
                        {
                            dateTracker = dateTracker.AddDays(this.RepeatFrequency * 7);
                        }
                        dateTracker = dateTracker.AddDays(nextDay - currentDay);
                        this.NumberOfLessons++;

                        dayCount++;
                    }
                }

                //recalculate dates based on number of lessons.
                ClassDates = Functions.getClassDates(this);


            }
        }

        public string GetUrl()
        {
            string url = HttpContext.Current.Request.Url.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped);
            if (this.IsAdvert)
            {
                url += "/Advert/" + this.Company + "/" + this.CategoryName.Replace(" and ", "-") + "/" + this.ID.ToString();
            }
            else { 
                url += "/Classes/" + this.Company + "/" + this.SubCategoryName.Replace(" ", "-") + "/" + this.ID.ToString();
            }
            return url;

        }

        private void updateSlug()
        {
            DAL db = new DAL();
            if(String.IsNullOrEmpty(this.SubCategoryName))
            {
                this.SubCategoryName = db.getSubCategoryName(this.SubCategoryID);
            }

            this.Slug = "/" + this.Company + "/" + Functions.convertToSlug(this.SubCategoryName) + "/" + StartDate.ToString("yyyy/MM/dd/") + Functions.convertToSlug(this.Name);

            while(true)
            {
                if(db.checkSlugExists(this))
                {
                    Match match = Regex.Match(this.Slug, @"-\d+$");
                    int i = 0;
                    if (match.Success )
                    {
                        i = Convert.ToInt32(this.Slug.Substring(this.Slug.LastIndexOf('-') + 1));
                        this.Slug = Regex.Replace(this.Slug, @"-\d+$", "");
                    }
                    i++;
                    this.Slug = this.Slug + "-" + i.ToString();
                }
                else
                {
                    if (!String.IsNullOrEmpty(this.Company))
                    {
                        db.updateClassSlug(this);
                    }
                    break;
                }
            }

        }

        #region 'Advert Functions'

        public GroupClass(bool isAdvert)
        {
            IsAdvert = isAdvert;

            //Default
            Name = "";
            Description = "";
            Image = "";
            Email = "";
            Phone = "";
            ContactName = "";
            Website = "";

            CategoryID = "";
            CategoryName = "";
            SubCategoryID = "";
            SubCategoryName = "";

            AdvertType = "";
            IsActive = false;

            Address1 = "";
            Address2 = "";
            City = "";
            State = "";

            ClassType = "Advert";

            CreatedDate = DateTime.Now;
        }

        public GroupClass(GroupClass advert_original, bool isAdvert)
        {
            IsAdvert = isAdvert;

            Name = advert_original.Name;
            Description = advert_original.Description;
            Image = advert_original.Image;
            Email = advert_original.Email;
            Phone = advert_original.Phone;
            ContactName = advert_original.ContactName;
            Website = advert_original.Website;

            CategoryID = advert_original.CategoryID;
            CategoryName = advert_original.CategoryName;
            AdvertType = advert_original.AdvertType;

            Address1 = advert_original.Address1;
            Address2 = advert_original.Address2;
            City = advert_original.City;
            State = advert_original.State;

            CreatedDate = advert_original.CreatedDate;
        }

        public GroupClass(Guid id, string name, string description, string imagePath, string email, string phone,
            string contactName, string website, string categoryID, string categoryName,
            string subCategoryID, string subCategoryName, string advertType, double cost,
            string address1, string address2, string city, string state, DateTime createdDate, string companyName,
            string userID, string billingID, bool isAdvert
            )
        {
            IsAdvert = isAdvert;

            ID = id;
            //Default
            UserID = userID;
            BillingID = billingID;
            Name = name;
            Description = description;
            Image = imagePath;
            Email = email;
            Phone = phone;
            ContactName = contactName;
            Website = website;

            CategoryID = categoryID;
            CategoryName = categoryName;
            SubCategoryID = subCategoryID;
            SubCategoryName = subCategoryName;
            if(SubCategoryID == "" && SubCategoryName != "")
            {
                DAL db = new DAL();
                SubCategoryID = db.getSubCategoryID(SubCategoryName, "DuckRow");
            }
            AdvertType = advertType;
            Cost = cost;

            Address1 = address1;
            Address2 = address2;
            City = city;
            State = state;

            Company = companyName;

            ClassType = "Advert";

            CreatedDate = createdDate;
        }


        public void CreateAdvert()
        {
            DAL db = new DAL();
            db.createAdvert(this);
        }

        public string UpdateAdvert(GroupClass advert_original)
        {
            if (this.Image == "")
            {
                this.Image = advert_original.Image;
            }
            DAL db = new DAL();
            return db.updateAdvert(this, advert_original);

        }

        public string PayAdvert(string ipn_track_id, string txn_id, string PayerName)
        {
            DAL db = new DAL();
            return db.insertAdvertPayment(this, ipn_track_id, txn_id, PayerName);
        }

        public bool DeleteAdvert()
        {
            DAL db = new DAL();
            return db.deleteAdvert(this.ID.ToString());
        }

        #endregion

    }
}
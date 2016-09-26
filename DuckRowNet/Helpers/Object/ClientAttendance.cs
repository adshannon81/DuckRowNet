using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DuckRowNet.Helpers.Object
{
    public class ClientAttendance
    {
        public string BookingDetailsID { get; set; }
        public string UserID { get; set; }
        public GroupClass ClassDetails { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public Dictionary<DateTime, Boolean> Paid { get; set; }
        public Dictionary<DateTime, Boolean> Cancelled { get; set; }
        public Dictionary<DateTime, Boolean> Confirmed { get; set; }
        public Functions.PersonType Type { get; set; }

        public List<ScheduledItem> ScheduledItems { get; set; }


        public ClientAttendance()
        {
            Paid = new Dictionary<DateTime, Boolean>();
            Cancelled = new Dictionary<DateTime, Boolean>();
            Confirmed = new Dictionary<DateTime, Boolean>();

            ScheduledItems = new List<ScheduledItem>();
        }

        public ClientAttendance(string userID, string bookingDetailsID, string classID, string firstName, string lastName, string email, string phone, Functions.PersonType type)
        {
            BookingDetailsID = bookingDetailsID;
            UserID = userID;
            FirstName = firstName;
            LastName = lastName;
            Phone = phone;
            Email = email;
            Type = type;
            Paid = new Dictionary<DateTime, Boolean>();
            Cancelled = new Dictionary<DateTime, Boolean>();
            Confirmed = new Dictionary<DateTime, Boolean>();
            ScheduledItems = new List<ScheduledItem>();

            DAL db = new DAL();
            ClassDetails = db.selectClassDetail(classID);

        }
    }
}
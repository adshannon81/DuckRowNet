using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DuckRowNet.Helpers.Object
{
    public class Subscription
    {
        public Guid ID { get; set; }
        public CompanyDetails CompanyDetails { get; set; }
        public string ProfileID { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime NextPaymentDate { get; set; }
        public DateTime LastPaymentDate { get; set; }
        public string Frequency { get; set; }
        public string Period { get; set; }
        public double Amount { get; set; }
        public string ProfileStatus { get; set; }
        public string PayerID { get; set; }
        public string PayerName { get; set; }

        public int TypeID { get; set; }

        public string Reference { get; set; }
        public string Comment { get; set; }
        public string txn_id { get; set; }
        public string ipn_track_id { get; set; }
        public string paypalFee { get; set; }
        public string UserID { get; set; }

        public Subscription()
        {
            ID = Guid.NewGuid();
        }

        public Subscription(CompanyDetails companyDetails, string profileID, DateTime createdDate, DateTime nextPaymentDate,
            DateTime lastPaymentDate, string frequency, string period, double amount, string profileStatus,
            string payerID, string subscriptionType, int subTypeID)
        {
            this.CompanyDetails = companyDetails;
            ProfileID = profileID;
            CreatedDate = createdDate;
            NextPaymentDate = nextPaymentDate;
            LastPaymentDate = lastPaymentDate;
            if (LastPaymentDate == null)
            {
                LastPaymentDate = CreatedDate;
            }
            Frequency = frequency;
            Period = period;
            Amount = amount;
            ProfileStatus = profileStatus;
            PayerID = payerID;

            DAL db = new DAL();

            TypeID = subTypeID;

            if (companyDetails.TypeID != this.TypeID)
            {
                db.updateCompanyType(companyDetails.ID.ToString(), TypeID.ToString());
            }

            ID = db.newSubscription(this);

        }

        public Subscription(CompanyDetails companyDetails)
        {
            DAL db = new DAL();
            IEnumerable<dynamic> details = db.getRecurringPaymentDetails(companyDetails.ID);

            this.CompanyDetails = companyDetails;

            if (details.Count() == 1)
            {
                ID = new Guid(details.ElementAt(0).ID);
                ProfileID = details.ElementAt(0).ProfileID;
                CreatedDate = details.ElementAt(0).CreatedDate;
                NextPaymentDate = details.ElementAt(0).NextPaymentDate;
                LastPaymentDate = details.ElementAt(0).LastPaymentDate;
                Frequency = details.ElementAt(0).Frequency;
                Period = details.ElementAt(0).Period;
                Amount = details.ElementAt(0).Amount;
                ProfileStatus = details.ElementAt(0).ProfileStatus;
                PayerID = details.ElementAt(0).PayerID;
            }
        }

        public bool PaymentReceived()
        {
            //update payment table
            DAL db = new DAL();
            db.InsertPayment(this.CompanyDetails.ID, Convert.ToDateTime(LastPaymentDate), Convert.ToDateTime(NextPaymentDate),
                Amount.ToString(), "Subscription", Comment, Reference, paypalFee, ipn_track_id, txn_id, PayerName, 0);

            //send receipt for payment
            //Paypal sends this automatically

            //update subscription table
            return db.updateSubscription(this);
        }

        public bool Update()
        {
            DAL db = new DAL();
            //update subscription table
            return db.updateSubscription(this);
        }

        public bool Cancel()
        {
            RecurringPayments rPay = new RecurringPayments();

            bool result = rPay.CancelRecurringPayment(this);
            if (result)
            {
                this.ProfileStatus = rPay.GetRecurringPaymentStatus(this);
                DAL db = new DAL();
                return db.cancelSubscription(this);
            }
            return false;
        }

        public void getDetails(string ProfileID)
        {
            this.ProfileID = ProfileID;
            DAL db = new DAL();
            IEnumerable<dynamic> details = db.getRecurringPaymentDetailsFromProfileID(ProfileID);

            if (details.Count() == 1)
            {
                ID = new Guid(details.ElementAt(0).ID);
                this.CompanyDetails = db.getCompanyDetails(details.ElementAt(0).CompanyName);
                CreatedDate = details.ElementAt(0).CreatedDate;
                NextPaymentDate = details.ElementAt(0).NextPaymentDate;
                LastPaymentDate = details.ElementAt(0).LastPaymentDate;
                Frequency = details.ElementAt(0).Frequency;
                Period = details.ElementAt(0).Period;
                Amount = details.ElementAt(0).Amount;
                ProfileStatus = details.ElementAt(0).ProfileStatus;
                PayerID = details.ElementAt(0).PayerID;
            }
        }

    }

}
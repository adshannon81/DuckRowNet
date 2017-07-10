using DuckRowNet.Helpers.Object;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;

namespace DuckRowNet.Helpers
{
    public class Email
    {
        private static string HostURL()
        {
            return HttpContext.Current.Request.Url.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped);
        }

        private static void Initialise()
        {
            if (ConfigurationManager.AppSettings["env"].ToString() == "prd")
            {             
                //PRD
                WebMail.SmtpServer = "localhost";
                WebMail.EnableSsl = false;
                WebMail.UserName = "info@duckrow.net";
                WebMail.Password = "la_i@s_Y5%la_i@s_Y5%";
                WebMail.From = "info@duckrow.net";
            }
        }


        public static string SendReservationConfirmed(string firstName, string lastName, string clientEmail, string clientType,
            GroupClass gClass, string className,
            DateTime classDate, string classRepeated, string classNumberOfLessons, string classLocation, string company,
            string comments = "",
            string sessionDate = "1950-1-1")
        {
            Initialise();

            var message = "Request Confirmed - An email has been sent to the client.";
            string cType = "class";

            if (String.IsNullOrEmpty(clientEmail))
            {
                message = "This client does not have an email address. " + Environment.NewLine.ToString() + "A place has been reserved but please contact the client directly to let them know.";
            }
            else
            {
                try
                {
                    DAL db = new DAL();

                    var emailBody = "";
                    DateTime sDate = Convert.ToDateTime(sessionDate);

                    emailBody = "<p>Hi " + firstName + " " + lastName + ", </p><p>This email is to let you know that ";
                    if (gClass.SubCategoryName == "Appointment")
                    {
                        cType = "appointment";
                        emailBody += "you have an ";
                        if (clientType == "client")
                        {
                            emailBody += "appointment ";
                        }
                        else
                        {
                            emailBody += "<a href=\"" + gClass.GetUrl() + "\">appointment</a> ";
                        }
                        emailBody += " with ";
                        List<string> instructors = db.getClassInstructors(gClass);

                        if (instructors.Count() == 0)
                        {
                            emailBody += company;
                        }
                        else {
                            emailBody += String.Join(", ", instructors);
                        }
                        emailBody += " <br /> <br/>";
                    }
                    else
                    {
                        emailBody += "we have reserved a place for you on <a href=\"" +
                            gClass.GetUrl() + "\">" + gClass.Name + "</a> (" + gClass.CategoryName
                            + "  - " + gClass.SubCategoryName
                            + ") <br /> <br/> ";
                    }

                    if (DateTime.Compare(Convert.ToDateTime(sessionDate), Convert.ToDateTime("1951-1-1")) > 0 || !gClass.IsCourse)
                    {
                        emailBody += "This is a single session beginning at " +
                            Convert.ToDateTime(sessionDate).ToString("HH:mm on dddd dd MMM yyyy") + "<br /> ";
                    }
                    else if (gClass.Repeated.ToString() == "never")
                    {
                        emailBody += "This is a single " + cType + " beginning at " +
                            gClass.StartDate.ToString("HH:mm on dddd dd MMM yyyy") + "<br /> ";
                    }
                    else if (gClass.Repeated.ToString() == "Day")
                    {
                        emailBody += "This " + cType + " begins at " +
                            gClass.StartDate.ToString("HH:mm on dddd dd MMM yyyy") + "<br /> ";
                        if (gClass.RepeatFrequency == 1)
                        {
                            emailBody += "It is repeated daily for " + gClass.NumberOfLessons + " days <br/>";
                        }
                        else
                        {
                            emailBody += "It is repeated every " + gClass.RepeatFrequency + " days for a total of " +
                                gClass.NumberOfLessons + " classes <br/>";
                        }
                    }
                    else if (gClass.Repeated.ToString() == "Week")
                    {
                        emailBody += "This " + cType + " begins at " +
                            gClass.StartDate.ToString("HH:mm on dddd dd MMM yyyy") + "<br /> ";
                        if (gClass.RepeatFrequency == 1)
                        {
                            emailBody += "It is repeated weekly on " + gClass.RepeatDays.Replace("|", ", ") +
                                " for a total of " + gClass.NumberOfLessons + " sessions <br/>";
                        }
                        else
                        {
                            emailBody += "It is repeated every " + gClass.RepeatFrequency + " weeks on " + gClass.RepeatDays +
                                " for a total of " + gClass.NumberOfLessons + " sessions <br/>";
                        }
                    }
                    //else if (gClass.Repeated.ToString() == "Month")
                    //{
                    //    emailBody += "This is a monthly class beginning at " + classDate.ToString("HH:mm") + " on " + classDate.DayOfWeek.ToString() + " " + classDate.ToString("dd MMM yyyy") + "<br /> " +
                    //    "and lasting for " + classNumberOfLessons + " months <br />";
                    //}

                    if (gClass.NumberOfLessons > 1)
                    {
                        emailBody += "</p><p>If there is any reason you cannot make an individual class then please contact us as soon as possible and let us know. ";
                    }

                    emailBody += "</p><p>This will take place at " + classLocation + "</p>";

                    var companyDetails = db.getCompanyDetails(company);
                    var anImage = "";
                    if (!String.IsNullOrEmpty(companyDetails.ImagePath))
                    {
                        anImage = companyDetails.ImagePath;
                    }

                    emailBody = EmailTemplate(company, anImage, "Reservation Confirmed", emailBody, companyDetails.Phone,
                        companyDetails.Email.ToString());


                    if (!String.IsNullOrEmpty(WebMail.SmtpServer))
                    {
                        WebMail.Send(to: clientEmail,
                            subject: company + " - Reservation Confirmed",
                            replyTo: companyDetails.Email.ToString(),
                            body: emailBody,
                            isBodyHtml: true
                        );
                    }

                }
                catch (Exception ex)
                {
                    message = "An error occurred while trying to send an email. " + Environment.NewLine.ToString() + "A place has been reserved but please contact the client directly to let them know.";
                    Logger.LogWarning("App_Code/Helper/Email", "SendReservationConfirmed", "email:'" + clientEmail + "'", ex.Message, ex);
                }
            }
            SendBookingConfirmed(firstName, lastName, clientEmail, clientType, gClass, comments, sessionDate);

            return message;
        }

        public static void SendBookingConfirmed(string firstName, string lastName, string clientEmail, string clientType,
            GroupClass gClass, string comments = "", string sessionDate = "1950-1-1")
        {
            Initialise();
            try
            {
                DAL db = new DAL();
                var emailBody = "";
                DateTime sDate = Convert.ToDateTime(sessionDate);
                string cType = "class";

                emailBody = "<p>Hi " + gClass.Company + ", </p><p>This email is to let you know that ";
                if (gClass.SubCategoryName == "Appointment")
                {
                    cType = "appointment";
                    emailBody += "an <a href=\"" + gClass.GetUrl() + "\">appointment</a> with ";
                    string instructor = String.Join(", ", db.getClassInstructors(gClass));
                    if (String.IsNullOrEmpty(instructor))
                    {
                        emailBody += gClass.Company;
                    }
                    else
                    {
                        emailBody += instructor;
                    }
                    emailBody += " has been booked";
                }
                else
                {
                    emailBody += "a place has been reserved on <a href=\"" +
                        gClass.GetUrl() + "\">" + gClass.Name + "</a> (" + gClass.CategoryName + "  - " + gClass.SubCategoryName
                        + ")";
                }
                emailBody += " for " + firstName + " " + lastName + " (" + clientEmail + ") <br/> <br/>";

                if (!String.IsNullOrEmpty(comments))
                {
                    emailBody += "<p>The following comments were sent with the request: </p><p> " + comments + "</p>";
                }

                if (DateTime.Compare(Convert.ToDateTime(sessionDate), Convert.ToDateTime("1951-1-1")) > 0 || !gClass.IsCourse)
                {
                    emailBody += "This is a single session beginning at " +
                        Convert.ToDateTime(sessionDate).ToString("HH:mm on dddd dd MMM yyyy") + "<br /> ";
                }
                else if (gClass.Repeated.ToString() == "never")
                {
                    emailBody += "This is a single " + cType + " beginning at " +
                        gClass.StartDate.ToString("HH:mm on dddd dd MMM yyyy") + "<br /> ";
                }
                else if (gClass.Repeated.ToString() == "Day")
                {
                    emailBody += "This " + cType + " begins at " +
                        gClass.StartDate.ToString("HH:mm on dddd dd MMM yyyy") + "<br /> ";
                    if (gClass.RepeatFrequency == 1)
                    {
                        emailBody += "It is repeated daily for " + gClass.NumberOfLessons + " days <br/>";
                    }
                    else
                    {
                        emailBody += "It is repeated every " + gClass.RepeatFrequency + " days for a total of " +
                            gClass.NumberOfLessons + " classes <br/>";
                    }
                }
                else if (gClass.Repeated.ToString() == "Week")
                {
                    emailBody += "This " + cType + " begins at " +
                        gClass.StartDate.ToString("HH:mm on dddd dd MMM yyyy") + "<br /> ";
                    if (gClass.RepeatFrequency == 1)
                    {
                        emailBody += "It is repeated weekly on " + gClass.RepeatDays.Replace("|", ", ") +
                            " for a total of " + gClass.NumberOfLessons + " sessions <br/>";
                    }
                    else
                    {
                        emailBody += "It is repeated every " + gClass.RepeatFrequency + " weeks on " + gClass.RepeatDays +
                            " for a total of " + gClass.NumberOfLessons + " sessions <br/>";
                    }
                }

                emailBody += "</p><p>This will take place at " + gClass.LocationName + "</p>";

                var companyDetails = db.getCompanyDetails(gClass.Company);

                emailBody = EmailTemplate("DuckRow.net", "Images/duckrow-logo2.png", "Reservation Confirmed",
                    emailBody, "", "info@DuckRow.net");



                if (!String.IsNullOrEmpty(WebMail.SmtpServer))
                {
                    WebMail.Send(to: companyDetails.Email.ToString(),
                        subject: "DuckRow.net - Booking Confirmed",
                        replyTo: companyDetails.Email.ToString(),
                        body: emailBody,
                        isBodyHtml: true
                    );
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning("App_Code/Helper/Email", "SendBookingConfirmed", "email:'" + clientEmail + "'", ex.Message,ex);
            }


        }


        public static string SendReservationCancelled(GroupClass gClass, string firstName, string lastName, string clientEmail, string classID, string className,
            string classDate, string classRepeated, string classNumberOfLessons, string classLocation, string company)
        {
            Initialise();
            var message = "Request Cancelled - An email has been sent to the client.";

            try
            {
                DateTime cDate = Convert.ToDateTime(classDate);

                var emailBody = "<p>Hi " + firstName + " " + lastName + ", </p><p> " +
                    "Unfortunately we could <b>not</b> confirm a place for you on <a href=\"" + gClass.GetUrl() + "\">" + className + "</a> (" + gClass.CategoryName + "  - " + gClass.SubCategoryName + ") <br /> " +
                    "Please contact us if you have any questions.<br /> " +
                    "</p>";

                DAL db = new DAL();
                var companyDetails = db.getCompanyDetails(company);
                var anImage = "";
                if (!String.IsNullOrEmpty(companyDetails.ImagePath))
                {
                    anImage = companyDetails.ImagePath;
                }

                emailBody = EmailTemplate(company, anImage, "Unfortunately your reservation has been cancelled", emailBody, companyDetails.Phone,
                    companyDetails.Email.ToString());

                if (!String.IsNullOrEmpty(WebMail.SmtpServer))
                {
                    WebMail.Send(to: clientEmail,
                        subject: company + " - Reservation Cancelled",
                        replyTo: companyDetails.Email.ToString(),
                        body: emailBody,
                        isBodyHtml: true
                    );
                }
            }
            catch (Exception ex)
            {
                message = "An error occurred while trying to send an email. Please try again or contact the client directly.";
                Logger.LogWarning("App_Code/Helper/Email", "SendReservationCancelled", clientEmail, ex.Message, ex);
            }

            return message;
        }


        public static string SendRequest_Client(string firstName, string lastName, string clientEmail, GroupClass gClass, string sessionDate = "1950-1-1")
        {
            Initialise();
            var message = "success";

            try
            {

                var emailBody = "<p>Hi " + firstName + " " + lastName + ", </p><p>We will confirm your place shortly.</p>" +
                        "<p></p><p> " +
                        "You have requested a place on <a href=\"" + gClass.GetUrl() + "\"</a> (" + gClass.CategoryName + "  - " + gClass.SubCategoryName + ") ";


                if (sessionDate != "1950-1-1" || !gClass.IsCourse)
                {
                    emailBody += "for " + Convert.ToDateTime(sessionDate).ToString("HH:mm on dddd dd MMM yyyy") + "<br /> ";
                }
                else if (gClass.Repeated == Functions.Repeat.never)
                {
                    emailBody += "<br/>This is a single class beginning at " + gClass.StartDate.ToString("HH:mm") + " on " + gClass.StartDate.DayOfWeek.ToString() + " " + gClass.StartDate.ToString("dd MMM yyyy") + "<br /> ";
                }
                else if (gClass.Repeated == Functions.Repeat.Day)
                {
                    emailBody += "<br/>This is a daily class beginning at " + gClass.StartDate.ToString("HH:mm") + " on " + gClass.StartDate.DayOfWeek.ToString() + " " + gClass.StartDate.ToString("dd MMM yyyy") + "<br /> " +
                    "and lasting for " + gClass.NumberOfLessons + " days <br />";
                }
                else if (gClass.Repeated == Functions.Repeat.Week)
                {
                    emailBody += "<br/>This is a weekly class beginning at " + gClass.StartDate.ToString("HH:mm") + " on " + gClass.StartDate.DayOfWeek.ToString() + " " + gClass.StartDate.ToString("dd MMM yyyy") + "<br /> " +
                    "and lasting for " + gClass.NumberOfLessons + " weeks <br />";
                }
                else if (gClass.Repeated == Functions.Repeat.Month)
                {
                    emailBody += "<br/>This is a monthly class beginning at " + gClass.StartDate.ToString("HH:mm") + " on " + gClass.StartDate.DayOfWeek.ToString() + " " + gClass.StartDate.ToString("dd MMM yyyy") + "<br /> " +
                    "and lasting for " + gClass.NumberOfLessons + " months <br />";
                }

                emailBody += "</p><p>The class will take place at " + gClass.LocationName + "</p>";

                DAL db = new DAL();
                var companyDetails = db.getCompanyDetails(gClass.Company);
                var anImage = "";
                if (!String.IsNullOrEmpty(companyDetails.ImagePath))
                {
                    anImage = companyDetails.ImagePath;
                }

                emailBody = EmailTemplate(companyDetails.Name, anImage, "Thank you for your request", emailBody, companyDetails.Phone,
                    companyDetails.Email.ToString());


                if (!String.IsNullOrEmpty(WebMail.SmtpServer))
                {
                    WebMail.Send(to: clientEmail,
                        subject: companyDetails.Name + " - Request Received",
                        replyTo: companyDetails.Email.ToString(),
                        body: emailBody,
                        isBodyHtml: true
                    );
                }
            }
            catch (Exception ex)
            {
                message = "An error occurred while trying to send an email. Please try again or contact the client directly.";
                Logger.LogWarning("App_Code/Helper/Email", "SendRequest_Client", clientEmail, ex.Message + " _ " + ex.StackTrace, ex);
            }

            return message;
        }


        public static string SendRequest_Admin(string firstName, string lastName,
            string comments, string guid, GroupClass gClass, string sessionDate = "1950-1-1")
        {
            Initialise();
            var message = "success";
            var companyEmail = "";

            try
            {


                var emailBody = "<p>Hi, </p><p> " +
                    firstName + " " + lastName + " has requested a place on <a href=\"" + gClass.GetUrl() + "\">" + gClass.Name + "</a> (" + gClass.CategoryName + "  - " + gClass.SubCategoryName + ")  ";

                if (!String.IsNullOrEmpty(comments))
                {
                    emailBody += "with the following comments: </p><p> " + comments + "</p><p>";
                }
                else {
                    emailBody += "</p><p>";
                }

                if (DateTime.Compare(Convert.ToDateTime(sessionDate), Convert.ToDateTime("1951-1-1")) > 0 || !gClass.IsCourse)
                {
                    emailBody += "This is a single session beginning at " +
                        Convert.ToDateTime(sessionDate).ToString("HH:mm on dddd dd MMM yyyy") + "<br /> ";
                }
                else if (gClass.Repeated == Functions.Repeat.never)
                {
                    emailBody += "This is a single class beginning at " + gClass.StartDate.ToString("HH:mm") + " on " + gClass.StartDate.DayOfWeek.ToString() + " " + gClass.StartDate.ToString("dd MMM yyyy") + "<br /> ";
                }
                else if (gClass.Repeated == Functions.Repeat.Day)
                {
                    emailBody += "This is a daily class beginning at " + gClass.StartDate.ToString("HH:mm") + " on " + gClass.StartDate.DayOfWeek.ToString() + " " + gClass.StartDate.ToString("dd MMM yyyy") + "<br /> " +
                    "and lasting for " + gClass.NumberOfLessons + " days <br />";
                }
                else if (gClass.Repeated == Functions.Repeat.Week)
                {
                    emailBody += "This is a weekly class beginning at " + gClass.StartDate.ToString("HH:mm") + " on " + gClass.StartDate.DayOfWeek.ToString() + " " + gClass.StartDate.ToString("dd MMM yyyy") + "<br /> " +
                    "and lasting for " + gClass.NumberOfLessons + " weeks <br />";
                }
                else if (gClass.Repeated == Functions.Repeat.Month)
                {
                    emailBody += "This is a monthly class beginning at " + gClass.StartDate.ToString("HH:mm") + " on " + gClass.StartDate.DayOfWeek.ToString() + " " + gClass.StartDate.ToString("dd MMM yyyy") + "<br /> " +
                    "and lasting for " + gClass.NumberOfLessons + " months <br />";
                }

                emailBody += "</p><p>The class will take place at " + gClass.LocationName + "</p>" +
                    "<p></p>";

                if (gClass.AutoReservation)
                {
                    emailBody += "<p>This Class has 'Auto Reserve' so there is no need to confirm.</p>";
                }
                else
                {
                    emailBody += "<p>To confirm this request, please click <a href='" + HostURL() + "/Admin/ReservationConfirm/"
                    + gClass.Company + "?guid=" + guid + "'>here</a></p>";
                }

                DAL db = new DAL();
                var companyDetails = db.getCompanyDetails(gClass.Company);
                companyEmail = companyDetails.Email.ToString();
                var anImage = "";
                if (!String.IsNullOrEmpty(companyDetails.ImagePath))
                {
                    anImage = companyDetails.ImagePath;
                }

                emailBody = EmailTemplate(companyDetails.Name, anImage, "Reservation Request", emailBody, companyDetails.Phone,
                    companyEmail);

                if (!String.IsNullOrEmpty(WebMail.SmtpServer))
                {
                    WebMail.Send(to: companyEmail,
                        subject: "DuckRow.net - Reservation Request",
                        replyTo: companyEmail,
                        body: emailBody,
                        isBodyHtml: true
                    );
                }
            }
            catch (Exception ex)
            {
                message = "An error occurred while trying to send an email. Please try again or contact the client directly.";
                Logger.LogWarning("App_Code/Helper/Email", "SendRequest_Admin", companyEmail, ex.Message + " _ " + ex.StackTrace, ex);
            }

            return message;
        }
        

        public static void SendClassDeleted(string firstName, string lastName, GroupClass gClass)
        {
            Initialise();
            var companyEmail = "";
            try
            {
                DateTime cDate = Convert.ToDateTime(gClass.StartDate);

                var emailBody = "<p>Hi, " + firstName + " " + lastName + "</p><p> " +
                    "Unfortunately, we had to cancel <a href=\"" + gClass.GetUrl() + "\">" + gClass.Name + "</a> (" + gClass.CategoryName + "  - " + gClass.SubCategoryName + ") <br /> " +
                    "</p><p>The class will no longer go ahead.</p>" +
                    "<p></p>" +
                    "<p>we regret any inconvenience this may cause. If you have any questions then please contact us directly</p>";

                DAL db = new DAL();
                var companyDetails = db.getCompanyDetails(gClass.Company);
                companyEmail = companyDetails.Email.ToString();
                var anImage = "";
                if (!String.IsNullOrEmpty(companyDetails.ImagePath))
                {
                    anImage = companyDetails.ImagePath;
                }
                emailBody = EmailTemplate(gClass.Company, anImage, "Class Cancelled", emailBody, companyDetails.Phone,
                    companyEmail);


                if (!String.IsNullOrEmpty(WebMail.SmtpServer))
                {
                    WebMail.Send(to: companyEmail,
                        subject: gClass.Company + " - Cancellation",
                        replyTo: companyEmail,
                        body: emailBody,
                        isBodyHtml: true
                    );
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning("App_Code/Helper/Email", "SendClassDeleted", companyEmail, ex.Message, ex);
            }
        }


        public static void SendRegistrationEmail(string email, string confirmationUrl, string company, string password = null)
        {
            Initialise();
            try
            {
                DAL db = new DAL();
                var emailBody = "<p>Welcome to DuckRow.net!</p><p> " +
                    "To get started you must first activate your account by <a href=\"" + confirmationUrl + "\">clicking here</a>." +
                    "<br/>You can then login using this email address, " + email + ", and the password you provided.</p>";

                if (!String.IsNullOrEmpty(password))
                {
                    emailBody = "<p>Welcome to DuckRow.net!</p><p> " +
                    company + " has added you as a user. To get started you must first activate your account by <a href=\"" + confirmationUrl + "\">clicking here</a>." +
                    "<br/>You can then login using this email address, " + email + " and the password " + password + "</p>";
                }

                if (!company.Equals("DuckRow.net"))
                {
                    emailBody += "<p>You can visit <a href=\"" + HostURL() + "/classes/" + company + "\">" + company + " here</a></p>";
                }

                emailBody += "<p></p>";

                var companyDetails = db.getCompanyDetails(company);

                emailBody = EmailTemplate(company, "Images/duckrow-logo2.png", "Activation Required", emailBody, "",
                    companyDetails.Email.ToString());


                if (!String.IsNullOrEmpty(WebMail.SmtpServer))
                {
                    WebMail.Send(to: email,
                        subject: "DuckRow.net - Please confirm your account",
                        replyTo: companyDetails.Email.ToString(),
                        body: emailBody,
                        isBodyHtml: true
                    );
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning("App_Code/Helper/Email", "SendRegistrationEmail", company, ex.Message, ex );
            }
        }

        public static void SendCompanyRegistrationEmail(string email, string company)
        {
            Initialise();
            try
            {
                DAL db = new DAL();
                var link = "" + HostURL() + "/Classes/" + company;
                var linkName = "DuckRow.Net/" + company;
                var companyInfo = "" + HostURL() + "/Admin/Company/" + company;

                var emailBody = "<p>" + company + " is now registered on DuckRow.net!</p><p> " +
                    "You can access your page at <br/><br/><a href=\"" + link + "\">" + linkName + "</a> <br/><br/>" +
                    "Share this link with everyone or just tell them to search for <b>" + company + "</b> on DuckRow.Net to find all classes and events you have going on.<br/><br/>" +
                    "Log in using your username/password and begin updating your <a href=\"" + companyInfo + "\">Company Information</a>.</p>" +
                    "<p></p>";

                var qtDetails = db.getCompanyDetails("DuckRow");

                emailBody = EmailTemplate("DuckRow.net", "Images/duckrow-logo2.png", "Company Registered!", emailBody, "",
                    qtDetails.Email.ToString());


                if (!String.IsNullOrEmpty(WebMail.SmtpServer))
                {
                    WebMail.Send(to: email,
                        subject: "DuckRow.net - Your Company Has Been Registered",
                        replyTo: qtDetails.Email.ToString(),
                        body: emailBody,
                        isBodyHtml: true
                    );
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning("App_Code/Helper/Email", "SendCompanyRegistrationEmail", company, ex.Message, ex);
            }
        }

        public static void SendCompanyRegistrationEmail_AddUser(string email, CompanyDetails companyDetails)
        {
            Initialise();
            try
            {
                DAL db = new DAL();
                var link = "" + HostURL() + "/" + companyDetails.Name;
                var linkName = "DuckRow.Net/" + companyDetails.Name;
                var companyInfo = "" + HostURL() + "/Admin/Company/" + companyDetails.Name;

                var emailBody = "<p>Hi,</p>" + "<p>You have been set up as a user with " + companyDetails.Name + "</p><p> " +
                    "Login at <a href=\"" + link + "\">" + linkName + "</a> to access the company details.<br/><br/></p>" +
                    "<p></p>";

                var anImage = "";
                if (!String.IsNullOrEmpty(companyDetails.ImagePath))
                {
                    anImage = companyDetails.ImagePath;
                }

                emailBody = EmailTemplate(companyDetails.Name, anImage, "Access to " + companyDetails.Name + "!", emailBody,
                    companyDetails.Phone,
                    companyDetails.Email.ToString());


                if (!String.IsNullOrEmpty(WebMail.SmtpServer))
                {
                    WebMail.Send(to: email,
                        subject: companyDetails.Name + " - Access Granted",
                        replyTo: companyDetails.Email.ToString(),
                        body: emailBody,
                        isBodyHtml: true
                    );
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning("App_Code/Helper/Email", "SendCompanyRegistrationEmail", companyDetails.Name, ex.Message, ex);
            }
        }

        public static void SendBookingPaymentEmail(string company, string payerEmail, string username, string userEmail,
            string txn_id, string itemName,
            DateTime classDate, string numberOfLessons, string location, string instructor,
            string companyImage, string companyPhone, string companyEmail, GroupClass gClass)
        {
            Initialise();
            try
            {
                var emailBody = "<p>Payment Received.</p><p> " +
                    "<p>Hi " + username + ", </p>" +
                    "<p>Thank you for choosing us. <br />" +
                    "Your business is greatly appreciated.</p><p></p><p>Your Order Reference is " + txn_id + " <br /> " +
                    "You have booked <a href=\"" + gClass.GetUrl() + "\">" + itemName + " </a> <br /> " +
                    "</p><p>The class will take place at " + location + "</p>" +
                    "<p>We look forward to seeing you soon!</p>" +
                    "<p></p>";

                emailBody = EmailTemplate(company, companyImage, "Thank You For Your Payment", emailBody, companyPhone,
                    companyEmail);


                if (!String.IsNullOrEmpty(WebMail.SmtpServer))
                {
                    WebMail.Send(to: userEmail,
                        subject: company + " - Payment Received",
                        replyTo: companyEmail,
                        body: emailBody,
                        isBodyHtml: true
                    );
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning("App_Code/Helper/Email", "SendBookingPaymentEmail", company + ":" + userEmail, ex.Message, ex);
            }
        }

        public static void SendSubscriptionPaymentEmail(string company, string email)
        {
            Initialise();
            try
            {
                var url = "" + HostURL() + "/Admin/Billing/" + company;
                var emailBody = "<p>Payment Received.</p><p> " +
                    "<p>Here at DuckRow.net it is important that you are successfully. After all, your Business is our Business. <br/>" +
                    "We strive to improve our services so that your Business will succeed. </p>" +
                    "<p>You can check all subscriptions for " + company + " by <a href=\"" + url + "\">clicking here</a>.</p>" +
                    "<p>We greatly appreciate your commitment to us and look forward to working with you!</p>" +
                    "<p></p>";

                emailBody = EmailTemplate("DuckRow.net", "Images/duckrow-logo2.png", "Thank You For Your Payment", emailBody, "",
                    "info@DuckRow.net");


                if (!String.IsNullOrEmpty(WebMail.SmtpServer))
                {
                    WebMail.Send(to: email,
                        subject: "DuckRow.net - Payment Received",
                        replyTo: "info@DuckRow.net",
                        body: emailBody,
                        isBodyHtml: true
                    );
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning("App_Code/Helper/Email", "SendSubscriptionPaymentEmail", company + ":" + email, ex.Message, ex);
            }
        }

        public static void SendAdvertPaymentConfirmed(PersonDetails user, GroupClass advert)
        {
            Initialise();
            try
            {
                var url = "" + HostURL() + "/advert/" + advert.ID;
                var emailBody = "<p>Payment Received.</p><p> " +
                    "<p>You have created an advert on DuckRow.net! <br/> " +
                    "To view your advert <a href=\"" + url + "\">click here</a> </p> " +
                    "<p><br/>Here at DuckRow.net we are always striving to improve our service. If you have any special requests or " +
                    "suggested improvements then we would love to hear them. </p>" +
                    "<p></p>";

                emailBody = EmailTemplate("DuckRow.net", "Images/duckrow-logo2.png", "Advert Created", emailBody, "",
                    "info@DuckRow.net");


                if (!String.IsNullOrEmpty(WebMail.SmtpServer))
                {
                    WebMail.Send(to: user.Email,
                        subject: "DuckRow.net - Advert Created",
                        replyTo: "info@DuckRow.net",
                        body: emailBody,
                        isBodyHtml: true
                    );
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning("App_Code/Helper/Email", "SendAdvertPaymentConfirmed", "advert ID - " + advert.ID + ": user " + user.Email, ex.Message + "<br/>" + ex.StackTrace, ex);
            }
        }


        public static bool SendContactEmail(string name, string fromEmail, string toEmail, string subject, string message)
        {
            Initialise();
            try
            {
                var emailBody = "<p>Contact message from " + name + " " + fromEmail + "</p><p> " +
                    "<p>" + message + "</p><p></p>";

                emailBody = EmailTemplate("DuckRow.net", "Images/duckrow-logo2.png", "Contact: " + subject, emailBody, "",
                    "info@DuckRow.net");


                if (!String.IsNullOrEmpty(WebMail.SmtpServer))
                {
                    WebMail.Send(to: toEmail,
                        subject: "DuckRow.net - Contact Received",
                        replyTo: fromEmail,
                        body: emailBody,
                        isBodyHtml: true
                    );
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogWarning("App_Code/Helper/Email", "SendContactEmail", fromEmail + "|" + toEmail, ex.Message, ex);
                return false;
            }
        }

        public static bool SendClientEmail(CompanyDetails companyDetails, string name, string toEmail, string fromEmail, string subject, string message, IEnumerable<string> attachment = null)
        {
            Initialise();
            try
            {
                var emailBody = "<p>Email received from " + name + " (" + fromEmail + ")</p><p>" + message + "</p><p></p>";

                var anImage = "";
                if (!String.IsNullOrEmpty(companyDetails.ImagePath))
                {
                    anImage = companyDetails.ImagePath;
                }

                emailBody = EmailTemplate(companyDetails.Name, anImage, subject, emailBody, companyDetails.Phone, companyDetails.Email);


                if (!String.IsNullOrEmpty(WebMail.SmtpServer))
                {
                    if (attachment != null && attachment.Count() > 0
                        && !String.IsNullOrEmpty(attachment.ElementAt(0)))
                    {
                        //Logger.LogWarning("App_Code/Helper/Email", "SendContactEmail:attachment", email, "");

                        WebMail.Send(to: toEmail,
                            subject: subject,
                            replyTo: fromEmail,
                            body: emailBody,
                            filesToAttach: attachment,
                            isBodyHtml: true
                        );
                        foreach (string s in attachment)
                        {
                            File.Delete(s);
                        }
                    }
                    else {
                        WebMail.Send(to: toEmail,
                            subject: subject,
                            replyTo: fromEmail,
                            body: emailBody,
                            isBodyHtml: true
                        );
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogWarning("App_Code/Helper/Email", "SendContactEmail", toEmail + "|" + fromEmail, ex.Message, ex);
                return false;
            }
        }

        public static bool transferRequest(string userID, string userEmail, string company, string total)
        {
            Initialise();
            try
            {
                var emailBody = "<p>The user " + userID + " " + userEmail + " has request transfer of funds for " + company + " </p><p> " +
                    "<p>total: " + total + "</p><p></p>";

                emailBody = EmailTemplate("DuckRow.net", "Images/duckrow-logo2.png", "Transfer Request", emailBody, "",
                    "info@DuckRow.net");


                if (!String.IsNullOrEmpty(WebMail.SmtpServer))
                {
                    WebMail.Send(to: "info@DuckRow.net",
                        subject: "DuckRow.net - Contact Received",
                        replyTo: "info@DuckRow.net",
                        body: emailBody,
                        isBodyHtml: true
                    );
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogWarning("App_Code/Helper/Email", "transferRequest", "ID:" + userID + ":Email:" + userEmail + ":company:" + company, ex.Message, ex);
                return false;
            }
        }

        public static bool SendUnconfirmedPaypalError(CompanyDetails companyDetails)
        {
            Initialise();
            try
            {
                var emailBody = "<p>Hi,</p><p>DuckRow.net has tried to process a payment for you but your Paypal Account (" +
                    companyDetails.PaypalEmail + ") is currently unconfirmed by Paypal.</p>" +
                    "<p>To fix this issue you need to do two things with your Paypal account... </p>" +
                    " 1 - Associate a bank account with your Paypal account <br/>" +
                    " 2 - Verify your Paypal account <br/> " +
                    " 3 - Confirm your email address with Paypal </br> " +
                    "<p>For more information on verifying your account, visit <a href=\"https://www.paypal.com/ie/cgi-bin/webscr?cmd=xpt/Marketing/general/NewConsumerLink-outside\">Paypal</a></p>" +
                    "<p>Or for information on confirming your email with paypal, click <a href=\"https://www.paypal.com/webapps/helpcenter/article/?solutionId=12757&m=SRE\">here</a></p>" +
                    "<p>If you still have difficulty verifiying your account then email us at info@DuckRow.net and we can help get you set up.</p>";


                emailBody = EmailTemplate("DuckRow.net", "Images/duckrow-logo2.png", "Unconfirmed Paypal Account", emailBody, "",
                    "info@DuckRow.net");


                if (!String.IsNullOrEmpty(WebMail.SmtpServer))
                {
                    WebMail.Send(to: "info@DuckRow.net",
                        subject: "DuckRow.net - Error Logged",
                        replyTo: "info@DuckRow.net",
                        body: emailBody,
                        isBodyHtml: true
                    );
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogWarning("App_Code/Helper/Email", "SendUnconfirmedPaypalError", companyDetails.Name + " - " + companyDetails.PaypalEmail, ex.Message, ex);
                return false;
            }
        }

        public static bool sendPasswordReset(string email, string resetToken, string resetUrl)
        {
            Initialise();
            try
            {
                var emailBody = "<p>Hi, <br/> A password reset was requested for your email address. </p>" +
                    "<p>Use this password reset token to reset your password. " +
                    "The token is: " + resetToken + @". Visit <a href=""" + resetUrl + @""">" + resetUrl + "</a> to reset your password.</p>" +
                    "<p>If you did not request this or if you have any difficulty resetting your password then please reply and let us know.</p>";

                emailBody = EmailTemplate("DuckRow.net", "Images/duckrow-logo2.png", "Password Reset Request",
                    emailBody, "", "info@DuckRow.net");

                if (!String.IsNullOrEmpty(WebMail.SmtpServer))
                {
                    WebMail.Send(to: email,
                        subject: "DuckRow.net - Password Reset Request",
                        body: emailBody,
                        isBodyHtml: true
                    );
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogWarning("App_Code/Helper/Email", "sendPasswordReset", ex.StackTrace, ex.Message, ex);
                return false;
            }
        }

        public static bool sendError(string error)
        {
            Initialise();
            try
            {
                var emailBody = "<p>Error Logged:  " + error + "</p>";

                emailBody = EmailTemplate("DuckRow.Net", "Images/duckrow-logo2.png", "Error Logged", emailBody, "",
                    "info@DuckRow.net");


                if (!String.IsNullOrEmpty(WebMail.SmtpServer))
                {
                    WebMail.Send(to: "info@duckrow.net",
                        subject: "DuckRow.Net - Error Logged",
                        replyTo: "info@duckrow.net",
                        body: emailBody,
                        isBodyHtml: true
                    );
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogWarning("App_Code/Helper/Email", "sendError", error, ex.Message, ex);
                return false;
            }
        }

        public static bool sendClaimOwnership(string userID, string company)
        {
            Initialise();
            try
            {
                var emailBody = "<p>Ownership Claim</p><p>User:" + userID +"</p><p>Company:" + company + "</p>";

                emailBody = EmailTemplate("DuckRow.Net", "Images/duckrow-logo2.png", "Ownership Claim", emailBody, "",
                    "info@DuckRow.net");


                if (!String.IsNullOrEmpty(WebMail.SmtpServer))
                {
                    WebMail.Send(to: "info@duckrow.net",
                        subject: "DuckRow.Net - Ownership Claim",
                        replyTo: "info@duckrow.net",
                        body: emailBody,
                        isBodyHtml: true
                    );
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogWarning("App_Code/Helper/Email", "sendClaimOwnership", "", ex.Message, ex);
                return false;
            }
        }

        //####################################


        private static string EmailTemplate(string company, string companyImage, string emailHeader, string emailBody, string companyPhone, string companyEmail)
        {
            var text = "<body style=\"background-color:#ffffff;\"> " +
                  "<table cellspacing=\"0\" cellpadding=\"0\" border=\"0\" width=\"100%\"> " +
                   " <tr> " +
                    " <td style=\"background-color:#FFFFFF; padding: 30px 15px 0;\"> " +
                     " <table cellspacing=\"0\" cellpadding=\"0\" border=\"0\" align=\"center\" width=\"710\" style=\"font-family: 'Helvetica Neue', Helvetica, Arial, sans-serif; font-size: 16px; color: #333;\"> " +
                      "  <tr> " +
                       "  <td style=\"background-color: #fff;border-bottom:3px solid #e4e8eb;\"> ";



            if (!String.IsNullOrEmpty(companyImage))
            {
                //companyImage = Href(companyImage);
                text += "  <a style=\"color:#245065; display: block;\" href=\"" + HostURL() + "/Classes/" + company + "\"> " +
                    "<img src=\"" + HostURL() + "/" + companyImage + "\" alt=\"" + company + "\" height=\"80\" style=\"padding: 10px 10px 10px 25px; display: block;\" /> " +
                    " </a> ";
            }
            else
            {
                text += "       <h1 style=\"color:#245065; font-weight: normal; font-size: 19px; line-height: 1.2; margin: 10px 10px 10px 25px;\"> " +
                       company +
                       "       </h1> ";
            }

            text += " </td> " +
                       " </tr>  " +
                       " <tr> " +
                       "  <td style=\"background-color: #FFFFFF; padding: 25px 40px 22px;\"> " +
                       "   <table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" width=\"600\" align=\"center\" style=\"margin: 0 auto;\" class=\"header\"> " +
                       "     <tr> " +
                       "      <td height=\"36\" width=\"540\" valign=\"middle\" style=\"padding: 0 25px;\"> " +
                       "       <h1 style=\"color:#245065; font-weight: normal; font-size: 19px; line-height: 1.2; margin: 0;\"> " +
                       emailHeader +
                       "       </h1> " +
                       "      </td> " +
                       "      <td valign=\"middle\" align=\"right\" style=\"padding: 0 25px;\"> " +
                       "       <!--<img alt=\"\" src=\"tick image\" style=\"display: block; border: 0;\" />--> " +
                       "      </td> " +
                       "     </tr> " +
                       "     <tr> " +
                       "         <td height=\"20\" width=\"540\" valign=\"middle\" style=\"padding: 0 25px;\">  " +
                       "         </td> " +
                       "     </tr> " +
                       "     <tr> " +
                       "      <td height=\"36\" width=\"540\" valign=\"middle\" style=\"padding: 0 25px;\"> " +
                       "       <p> " +
                       emailBody +
                       "       </p> " +
                       "      </td> " +
                       "     </tr> " +
                       "     <tr> " +
                       "         <td height=\"20\" width=\"540\" valign=\"middle\" style=\"padding: 0 25px;\"> " +
                       "         </td> " +
                       "     </tr> " +
                       "     <tr> " +
                       "         <td height=\"36\" width=\"540\" valign=\"middle\" style=\"padding: 0 25px;\"> " +
                       "             <p>Thank You,</p> " +
                       "             <p>" + company + "</p> " +
                       "             <p> </p> " +
                       "         </td> " +
                       "     </tr > " +
                       "     <tr> " +
                       "         <td height=\"20\" width=\"540\" valign=\"middle\" style=\"font-size: 16px; padding: 0 25px;\"> ";

            if (!String.IsNullOrEmpty(companyPhone))
            {
                text += "<p>Tel: " + companyPhone + "<br />";
            }

            text += "             <a href=\"mailto:" + companyEmail + "\" style=\"color: #57809E; text-decoration: none;\">" + companyEmail + "</a></p> " +
                       "         </td> " +
                       "     </tr> " +
                       "   </table> " +
                       "  </td> " +
                       " </tr> " +
                       " <tr> " +
                       "  <td style=\"padding: 0 0 10px 0; background-color: #ffffff; border-bottom:3px solid #e4e8eb;\"> " +
                       "  </td> " +
                       " </tr> " +
                       "<tr> " +
                       " <td> " +
                      "<div style=\"padding: 0 5px;\"> " +
                      "  <div style=\"height: 2px; line-height: 2px; font-size: 2px; clear: both; -webkit-border-bottom-right-radius: 5px; -webkit-border-bottom-left-radius: 5px; -moz-border-radius-bottomright: 5px; -moz-border-radius-bottomleft: 5px; border-bottom-right-radius: 5px; border-bottom-left-radius: 5px;\"></div> " +
                      "</div> " +
                      "</td></tr> " +
                      "<tr><td style=\"font-size: 11px; line-height: 16px; color: #777; padding: 25px 40px;\"> " +
                      "  This is an automatic service email generated by <a href=\"" + HostURL() + "/\" style=\"color: #57809E; text-decoration: none;\" >DuckRow.Net</a> for " + company + ".  " +
                      "  Please do not reply to this message. If you would like to contact " + company + " then use the details provided within the email. " +
                      "         </td></tr> " +
               "     </table> " +
               "    </td> " +
               "   </tr> " +
               "  </table> " +
               " </body> ";
            //" </html> ";

            return text;
        }




    }
}
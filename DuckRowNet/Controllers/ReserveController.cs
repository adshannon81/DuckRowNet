using DuckRowNet.Helpers.Object;
using DuckRowNet.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;

namespace DuckRowNet.Controllers
{
    public class ReserveController : Controller
    {
         [Authorize]
        public ActionResult Confirmation(string companyName = "", string classID = "")
        {

            var company = "";
            var id = "";
            CompanyDetails companyDetails;
            DAL db = new DAL();

            if (classID != "")
            {
                company = db.getCompanyFromClass(classID);
            }
            companyDetails = Authenticate.Validate(companyName);

            if (company == "DuckRow" || companyName != company)
            {
                Response.Redirect("~/", false);
            }

            ViewBag.CostOfCourse = "";
            ViewBag.CostOfSession = "";

            GroupClass classItem = db.selectClassDetail(classID);
            ViewBag.CostOfCourse = classItem.CostOfCourse.ToString("###0.00");
            ViewBag.CostOfSession = classItem.CostOfSession.ToString("###0.00");

            if (classItem.IsPrivate)
            {
                if (!Authenticate.IsUserInRole(companyDetails.Name, "basic"))
                {
                    Response.Redirect("~/Classes/" + companyName, false);
                }
            }

            ViewBag.SingleSession = false;
            ViewBag.SessionDate = DateTime.Now;
            var tempDate = Request.Params["date"];

            if (tempDate != null )
            {
                if (classItem.ClassDates.Any(d => d == Convert.ToDateTime(tempDate)))
                {
                    ViewBag.SingleSession = true;
                    ViewBag.SessionDate = Convert.ToDateTime(tempDate);
                }
            }

            if (Request.HttpMethod == "POST")
            {
                if (Request.Form["comments"] != null)
                {
                    ViewBag.Comments = Request.Form["comments"].ToString();
                }

                var guid = Guid.NewGuid().ToString();
                Guid bookingID = new Guid();


                if (!String.IsNullOrEmpty(Request.Form["entireCourse"]) && Request.Form["entireCourse"].ToString() == "no")
                {
                    ViewBag.SingleSession = true;
                    ViewBag.SessionDate = Convert.ToDateTime(Request.Form["dropInDate"]);
                }

                if (Authenticate.Admin(company))
                {

                    //add unconfirmed reservation to database

                    PersonDetails client = new PersonDetails();

                    //if client is not linked to user id then user client id

                    if (Request.Form["client"].StartsWith("c_"))
                    {
                        client.Type = Functions.PersonType.Client;
                        client.ID = Request.Form["client"].Substring(2);                        
                    }
                    else
                    {
                        client.Type = Functions.PersonType.User;
                        client.ID = Request.Form["client"];
                    }
                    client.GetDetails();


                    if (ViewBag.SingleSession)
                    {
                        bookingID = db.InsertBooking(classItem, client.ID, client.Email, "", Convert.ToDouble(ViewBag.CostOfSession), 0, 0,
                        true, guid, false, true, "Reservation", client.Type.ToString(), ViewBag.Comments, ViewBag.SessionDate.ToString("yyyy-MM-dd HH:mm:ss"));
                    }
                    else
                    {
                        bookingID = db.InsertBooking(classItem, client.ID, client.Email, "", Convert.ToDouble(ViewBag.CostOfCourse), 0, 0,
                        true, guid, false, true, "Reservation", client.Type.ToString(), ViewBag.Comments);
                    }


                    if (bookingID == new Guid())
                    {
                        ViewBag.UpdateSuccess = false;
                        ViewBag.Message = "Error updating the database - please try again later";
                        Logger.LogWarning("ReserveConfirmation", "InsertBooking", "", "booking id 0 returned");
                    }
                    else
                    {
                        DateTime classDate = Convert.ToDateTime(@classItem.StartDate.ToString("yyyy/MM/dd HH:mm:ss"));

                        //send reservation emails
                        //Client
                        if (ViewBag.SingleSession)
                        {
                            ViewBag.Message = Email.SendReservationConfirmed(client.FirstName, client.LastName,
                                client.Email, client.Type.ToString(), classItem, classItem.Name,
                                classItem.StartDate, classItem.Repeated.ToString(),
                                classItem.NumberOfLessons.ToString(), classItem.LocationName,
                                company,
                                comments: ViewBag.Comments,
                                sessionDate: ViewBag.SessionDate.ToString("yyyy-MM-dd HH:mm:ss"));
                        }
                        else
                        {
                            ViewBag.Message = Email.SendReservationConfirmed(client.FirstName, client.LastName,
                                client.Email, client.Type.ToString(), classItem, classItem.Name,
                                classItem.StartDate, classItem.Repeated.ToString(),
                                classItem.NumberOfLessons.ToString(), classItem.LocationName,
                                company, comments: ViewBag.Comments);
                        }

                        ViewBag.UpdateSuccess = true;


                        //Response.Redirect(Href("~/ReserveThankYou/" + company + "?id=" + guid + "&admin=false"));
                    }

                }
                else
                { //not admin user so must be customer request

                    var hostUrl = Request.Url.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped);

                    if (!db.isUserAClient(User.Identity.GetUserId(), company))
                    {
                        db.insertUserRole(User.Identity.GetUserId(), db.getRoleID(company));
                    }

                    if (ViewBag.SingleSession)
                    {
                        bookingID = db.InsertBooking(classItem, User.Identity.GetUserId(), User.Identity.Name, "", Convert.ToDouble(ViewBag.CostOfCourse), 0, 0,
                            classItem.AutoReservation, guid, false, true, "Reservation", "user", ViewBag.Comments, ViewBag.SessionDate.ToString("yyyy-MM-dd HH:mm:ss"));
                    }
                    else
                    {
                        bookingID = db.InsertBooking(classItem, User.Identity.GetUserId(), User.Identity.Name, "", Convert.ToDouble(ViewBag.CostOfCourse), 0, 0,
                            classItem.AutoReservation, guid, false, true, "Reservation", "user", ViewBag.Comments);
                    }

                    if ( bookingID == new Guid())
                    {
                        ViewBag.UpdateSuccess = false;
                        ViewBag.Message = "We're unable to process your request at the moment - please try again later or contact us directly";
                        Logger.LogWarning("ReserveConfirmation", "InsertBooking", "", "booking id 0 returned");
                    }
                    else
                    {
                        if (classItem.AutoReservation)
                        {
                            DateTime classDate = Convert.ToDateTime(@classItem.StartDate.ToString("yyyy/MM/dd HH:mm:ss"));

                            //send reservation emails
                            if (ViewBag.SingleSession)
                            {
                                //Client + company
                                ViewBag.Message = Email.SendReservationConfirmed(User.Identity.Name, "",
                                    User.Identity.Name, "user", classItem, classItem.Name,
                                    classItem.StartDate, classItem.Repeated.ToString(),
                                    classItem.NumberOfLessons.ToString(), classItem.LocationName,
                                    company, comments: ViewBag.Comments, sessionDate: ViewBag.SessionDate.ToString("yyyy-MM-dd HH:mm:ss"));
                            }
                            else {
                                //Client + company
                                ViewBag.Message = Email.SendReservationConfirmed(User.Identity.Name, "",
                                    User.Identity.Name, "user", classItem, classItem.Name,
                                    classItem.StartDate, classItem.Repeated.ToString(),
                                    classItem.NumberOfLessons.ToString(), classItem.LocationName,
                                    company, comments: ViewBag.Comments);
                            }
                            ViewBag.UpdateSuccess = true;
                        }
                        else
                        {
                            DateTime classDate = Convert.ToDateTime(@classItem.StartDate.ToString("yyyy/MM/dd HH:mm:ss"));
                            //send reservation emails
                            if (ViewBag.SingleSession)
                            {
                                //Client
                                ViewBag.Message = Email.SendRequest_Client(User.Identity.Name, "",
                                        User.Identity.Name, classItem, ViewBag.SessionDate.ToString("yyyy-MM-dd HH:mm:ss"));

                                //Company
                                ViewBag.Message = Email.SendRequest_Admin(User.Identity.Name, "", ViewBag.Comments, guid, classItem, ViewBag.SessionDate.ToString("yyyy-MM-dd HH:mm:ss"));
                            }
                            else
                            {
                                //Client
                                ViewBag.Message = Email.SendRequest_Client(User.Identity.Name, "",
                                        User.Identity.Name, classItem);

                                //Company
                                ViewBag.Message = Email.SendRequest_Admin(User.Identity.Name, "", ViewBag.Comments, guid, classItem);
                            }
                        }
                        Response.Redirect("~/ReserveThankYou/" + company + "?id=" + guid, false);
                    }

                }

                var imageFile = db.getCompanyImage(company);
                if (imageFile == null)
                {
                    imageFile = "http://www.duckrow.net/Images/CompanyImages/DuckRow-Logo.jpg";
                }


            }

            ViewBag.Clients = db.getClients(companyName);
            ViewBag.CompanyDetails = companyDetails;
            ViewBag.GClass = classItem;

            return View();
        }


    }
}
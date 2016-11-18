using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using DuckRowNet.Helpers.Object;
using WebMatrix.Data;
using Microsoft.AspNet.Identity;

namespace DuckRowNet.Helpers
{
    public class DAL
    {
        public DAL()
        { }

        private SqlConnection db;
        private Database _database;
        private bool isLive;


        #region "SQL Connection"

        private void connectDB_SQL()
        {
            string connStr = ConfigurationManager.ConnectionStrings["duckrowDB"].ToString();
            db = new SqlConnection(connStr);
            db.Open();

        }

        private void DBClose_SQL()
        {
            db.Close();
        }

        private static IEnumerable<T> GetSqlData_SQL<T>(string connectionstring, string sql) where T : new()
        {
            var properties = typeof(T).GetProperties();

            using (var conn = new SqlConnection(connectionstring))
            {
                using (var comm = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    using (var reader = comm.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var element = new T();

                            foreach (var f in properties)
                            {
                                var o = reader[f.Name];
                                if (o.GetType() != typeof(DBNull)) f.SetValue(element, o, null);
                            }
                            yield return element;
                        }
                    }
                    conn.Close();
                }
            }
        }

        public IEnumerable<T> runQuery_SQL<T>(string strSQL) where T : new()
        {
            try
            {
                connectDB_SQL();

                SqlCommand sqlCommand = new SqlCommand(strSQL, db);
                SqlDataReader sr = sqlCommand.ExecuteReader();

                var properties = typeof(T).GetProperties();

                while (sr.Read())
                {
                    var element = new T();

                    foreach (var f in properties)
                    {
                        var o = sr[f.Name];
                        if (o.GetType() != typeof(DBNull)) f.SetValue(element, o, null);
                    }
                    yield return element;
                }
            }
            finally
            {
                DBClose_SQL();
            }
        }

        private dynamic runQueryValue_SQL(string strSQL, List<string> paramList)
        {
            return runQueryValue_SQL(String.Format(strSQL, paramList));
        }

        private dynamic runQueryValue_SQL(string strSQL)
        {
            try
            {
                connectDB_SQL();

                SqlCommand sqlCommand = new SqlCommand(strSQL, db);
                return sqlCommand.ExecuteScalar();

            }
            catch (System.Exception e)
            {
                //Logger.LogWarning("runQueryValue", strSQL, "", e.Message);
                return null;
            }
            finally
            {
                DBClose_SQL();
            }

        }

        private bool insertQuery_SQL(string strSQL, List<string> paramList)
        {
            return insertQuery_SQL(String.Format(strSQL, paramList.ToArray()));
        }

        private bool insertQuery_SQL(string strSQL)
        {
            try
            {
                connectDB_SQL();
                SqlCommand sqlCommand = new SqlCommand(strSQL, db);
                sqlCommand.ExecuteNonQuery();

                return true;
            }
            catch (System.Exception e)
            {
                //Logger.LogWarning("InsertQuery", strSQL, "", e.Message);
                return false;
            }
            finally
            {
                DBClose_SQL();
            }
        }

        public int insertQueryGetID_SQL(string strSQL, string ID, List<string> paramList)
        {
            return insertQueryGetID_SQL(String.Format(strSQL, paramList.ToArray()), ID);
        }

        public int insertQueryGetID_SQL(string strSQL, string ID)
        {
            connectDB_SQL();

            strSQL += ";select scope_identity()";

            var i = 0;

            try
            {
                SqlCommand sqlCommand = new SqlCommand(strSQL, db);
                i = (int)sqlCommand.ExecuteScalar();
            }
            catch (System.Exception e)
            {
                //Logger.LogWarning("insertquerygetid", strSQL, "", e.Message);

                return 0;
            }
            finally
            {
                DBClose_SQL();
            }

            return i;
        }

        #endregion

        #region "WebMatrix Data"

        private void connectDB()
        {
            _database = Database.Open("duckrowDB");
        }

        private void DBClose()
        {
            _database.Close();
        }

        private IEnumerable<dynamic> runQuery(string strSQL, List<string> paramList)
        {
            try
            {
                connectDB();
                if (paramList == null)
                {
                    return _database.Query(strSQL);
                }
                else
                {
                    IEnumerable<dynamic> result = _database.Query(strSQL, paramList.ToArray());
                    return result;
                }
            }
            catch (System.Exception e)
            {
                var parameters = "";
                foreach (string item in paramList)
                {
                    parameters += ":" + item;
                }

                //Logger.LogWarning("runQuery", strSQL, parameters, e.Message);
                return null;
            }
            finally
            {
                DBClose();
            }
        }

        private dynamic runQueryValue(string strSQL, List<string> paramList)
        {
            try
            {
                connectDB();
                if (paramList == null)
                {
                    var result = _database.QueryValue(strSQL);
                    return result;
                }
                else
                {
                    var result = _database.QueryValue(strSQL, paramList.ToArray());
                    return result;
                }
            }
            catch (System.Exception e)
            {
                var parameters = "";
                foreach (string item in paramList)
                {
                    parameters += ":" + item;
                }

                //Logger.LogWarning("runQueryValue", strSQL, parameters, e.Message);
                return null;
            }
            finally
            {
                DBClose();
            }

        }

        private bool insertQuery(string strSQL, List<string> paramList)
        {
            try
            {
                connectDB();
                if (paramList == null)
                {
                    _database.Execute(strSQL);
                }
                else
                {
                    _database.Execute(strSQL, paramList.ToArray());
                }

                return true;
            }
            catch (System.Exception e)
            {
                var parameters = "";
                foreach (string item in paramList)
                {
                    parameters += ":" + item;
                }

                //Logger.LogWarning("InsertQuery", strSQL, parameters, e.Message);
                return false;
            }
            finally
            {
                DBClose();
            }
        }

        private bool insertLog(string strSQL, List<string> paramList)
        {
            try
            {
                connectDB();
                if (paramList == null)
                {
                    _database.Execute(strSQL);
                }
                else
                {
                    _database.Execute(strSQL, paramList.ToArray());
                }

                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                DBClose();
            }
        }

        //LIVE
        public dynamic insertQueryGetID(string strSQL, string ID, List<string> paramList)
        {
            if (isLive)
            {
                //LIVE
                connectDB();
                strSQL += ";select scope_identity()";

                var i = 0;

                try
                {
                    if (paramList == null)
                    {
                        i = _database.QueryValue(strSQL);
                    }
                    else
                    {
                        i = _database.QueryValue(strSQL, paramList.ToArray());
                    }
                }
                catch (System.Exception e)
                {
                    var parameters = "";
                    foreach (string item in paramList)
                    {
                        parameters += ":" + item;
                    }
                    //Logger.LogWarning("insertquerygetid", strSQL, parameters, e.Message);

                    return 0;
                }
                finally
                {
                    DBClose();
                }

                return i;
            }
            else
            {
                //Local
                connectDB();
                var i = 0;
                try
                {
                    if (paramList == null)
                    {
                        _database.Execute(strSQL);
                    }
                    else
                    {
                        _database.Execute(strSQL, paramList.ToArray());
                    }
                    var s = strSQL.Remove(0, strSQL.IndexOf("INTO ") + 5);
                    s = s.Remove(s.IndexOf(" "), s.Length - s.IndexOf(" "));
                    i = _database.QueryValue("Select MAX(" + ID + ") FROM " + s);
                }
                catch (System.Exception e)
                {
                    var parameters = "";
                    foreach (string item in paramList)
                    {
                        parameters += ":" + item;
                    }
                    //Logger.LogWarning("insertQueryGetID", strSQL, parameters, e.Message);
                    i = 0;
                }
                finally
                {
                    DBClose();
                }

                return i;
            }
        }

        public void logWarning(List<string> paramList)
        {
            insertLog("INSERT INTO WarningLogs (Title, Command, Parameters, Warning, Date) VALUES (@0,@1,@2,@3,@4)", paramList);
        }

        public string getID(string name, string table)
        {
            List<String> paramList = new List<string>();
            paramList.Add(name);

            String sqlQuery = "";
            switch (table)
            {
                case "Level":
                    sqlQuery = "select ID from Level WHERE name = @0";
                    break;
                case "User":
                    sqlQuery = "select userID from userProfile where email=@0";
                    break;
                case "Type":
                    sqlQuery = "select ID from Type where Name = @0";
                    break;
            }

            if (String.IsNullOrEmpty(sqlQuery))
                return "Error";

            var result = runQueryValue(sqlQuery, paramList);
            if (result != null)
                return result.ToString();
            else
                return "";
        }

        #endregion

        #region "User Details"

        public bool insertUserRole(string userID, string roleID)
        {
            List<String> paramList = new List<string>();
            paramList.Add(userID.ToString());
            paramList.Add(roleID.ToString());
            //check if user is already in role

            int found = (int)runQueryValue("SELECT Count(UserId) FROM AspNetUserRoles Where UserId = @0 and RoleId = @1", paramList);
            if (found == 1)
            {
                return true;
            }
            else
            {
                return insertQuery("INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@0, @1)", paramList);
                //return insertQuery(String.Format("INSERT INTO AspNetUserRoles VALUES ({0}, {1})", userID, roleID)); 
            }
        }

        public bool isUserInRole(string userID, string roleID)
        {
            List<String> paramList = new List<string>();
            paramList.Add(userID);
            paramList.Add(roleID);

            int found = (int)runQueryValue("SELECT Count(UserId) FROM AspNetUserRoles Where UserId = @0 and RoleId = @1", paramList);
            if (found == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool updateUserSettings(string userID, int calStartTime, int calEndTime, bool calActiveOnly, int calTimeSlot)
        {
            List<String> paramList = new List<string>();
            paramList.Add(userID);

            var count = (int)runQueryValue("SELECT Count(UserID) FROM UserSettings WHERE UserID = @0", paramList);

            if (count == 1)
            {
                paramList.Add(calStartTime.ToString());
                paramList.Add(calEndTime.ToString());
                paramList.Add(calActiveOnly.ToString());
                paramList.Add(calTimeSlot.ToString());

                return insertQuery("UPDATE UserSettings SET CalendarStartTime=@1, CalendarEndTime=@2, CalendarActiveOnly=@3," +
                "CalendarTimeSlot=@4 WHERE UserID=@0", paramList);
            }
            else
            {
                paramList.Add(calStartTime.ToString());
                paramList.Add(calEndTime.ToString());
                paramList.Add(calActiveOnly.ToString());
                paramList.Add(calTimeSlot.ToString());

                return insertQuery("INSERT INTO UserSettings (UserID, CalendarStartTime, CalendarEndTime, CalendarActiveOnly, CalendarTimeSlot ) " +
                    "VALUES (@0,@1,@2,@3,@4) ", paramList);
            }
        }

        public bool insertNewUser(string userID, string email)
        {
            List<string> paramList = new List<string>();
            paramList.Add(userID);
            paramList.Add(email);

            int found = (int)runQueryValue("SELECT Count(UserId) FROM UserProfile Where UserId = @0 and Email = @1", paramList);
            if (found == 0)
            {

                bool result = this.insertQuery("INSERT INTO UserProfile (UserId, Email) VALUES (@0, @1)", paramList);
                if (result)
                    return this.updateUserSettings(userID, 8, 20, false, 60);
                else
                    return false;
            }
            else
                return false;
        }

        public int getUserID(string email)
        {
            var id = getID(email, "User");
            if (String.IsNullOrEmpty(id) || id == "Error")
            {
                return 0;
            }
            else
            {
                return Convert.ToInt16(id);
            }
        }

        public string getUserEmail(string userID)
        {
            var user = getUserDetails(userID, "user");
            string email = user.Email;
            if (String.IsNullOrEmpty(email) || email == "Error")
            {
                return "";
            }
            else
            {
                return email;
            }
        }

        public PersonDetails getUserDetails(string email)
        {
            List<String> paramList = new List<string>();
            paramList.Add(email);

            //IEnumerable<dynamic> user = runQuery("SELECT * FROM UserProfile WHERE Email = @0", paramList);
            IEnumerable<dynamic> user = runQuery("SELECT AspNetUsers.Id as UserID, UserProfile.Firstname, UserProfile.Lastname, UserProfile.Phone, " +
                "UserProfile.Address1, UserProfile.Address2, UserProfile.City, UserProfile.State, UserProfile.Postcode, UserProfile.Country as CountryCode, " +
                "Countries.Name as Country, UserProfile.Email, Company.ID as CompanyID, Company.Name as CompanyName FROM UserProfile " +
                "INNER JOIN AspNetUsers ON AspNetUsers.Email = UserProfile.Email " +
                "Left JOIN Countries ON UserProfile.Country = Countries.IsoCode2 " +
                "LEFT JOIN Company on UserProfile.Company = Company.ID " +
                "WHERE UserProfile.Email = @0", paramList);

            if (user.Count() != 0)
            {
                return new PersonDetails(user.ElementAt(0).UserID.ToString(), user.ElementAt(0).FirstName, user.ElementAt(0).LastName,
                    user.ElementAt(0).CompanyID, user.ElementAt(0).CompanyName, user.ElementAt(0).Address1, user.ElementAt(0).Address2, "",
                    user.ElementAt(0).City, user.ElementAt(0).State, user.ElementAt(0).Postcode, user.ElementAt(0).Country,
                    user.ElementAt(0).Phone, user.ElementAt(0).Email, Functions.PersonType.User);
            }
            else { return null; };
        }

        public PersonDetails getUserDetails(string userID, string userType)
        {
            List<String> paramList = new List<string>();
            paramList.Add(userID);

            IEnumerable<dynamic> user;
            Functions.PersonType type = Functions.PersonType.Client;

            if (userType == "client")
            {
                user = runQuery("SELECT * FROM Clients WHERE ID = @0", paramList);
            }
            else
            {
                user = runQuery("SELECT UserProfile.UserId, UserProfile.Firstname, UserProfile.Lastname, UserProfile.Phone, UserProfile.Address1, " + 
                    "UserProfile.Address2, UserProfile.City, UserProfile.State, UserProfile.Postcode, UserProfile.Country as CountryCode, " +
                    " Countries.Name as Country, UserProfile.Email, Company.ID as CompanyID, Company.Name as CompanyName FROM UserProfile " +
                    "LEFT JOIN Countries ON UserProfile.Country = Countries.IsoCode2 " +
                    "LEFT JOIN Company on UserProfile.Company = Company.ID " +
                    "WHERE UserID = @0", paramList);
                type = Functions.PersonType.User;
            }

            if (user.Count() != 0)
            {
                return new PersonDetails(user.ElementAt(0).UserId.ToString(), user.ElementAt(0).FirstName, user.ElementAt(0).LastName,
                    user.ElementAt(0).CompanyID, user.ElementAt(0).CompanyName,
                    user.ElementAt(0).Address1, user.ElementAt(0).Address2, "", 
                    user.ElementAt(0).City, user.ElementAt(0).State, user.ElementAt(0).Postcode, user.ElementAt(0).Country,
                    user.ElementAt(0).Phone, user.ElementAt(0).Email, type);
            }
            else { return null; };

        }

        public IEnumerable<dynamic> getUserSchedule(string userID)
        {
            List<string> paramList = new List<string>();
            paramList.Add(userID);
            return runQuery("SELECT Class.ID, Class.Name, Class.MaxCapacity, Class.CostPerPerson, Class.StartDate,  " +
                "Class.ClassDuration, Location.Name As Location, Class.Description, Class.ImageURL, Class.Company, Level.Name as LevelName, " +
                "UserProfile.Firstname + ' ' + UserProfile.Lastname as AdminName, " +
                "Category.ID AS CategoryID, Category.Name AS CategoryName, SubCategory.ID AS SubCategoryID, SubCategory.Name AS SubCategoryName, " +
                "Schedule.Date as ScheduleDate FROM Class LEFT JOIN Level ON Class.LevelID = Level.ID  " +
                "INNER JOIN SubCategory ON Class.SubCategoryID = SubCategory.ID " +
                "INNER JOIN Category ON SubCategory.CategoryID = Category.ID " +
                "INNER JOIN Schedule ON Class.ID = Schedule.ClassID INNER JOIN UserProfile " +
                "ON Class.AdminID = UserProfile.UserID INNER JOIN Location ON Class.LocationID = Location.ID " +
                "WHERE (AdminID = @2 OR AdminID2 = @2 OR AdminID3 = @2 OR AdminID4 = @2 OR AdminID5 = @2 ) " +
                "ORDER BY ScheduleDate",
                paramList);
        }

        public IEnumerable<dynamic> getUserSchedule(string userID, DateTime startDate, DateTime endDate, String companyName = "")
        {
            List<string> paramList = new List<string>();
            paramList.Add(startDate.ToString("yyyy-MM-dd HH:mm"));
            paramList.Add(endDate.ToString("yyyy-MM-dd HH:mm"));
            paramList.Add(userID);

            string strSQL = "SELECT Class.ID, Class.Name, Class.MaxCapacity, Class.CostPerPerson, Class.StartDate,  " +
                "Class.ClassDuration, Location.Name As Location, Class.Description, Class.ImageURL, Level.Name as LevelName, " +
                "UserProfile.Firstname + ' ' + UserProfile.Lastname as AdminName, " +
                "Category.ID AS CategoryID, Category.Name AS CategoryName, SubCategory.ID AS SubCategoryID, SubCategory.Name AS SubCategoryName, " +
                "Schedule.Date as ScheduleDate FROM Class LEFT JOIN Level ON Class.LevelID = Level.ID  " +
                "INNER JOIN SubCategory ON Class.SubCategoryID = SubCategory.ID " +
                "INNER JOIN Category ON SubCategory.CategoryID = Category.ID " +
                "INNER JOIN Schedule ON Class.ID = Schedule.ClassID INNER JOIN UserProfile " +
                "ON Class.AdminID = UserProfile.UserID INNER JOIN Location ON Class.LocationID = Location.ID " +
                "WHERE (AdminID = @2 OR AdminID2 = @2 OR AdminID3 = @2 OR AdminID4 = @2 OR AdminID5 = @2 ) " +
                "AND Schedule.Date Between @0 AND @1 ";

            if (companyName != "")
            {
                paramList.Add(companyName);
                strSQL += " AND Class.Company = @3 ";
            }
            strSQL += " ORDER BY ScheduleDate";

            return runQuery(strSQL, paramList);
        }

        public IEnumerable<dynamic> getUserDaySchedule(string userID, DateTime startDate, String companyName = "")
        {
            List<string> paramList = new List<string>();
            paramList.Add(startDate.ToString("yyyy-MM-dd 00:00:00"));
            paramList.Add(startDate.ToString("yyyy-MM-dd 23:59:59"));
            paramList.Add(userID);
            var strSQL = "SELECT Class.ID, Class.Name, Class.MaxCapacity, Class.CostPerPerson, Class.StartDate,  " +
                "Class.ClassDuration, Location.Name As Location, Class.Description, Class.ImageURL, Level.Name as LevelName, " +
                "UserProfile.Firstname + ' ' + UserProfile.Lastname as AdminName, " +
                "Category.ID AS CategoryID, Category.Name AS CategoryName, SubCategory.ID AS SubCategoryID, SubCategory.Name AS SubCategoryName, " +
                "Schedule.Date as ScheduleDate FROM Class LEFT JOIN Level ON Class.LevelID = Level.ID  " +
                "INNER JOIN SubCategory ON Class.SubCategoryID = SubCategory.ID " +
                "INNER JOIN Category ON SubCategory.CategoryID = Category.ID " +
                "INNER JOIN Schedule ON Class.ID = Schedule.ClassID INNER JOIN UserProfile " +
                "ON Class.AdminID = UserProfile.UserID INNER JOIN Location ON Class.LocationID = Location.ID " +
                "WHERE (AdminID = @2 OR AdminID2 = @2 OR AdminID3 = @2 OR AdminID4 = @2 OR AdminID5 = @2 ) " +
                "AND Schedule.Date Between @0 AND @1 ";

            if (companyName != "")
            {
                paramList.Add(companyName);
                strSQL += " AND Class.Company = @2 ";
            }
            strSQL += "ORDER BY ScheduleDate";
            return runQuery(strSQL, paramList);
        }

        public IEnumerable<dynamic> getUserSchedule(string userID, string subCategoryName)
        {
            List<string> paramList = new List<string>();
            paramList.Add(userID);
            paramList.Add(subCategoryName);
            return runQuery("SELECT Class.ID, Class.Name, Class.MaxCapacity, Class.CostPerPerson, Class.StartDate,  " +
                "Class.ClassDuration, Location.Name as Location, Class.Description, Class.ImageURL, Level.Name as LevelName, " +
                "UserProfile.Firstname + ' ' + UserProfile.Lastname as AdminName, " +
                "Category.ID AS CategoryID, Category.Name AS CategoryName, SubCategory.ID AS SubCategoryID, SubCategory.Name AS SubCategoryName, " +
                "Schedule.Date as ScheduleDate FROM Class LEFT JOIN Level ON Class.LevelID = Level.ID  " +
                "INNER JOIN SubCategory ON Class.SubCategoryID = SubCategory.ID " +
                "INNER JOIN Category ON SubCategory.CategoryID = Category.ID " +
                "INNER JOIN Schedule ON Class.ID = Schedule.ClassID INNER JOIN UserProfile " +
                "ON Class.AdminID = UserProfile.UserID INNER JOIN Location ON Class.LocationID = Location.ID " +
                "WHERE (AdminID = @0 OR AdminID2 = @0 OR AdminID3 = @0 OR AdminID4 = @0 OR AdminID5 = @0 )  AND SubCategory.Name = @1   " +
                "AND Schedule.Date >= GETDATE() ORDER BY ScheduleDate",
                paramList);
        }

        public bool checkClientIsFree(string start, string duration, string repeatType, string numberOfRepeats, string userID, string companyName)
        {
            return checkClientIsFree(start, duration, repeatType, numberOfRepeats, userID, companyName, "");
        }

        public bool checkClientIsFree(string start, string duration, string repeatType, string numberOfRepeats, string userID, string companyName, string excludeClassID)
        {

            //same process can be used for single, daily and weekly.   
            if (repeatType == "Week")
            {
                int count = Convert.ToInt16(numberOfRepeats);

                DateTime dtBeginStart, dtBeginEnd, dtStart, dtEnd;

                dtBeginStart = Convert.ToDateTime(start);
                dtBeginEnd = dtBeginStart.AddMinutes(Convert.ToInt16(duration));

                for (int i = 0; i < count; i++)
                {
                    dtStart = dtBeginStart.AddDays((i * 7));
                    dtEnd = dtBeginEnd.AddDays((i * 7));

                    var schedule = getUserDaySchedule(userID, dtStart, companyName); //, dtEnd);

                    DateTime dtTempStart;
                    DateTime dtTempEnd;

                    foreach (var slot in schedule)
                    {
                        dtTempStart = slot.ScheduleDate;
                        dtTempEnd = dtTempStart.AddMinutes(slot.ClassDuration);

                        if (dtTempStart >= dtStart && dtTempStart < dtEnd && slot.ID.ToString() != excludeClassID)
                        {
                            return false;
                        }
                        else if (dtTempEnd > dtStart && dtTempEnd < dtEnd && slot.ID.ToString() != excludeClassID)
                        {
                            return false;
                        }
                    }

                }
            }
            else if (repeatType == "Day" || repeatType == "never")
            {
                int count = Convert.ToInt16(numberOfRepeats);

                DateTime dtBeginStart, dtBeginEnd, dtStart, dtEnd;

                dtBeginStart = Convert.ToDateTime(start);
                dtBeginEnd = dtBeginStart.AddMinutes(Convert.ToInt16(duration));

                for (int i = 0; i < count; i++)
                {
                    dtStart = dtBeginStart.AddDays(i);
                    dtEnd = dtBeginEnd.AddDays(i);

                    var schedule = getUserDaySchedule(userID, dtStart, companyName);

                    DateTime dtTempStart;
                    DateTime dtTempEnd;

                    foreach (var slot in schedule)
                    {
                        dtTempStart = slot.ScheduleDate;
                        dtTempEnd = dtTempStart.AddMinutes(slot.ClassDuration);
                        var ii = slot.ID.ToString();

                        if (dtTempStart >= dtStart && dtTempStart < dtEnd && slot.ID.ToString() != excludeClassID)
                        {
                            //if (dtTempEnd > dtStart && dtTempEnd <= dtEnd)
                            //{
                            return false;
                            //}
                        }
                        else if (dtTempEnd > dtStart && dtTempEnd < dtEnd && slot.ID.ToString() != excludeClassID)
                        {
                            return false;
                        }
                    }

                }
            }
            else //Monthly
            {
                int count = Convert.ToInt16(numberOfRepeats);

                DateTime dtBeginStart, dtBeginEnd, dtStart, dtEnd;

                dtBeginStart = Convert.ToDateTime(start);
                dtBeginEnd = dtBeginStart.AddMinutes(Convert.ToInt16(duration));

                for (int i = 0; i < count; i++)
                {
                    dtStart = dtBeginStart.AddMonths(i);
                    dtEnd = dtBeginEnd.AddMonths(i);

                    var schedule = getUserDaySchedule(userID, dtStart, companyName);

                    DateTime dtTempStart;
                    DateTime dtTempEnd;

                    foreach (var slot in schedule)
                    {
                        dtTempStart = slot.ScheduleDate;
                        dtTempEnd = dtTempStart.AddMinutes(slot.ClassDuration);

                        if (dtTempStart > dtStart && dtTempStart < dtEnd && slot.ID.ToString() != excludeClassID)
                        {
                            //if (dtTempEnd > dtStart && dtTempEnd <= dtEnd)
                            //{
                            return false;
                            //}
                        }
                        else if (dtTempEnd > dtStart && dtTempEnd < dtEnd && slot.ID.ToString() != excludeClassID)
                        {
                            return false;
                        }
                    }

                }

            }

            return true;
        }

        public bool checkClientIsFree(GroupClass gClass, string userID, string companyName)
        {
            return checkClientIsFree(gClass, userID, companyName, "");
        }

        public bool checkClientIsFree(GroupClass gClass, string userID, string companyName, string excludeClassID)
        {
            List<DateTime> classDates = Functions.getClassDates(gClass);

            foreach (DateTime dtStart in classDates)
            {
                DateTime dtEnd = dtStart.AddMinutes(gClass.Duration);

                var schedule = getUserDaySchedule(userID, dtStart, companyName);
                DateTime dtTempStart;
                DateTime dtTempEnd;

                foreach (var slot in schedule)
                {
                    dtTempStart = slot.ScheduleDate;
                    dtTempEnd = dtTempStart.AddMinutes(slot.ClassDuration);

                    if (dtTempStart >= dtStart && dtTempStart < dtEnd && slot.ID.ToString() != excludeClassID)
                    {
                        return false;
                    }
                    else if (dtTempEnd > dtStart && dtTempEnd < dtEnd && slot.ID.ToString() != excludeClassID)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public List<DateTime> hasUserBookedClass(string classID, string userID)
        {
            List<string> paramList = new List<string>();
            paramList.Add(classID);
            paramList.Add(userID);
            var result = runQuery("SELECT DISTINCT Schedule.Date as ScheduleDate FROM Schedule " +
                "INNER JOIN Booking ON Schedule.ID = Booking.ScheduleID " +
                "INNER JOIN BookingDetails ON Booking.BookingID = BookingDetails.ID " +
                "WHERE BookingDetails.ClassID = @0 AND BookingDetails.UserID = @1 AND " +
                "BookingDetails.cancelled = 'False' AND Booking.Cancelled = 'False'", paramList);

            List<DateTime> dList = new List<DateTime>();
            foreach (var item in result)
            {
                dList.Add(Convert.ToDateTime(item.ScheduleDate));
            }
            return dList;
        }

        public string getRoleID(string roleName)
        {
            List<string> paramList = new List<string>();
            paramList.Add(roleName);
            return (string)runQueryValue("SELECT Id FROM AspNetRoles WHERE Name = @0", paramList);
        }

        public bool updateRole(string userID, string roleName, string company)
        {
            var roleID = getRoleID(roleName + "_" + company);
            bool success = false;

            //delete any other roles for the company
            success = deleteRoles(userID, company);

            if (success)
            {
                //add the new role
                success = insertUserRole(userID, roleID);
            }
            return success;
        }

        public bool AddFavourite(string userID, string classID)
        {
            List<string> paramList = new List<string>();
            paramList.Add(userID);
            paramList.Add(classID);
            return insertQuery("INSERT INTO UserFavourites(UserId, ClassId) VALUES (@0,@1)", paramList);
        }

        public bool RemoveFavourite(string userID, string classID)
        {
            List<string> paramList = new List<string>();
            paramList.Add(userID);
            paramList.Add(classID);
            return insertQuery("DELETE FROM UserFavourites WHERE UserId=@0 AND ClassId=@1", paramList);
        }

        public List<GroupClass> getUserFavourites(string userID)
        {
            List<string> paramList = new List<string>();
            paramList.Add(userID);


            var tempClass = runQuery("SELECT Class.ID, Class.Name, Class.Description, " +
                "CONVERT(NVARCHAR(50),Category.ID) AS CategoryID, Category.Name AS CategoryName, CONVERT(NVARCHAR(50),SubCategory.ID) AS SubCategoryID, SubCategory.Name AS SubCategoryName, " +
                "CONVERT(NVARCHAR(50),Level.ID) AS LevelID, Level.Name AS LevelName, " +
                "Class.StartDate, Class.EndDate, Class.ClassDuration, Class.NumberOfWeeks, Class.Repeated, Class.NumberOfLessons, " +
                "Class.RepeatFrequency, " +
                "Class.DaysOfWeek, round(Class.CostPerPerson, 2) as CostPerPerson, round(Class.CostOFSession, 2) as CostOfSession, " +
                "Class.ImageURL, Class.MaxCapacity, " +
                "Class.Company, Company.ImagePath as CompanyImage, Class.Private, Class.IsCourse, Class.ClassType, " +
                "Class.AdminID, Class.AdminID2, Class.AdminID3, Class.AdminID4, Class.AdminID5, " +
                "UserProfile.Firstname + ' ' + UserProfile.Lastname AS Admin, " +
                "Company.Phone as ContactPhone, Company.Email as ContactEmail, " +
                "Class.AllowDropIn, Class.AbsorbFee, Class.AllowReservation, Class.AutoReservation, Class.AllowPayment, Class.Private, " +
                "Location.ID as LocationID, Location.Name As LocName, Location.State as LocState, Location.Longitude as LocLng, Location.Latitude as LocLat, " +
                "UserFavourites.UserId as Fav " +
                "FROM Class INNER JOIN SubCategory ON Class.SubCategoryID = SubCategory.ID  " +
                "INNER JOIN Category ON SubCategory.CategoryID = Category.ID " +
                "INNER JOIN UserProfile ON Class.AdminID = UserProfile.userID " +
                "INNER JOIN Location ON Class.LocationID = Location.ID " +
                "INNER JOIN Company ON Class.Company = Company.Name " +
                "LEFT JOIN Level ON Class.LevelID = Level.ID " +
                "INNER JOIN UserFavourites ON (UserFavourites.ClassId = Class.ID AND UserFavourites.UserId = @0 ) ", paramList);

            List<GroupClass> userFavourites = new List<GroupClass>();

            for (int i = 0; i < tempClass.Count(); i++)
            {
                string adminList = tempClass.ElementAt(i).AdminID.ToString();
                if (tempClass.ElementAt(i).AdminID2 != null)
                { adminList += "," + tempClass.ElementAt(i).AdminID2.ToString(); }
                if (tempClass.ElementAt(i).AdminID3 != null)
                { adminList += "," + tempClass.ElementAt(i).AdminID3.ToString(); }
                if (tempClass.ElementAt(i).AdminID4 != null)
                { adminList += "," + tempClass.ElementAt(i).AdminID4.ToString(); }
                if (tempClass.ElementAt(i).AdminID5 != null)
                { adminList += "," + tempClass.ElementAt(i).AdminID5.ToString(); }

                bool fav = false;
                if (!String.IsNullOrEmpty(tempClass.ElementAt(i).Fav))
                {
                    fav = true;
                }

                GroupClass gClass = new GroupClass(new Guid(tempClass.ElementAt(i).ID.ToString()), tempClass.ElementAt(i).Company, tempClass.ElementAt(i).CompanyImage, tempClass.ElementAt(i).Name,
                    tempClass.ElementAt(i).CategoryID, tempClass.ElementAt(i).CategoryName, tempClass.ElementAt(i).SubCategoryID, tempClass.ElementAt(i).SubCategoryName,
                    //tempClass.ElementAt(i).LevelID, tempClass.ElementAt(i).LevelName,
                    Convert.ToInt16(tempClass.ElementAt(i).ClassDuration), tempClass.ElementAt(i).Repeated,
                    tempClass.ElementAt(i).RepeatFrequency, tempClass.ElementAt(i).DaysOfWeek,
                    Convert.ToInt16(tempClass.ElementAt(i).NumberOfLessons), Convert.ToDateTime(tempClass.ElementAt(i).StartDate), Convert.ToInt16(tempClass.ElementAt(i).MaxCapacity),
                    Convert.ToDouble(tempClass.ElementAt(i).CostPerPerson), Convert.ToDouble(tempClass.ElementAt(i).CostOfSession),
                    adminList.Split(','), tempClass.ElementAt(i).Admin,
                    tempClass.ElementAt(i).LocationID, tempClass.ElementAt(i).LocName, tempClass.ElementAt(i).LocState,
                    tempClass.ElementAt(i).LocLng, tempClass.ElementAt(i).LocLng,
                    tempClass.ElementAt(i).Description, tempClass.ElementAt(i).ImageURL,
                    Convert.ToBoolean(tempClass.ElementAt(i).IsCourse),
                    Convert.ToBoolean(tempClass.ElementAt(i).AllowDropIn), Convert.ToBoolean(tempClass.ElementAt(i).AbsorbFee),
                    Convert.ToBoolean(tempClass.ElementAt(i).AllowReservation), Convert.ToBoolean(tempClass.ElementAt(i).AutoReservation),
                    Convert.ToBoolean(tempClass.ElementAt(i).AllowPayment),
                    Convert.ToBoolean(tempClass.ElementAt(i).Private), tempClass.ElementAt(i).ClassType, fav);

                if (tempClass.ElementAt(i).Admin == null || String.IsNullOrEmpty(tempClass.ElementAt(i).Admin.ToString()))
                {
                    gClass.ContactName = tempClass.ElementAt(i).Company;
                }
                else
                {
                    gClass.ContactName = tempClass.ElementAt(i).Admin.ToString();
                }
                if (tempClass.ElementAt(i).ContactPhone != null && !String.IsNullOrEmpty(tempClass.ElementAt(i).ContactPhone.ToString()))
                {
                    gClass.Phone = tempClass.ElementAt(i).ContactPhone.ToString();
                }
                gClass.Email = tempClass.ElementAt(i).ContactEmail.ToString();

                userFavourites.Add(gClass);

            }

            return userFavourites;
        }


        #endregion

        #region "Client Details"

        public bool UpdateClient(PersonDetails client, string companyName)
        {
            return UpdateClient(client.ID, client.FirstName, client.LastName, client.Email, client.Phone, client.Address1, client.Address2, client.City,
                client.State, client.Country, client.Postcode, companyName);
        }

        public bool UpdateClient(string clientID, string firstName, string lastName, string email, string phone, string address1, string address2,
        string city, string state, string country, string postcode, string company)
        {
            List<string> paramList = new List<string>();
            paramList.Add(clientID);

            var count = (int)runQueryValue("Select count(ID) from Clients WHERE ID = @0", paramList);
            string strSQL = "";

            if (count == 0)
            {
                paramList = new List<string>();
                paramList.Add(Guid.NewGuid().ToString());
                paramList.Add(firstName);
                paramList.Add(lastName);
                paramList.Add(email);
                paramList.Add(phone);
                paramList.Add(address1);
                paramList.Add(address2);
                paramList.Add(city);
                paramList.Add(state);
                paramList.Add(country);
                paramList.Add(postcode);
                paramList.Add(company);
                strSQL = "INSERT INTO Clients (ID, Firstname, Lastname, Email, Phone, Address1, Address2, City, State, Country, Postcode," +
                    "Company) VALUES (@0,@1,@2,@3,@4,@5,@6,@7,@8,@9,@10,@11)";
            }
            else
            {
                paramList = new List<string>();
                paramList.Add(firstName);
                paramList.Add(lastName);
                paramList.Add(email);
                paramList.Add(phone);
                paramList.Add(address1);
                paramList.Add(address2);
                paramList.Add(city);
                paramList.Add(state);
                paramList.Add(country);
                paramList.Add(postcode);
                paramList.Add(clientID);
                strSQL = "UPDATE Clients SET Firstname = @0, Lastname = @1, Email = @2, Phone = @3, Address1 = @4, " +
                    "Address2 = @5, City = @6, State = @7, Country = @8, Postcode = @9 WHERE ID = @10 ";
            }

            return insertQuery(strSQL, paramList);
        }

        public IEnumerable<dynamic> getClientDetails(string clientID, string clientType)
        {
            List<string> paramList = new List<string>();
            paramList.Add(clientID);

            if (clientType == "client")
            {
                return runQuery("SELECT Clients.ID, Clients.Firstname, Clients.Lastname, Clients.Phone, Clients.Address1, Clients.Address2, Clients.City, " +
                    "Clients.State, Clients.Postcode, Clients.Country as CountryCode, Countries.Name as Country, Clients.Email FROM Clients Left JOIN Countries ON Clients.Country = Countries.IsoCode2 " +
                    "WHERE Clients.ID = @0", paramList);
            }
            else
            {
                return runQuery("SELECT UserProfile.UserId, UserProfile.Firstname, UserProfile.Lastname, UserProfile.Phone, UserProfile.Address1, UserProfile.Address2, UserProfile.City, " +
                    "UserProfile.State, UserProfile.Postcode, UserProfile.Country as CountryCode, Countries.Name as Country, UserProfile.Email  FROM UserProfile LEFT JOIN Countries ON UserProfile.Country = Countries.IsoCode2 " +
                    "WHERE UserID = @0", paramList);
            }

        }

        public PersonDetails getClientDetails(PersonDetails client)
        {
            List<string> paramList = new List<string>();
            paramList.Add(client.ID);

            IEnumerable<dynamic> results;

            if (client.Type == Functions.PersonType.Client)
            {
                results = runQuery("SELECT Clients.ID, Clients.Firstname, Clients.Lastname, Clients.Phone, Clients.Address1, Clients.Address2, Clients.City, " +
                    "Clients.State, Clients.Postcode, Clients.Country as CountryCode, Countries.Name as Country, Clients.Email, Company.ID as CompanyID, Company.Name as CompanyName FROM Clients Left JOIN Countries ON Clients.Country = Countries.IsoCode2 " +
                    "LEFT JOIN Company on Clients.Company = Company.ID " +
                    "WHERE Clients.ID = @0", paramList);
            }
            else
            {
                results = runQuery("SELECT UserProfile.UserId as ID, UserProfile.Firstname, UserProfile.Lastname, UserProfile.Phone, UserProfile.Address1, UserProfile.Address2, UserProfile.City, " +
                    "UserProfile.State, UserProfile.Postcode, UserProfile.Country as CountryCode, Countries.Name as Country, UserProfile.Email, Company.ID as CompanyID, Company.Name as CompanyName FROM UserProfile LEFT JOIN Countries ON UserProfile.Country = Countries.IsoCode2 " +
                    "LEFT JOIN Company on UserProfile.Company = Company.ID " + 
                    "WHERE UserID = @0", paramList);
            }

            client = new PersonDetails(results.ElementAt(0).ID.ToString(), results.ElementAt(0).Firstname, results.ElementAt(0).Lastname, 
                results.ElementAt(0).CompanyID, results.ElementAt(0).CompanyName,
                results.ElementAt(0).Address1, results.ElementAt(0).Address2, "", results.ElementAt(0).City, results.ElementAt(0).State, results.ElementAt(0).Postcode,
                results.ElementAt(0).Country, results.ElementAt(0).Phone, results.ElementAt(0).Email, client.Type);


            return client;

        }

        public IEnumerable<dynamic> getClientSchedule(PersonDetails client, string company)
        {
            List<string> paramList = new List<string>();
            paramList.Add(company);
            paramList.Add(client.ID);
            paramList.Add(client.Type.ToString());

            if (client.Type == Functions.PersonType.User)
            {
                return runQuery("SELECT DISTINCT BookingDetails.UserID, BookingDetails.ClassID, UserProfile.Email, " +
                    "UserProfile.Firstname, UserProfile.Lastname, Class.Name as ClassName, " +
                "Category.ID AS CategoryID, Category.Name AS CategoryName, SubCategory.ID AS SubCategoryID, SubCategory.Name AS SubCategoryName, " +
                "Level.Name as LevelName, " +
                    "Schedule.Date, Location.Name as Location, BookingDetails.confirmed, BookingDetails.paid FROM BookingDetails " +
                    "INNER JOIN Class on BookingDetails.ClassID = Class.ID " +
                    "INNER JOIN UserProfile ON BookingDetails.UserID = UserProfile.UserID " +
                    "INNER JOIN SubCategory ON Class.SubCategoryID = SubCategory.ID " +
                    "INNER JOIN Category ON SubCategory.CategoryID = Category.ID " +
                    "LEFT JOIN Level ON Class.LevelID = Level.ID " +
                    "INNER JOIN Location on Class.LocationID = Location.ID " +
                    "INNER JOIN Schedule on Class.ID = Schedule.ClassID " +
                    "WHERE Class.Company = @0 AND BookingDetails.UserID = @1 AND BookingDetails.UserType = @2 " +
                    "AND BookingDetails.cancelled = 'False' AND Schedule.Date > dateadd(day,-1,getdate()) " +
                    "ORDER BY Schedule.Date", paramList);
            }
            {

                return runQuery("SELECT DISTINCT BookingDetails.UserID, BookingDetails.ClassID, Clients.Email, " +
                    "Clients.Firstname, Clients.Lastname, Class.Name as ClassName, " +
                    "Category.ID AS CategoryID, Category.Name AS CategoryName, SubCategory.ID AS SubCategoryID, SubCategory.Name AS SubCategoryName, " +
                    "Level.Name as LevelName, " +
                    "Schedule.Date, Location.Name as Location, BookingDetails.confirmed, BookingDetails.paid FROM BookingDetails " +
                    "INNER JOIN Class on BookingDetails.ClassID = Class.ID " +
                    "INNER JOIN Clients ON BookingDetails.UserID = Clients.ID " +
                    "INNER JOIN SubCategory ON Class.SubCategoryID = SubCategory.ID " +
                    "INNER JOIN Category ON SubCategory.CategoryID = Category.ID " +
                    "LEFT JOIN Level ON Class.LevelID = Level.ID " +
                    "INNER JOIN Location on Class.LocationID = Location.ID " +
                    "INNER JOIN Schedule on Class.ID = Schedule.ClassID " +
                    "WHERE Class.Company = @0 AND BookingDetails.UserID = @1 AND BookingDetails.UserType = @2 " +
                    "AND BookingDetails.cancelled = 'False' AND Schedule.Date > dateadd(day,-1,getdate()) " +
                    "ORDER BY Schedule.Date", paramList);
            }

        }

        public bool deleteClient(string clientID, string company)
        {
            List<String> paramList = new List<string>();
            paramList.Add(clientID);
            paramList.Add(company);
            return insertQuery("DELETE FROM Clients WHERE ID = @0 AND Company = @1", paramList);
        }

        public bool isUserAClient(string userID, string company)
        {
            List<string> paramList = new List<string>();
            paramList.Add(userID);
            paramList.Add(company);
            string result = runQueryValue("SELECT AspNetRoles.Name FROM " +
                "UserProfile JOIN AspNetUserRoles ON AspNetUserRoles.UserId = UserProfile.UserID " +
                "INNER JOIN AspNetRoles ON AspNetUserRoles.RoleId = AspNetRoles.Id " +
                "WHERE UserProfile.UserID =@0 and AspNetRoles.Company = @1  ", paramList);
            if (result == null)
            {
                return false;
            }
            return true;
        }

        #endregion

        #region "Company Details"

        public bool checkCompanyExists(string companyName)
        {
            List<string> paramList = new List<string>();
            paramList.Add(companyName);
            if (companyName == "DuckRow")
            {
                return true;
            }
            //check against other company names
            var company = runQueryValue("SELECT Name FROM Company WHERE Name = @0", paramList);
            if (!String.IsNullOrEmpty(company))
                return true;

            //check against category names
            company = runQueryValue("SELECT Name From Category WHERE Name = @0", paramList);
            if (!String.IsNullOrEmpty(company))
                return true;

            //check against sub-category names
            company = runQueryValue("SELECT Name From SubCategory WHERE Name = @0", paramList);
            if (!String.IsNullOrEmpty(company))
                return true;

            return false;
        }

        public string insertCompanyRole(string roleName, string companyName)
        {
            Guid roleID = Guid.NewGuid();

            List<String> paramList = new List<string>();
            paramList.Add(roleID.ToString());
            paramList.Add(roleName);

            var ID = runQueryValue("SELECT Id FROM AspNetRoles WHERE Name = @0", paramList);
            if (ID == null)
            {
                paramList.Add(companyName);
                insertQuery("INSERT INTO AspNetRoles (Id, Name, Company) VALUES (@0,@1,@2)", paramList);
            }
            else
            {
                return ID;
            }
            return roleID.ToString();
        }
        
        public CompanyDetails getCompanyDetails(string companyName)
        {
            List<string> paramList = new List<string>();
            paramList.Add(companyName);
            IEnumerable<dynamic> results = runQuery("SELECT Company.ID, Company.Name,Company.URL, Company.ImagePath, Company.PaypalEmail,Company.Phone, Company.Email, " +
                "Company.Address1, Company.Address2, Company.FaceBookURL, Company.Country, Company.Style, Company.Description, " +
                    "Company.PaypalAbsorbFees, Company.City, Company.State, Company.Postcode, Company.Type, '2500/12/31' as NextPaymentDate," + //Subscriptions.NextPaymentDate, " +
                    "CompanyType.Name SubName, CompanyType.MaxUsers, CompanyType.MaxAdverts, CompanyType.MaxClasses, CompanyType.MonthlyCost, " +
                    "Company.IsClub " +
                    "FROM Company  LEFT JOIN CompanyType on Company.Type = CompanyType.ID " +
                    "LEFT JOIN Subscriptions ON (Subscriptions.CompanyID = Company.ID AND Subscriptions.ProfileStatus = 'Active') " +
                "WHERE Company.Name = @0", paramList);
            if (results.Count() == 0)
            {
                return null;
            }
            else
            {
                SubscriptionType stype = new SubscriptionType(results.ElementAt(0).Type, results.ElementAt(0).SubName,
                    results.ElementAt(0).MaxUsers, results.ElementAt(0).MaxAdverts, results.ElementAt(0).MaxClasses,
                    (double)results.ElementAt(0).MonthlyCost);

                return new CompanyDetails(new Guid(results.ElementAt(0).ID), results.ElementAt(0).Name, results.ElementAt(0).Description, results.ElementAt(0).URL,
                    results.ElementAt(0).ImagePath, results.ElementAt(0).Address1, results.ElementAt(0).Address2, results.ElementAt(0).City, results.ElementAt(0).State,
                    results.ElementAt(0).Postcode, results.ElementAt(0).Country, results.ElementAt(0).FaceBookURL, results.ElementAt(0).PaypalEmail,
                    Convert.ToBoolean(results.ElementAt(0).PaypalAbsorbFees), results.ElementAt(0).Phone, results.ElementAt(0).Email, results.ElementAt(0).Style,
                    results.ElementAt(0).Type, Convert.ToBoolean(results.ElementAt(0).IsClub), stype, Convert.ToDateTime(results.ElementAt(0).NextPaymentDate));
            }
        }

        public CompanyDetails getCompanyDetails(string companyName, string userID)
        {
            List<string> paramList = new List<string>();
            paramList.Add(userID);
            paramList.Add(companyName);

            IEnumerable<dynamic> results = runQuery("SELECT Company.ID, Company.Name,Company.URL, Company.ImagePath, Company.PaypalEmail,Company.Phone, Company.Email, " +
                "Company.Address1, Company.Address2, Company.FaceBookURL, Company.Country, Company.Style, Company.Description, " +
                    "Company.PaypalAbsorbFees, Company.City, Company.State, Company.Postcode, Company.Type,  Subscriptions.NextPaymentDate, " +
                    "CompanyType.Name SubName, CompanyType.MaxUsers, CompanyType.MaxAdverts, CompanyType.MaxClasses, CompanyType.MonthlyCost, " +
                    "Company.IsClub " +
                    "FROM Company INNER JOIN UserProfile ON UserProfile.Company = Company.Name " +
                    "LEFT JOIN CompanyType on Company.Type = CompanyType.ID " +
                    "LEFT JOIN Subscriptions ON (Subscriptions.CompanyID = Company.ID AND Subscriptions.ProfileStatus = 'Active') " +
                "WHERE UserProfile.UserID = @0 AND Company.Name = @1", paramList);

            SubscriptionType stype = new SubscriptionType(results.ElementAt(0).Type, results.ElementAt(0).SubName,
                    results.ElementAt(0).MaxUsers, results.ElementAt(0).MaxAdverts, results.ElementAt(0).MaxClasses,
                    results.ElementAt(0).MonthlyCost);

            return new CompanyDetails(results.ElementAt(0).ID, results.ElementAt(0).Name, results.ElementAt(0).Description, results.ElementAt(0).URL,
                results.ElementAt(0).ImagePath, results.ElementAt(0).Address1, results.ElementAt(0).Address2, results.ElementAt(0).City, results.ElementAt(0).State,
                results.ElementAt(0).Postcode, results.ElementAt(0).Country, results.ElementAt(0).FaceBookURL, results.ElementAt(0).PaypalEmail,
                Convert.ToBoolean(results.ElementAt(0).PaypalAbsorbFees), results.ElementAt(0).Phone, results.ElementAt(0).Email, results.ElementAt(0).Style,
                results.ElementAt(0).Type, results.ElementAt(0).IsClub, stype, results.ElementAt(0).NextPaymentDate);
        }
        
        public bool updateCompany(string userName, string companyName, string companyURL, string companyImage, string companyDescription,
        string paypalEmail, string paypalAbsorbFees, string companyPhone,
        string companyEmail, string companyAddress1, string companyAddress2, string companyCity, string companyState, string companyPostcode, string country, string companyFaceBook)
        {
            List<string> paramList = new List<string>();
            paramList.Add(companyName);

            var company = runQueryValue("SELECT Name FROM Company WHERE Name = @0", paramList);
            //var company = runQueryValue(String.Format("SELECT Name FROM Company INNER JOIN UserProfile ON UserProfile.Company = Company.Name " +
            //    "WHERE UserProfile.UserID = '{0}' AND Company.Name = '{1}'", userID, companyName));

            paramList = new List<string>();
            paramList.Add(companyName);
            paramList.Add(userName);
            insertQuery("UPDATE UserProfile SET Company = @0 WHERE Email = @1", paramList );

            //check if company exists already or not
            if (String.IsNullOrEmpty(company))
            {
                paramList = new List<string>();
                paramList.Add(companyName);
                paramList.Add(companyURL);
                paramList.Add(companyEmail);
                paramList.Add(getCompanyTypeID("basic").ToString());
                paramList.Add(country);

                if (insertQuery("INSERT INTO Company (Name, URL, Email, Style, PaypalAbsorbFees, Type, Country) Values (@0,@1,@2,'Default', 'True', @3, @4)", paramList))
                {
                    var companyID = getCompanyID(companyName);
                    var startDate = DateTime.Now;
                    var expiresDate = startDate.AddMonths(1);

                    paramList = new List<string>();
                    paramList.Add(companyID.ToString());
                    paramList.Add(startDate.ToString("yyyy/MM/dd HH:mm:ss"));
                    paramList.Add(expiresDate.ToString("yyyy/MM/dd HH:mm:ss"));

                    return insertQuery("INSERT INTO Billing (ID, CompanyID, Cost, Paid, Category, Comment, IssueDate, PaidDate, ExpiresDate) VALUES " +
                        "(newid(), @0, '0.00', 'True', 'Monthly Subscription', 'First Month Free', @1, @1, @2)",
                        paramList);
                }
                else
                {
                    return false;
                };
            }
            else
            {
                if (companyImage != "")
                {
                    paramList = new List<string>();
                    paramList.Add(companyImage);
                    paramList.Add(companyName);
                    insertQuery("UPDATE Company SET ImagePath = @0 WHERE Name = @1", paramList);
                }

                paramList = new List<string>();
                paramList.Add(companyURL);
                paramList.Add(paypalEmail);
                paramList.Add(paypalAbsorbFees);
                paramList.Add(companyPhone);
                paramList.Add(companyEmail);
                paramList.Add(companyAddress1);
                paramList.Add(companyAddress2);
                paramList.Add(companyCity);
                paramList.Add(companyState);
                paramList.Add(companyPostcode);
                paramList.Add(companyFaceBook);
                paramList.Add(country);
                paramList.Add(companyDescription);
                paramList.Add(companyName);

                return insertQuery("UPDATE Company SET URL = @0, PaypalEmail = @1, PaypalAbsorbFees = @2, Phone=@3, Email=@4, " +
                    "Address1 = @5, Address2 = @6, City = @7, State = @8, Postcode = @9, FaceBookURL = @10, Country = @11, Description = @12 WHERE Name = @13",
                    paramList);
            }
        }

        public bool updateCompany(CompanyDetails companyDetails)
        {
            List<string> paramList = new List<string>();
            paramList.Add(companyDetails.Name);

            var company = runQueryValue("SELECT Name FROM Company WHERE Name = @0", paramList);
            //var company = runQueryValue(String.Format("SELECT Name FROM Company INNER JOIN UserProfile ON UserProfile.Company = Company.Name " +
            //    "WHERE UserProfile.UserID = '{0}' AND Company.Name = '{1}'", userID, companyName));

            bool result = true;

            //check if company exists already or not
            if (String.IsNullOrEmpty(company))
            {
                companyDetails.ID = Guid.NewGuid();

                paramList = new List<string>();
                paramList.Add(companyDetails.ID.ToString());
                paramList.Add(companyDetails.Name);
                paramList.Add(companyDetails.URL);
                paramList.Add(companyDetails.Email);
                paramList.Add(companyDetails.TypeID.ToString());
                paramList.Add(companyDetails.Country);

                if (insertQuery("INSERT INTO Company (Id, Name, URL, Email, Style, PaypalAbsorbFees, Type, Country, IsClub) Values (@0,@1,@2,@3,'Default', 'True', @4, @5, 'False')", paramList))
                {
                    //var companyID = getCompanyID(companyDetails.Name);
                    var startDate = DateTime.Now;
                    var expiresDate = startDate.AddMonths(1);

                    paramList = new List<string>();
                    paramList.Add(companyDetails.ID.ToString());
                    paramList.Add(startDate.ToString("yyyy/MM/dd HH:mm:ss"));
                    paramList.Add(expiresDate.ToString("yyyy/MM/dd HH:mm:ss"));

                    result = insertQuery("INSERT INTO Billing (Id, CompanyID, Cost, Paid, Category, Comment, IssueDate, PaidDate, ExpiresDate) VALUES " +
                        "(newid(), @0, '0.00', 'True', 'Monthly Subscription', 'First Month Free', @1, @1, @2)",
                        paramList);
                }
                else
                {
                    return false;
                };
            }
            else
            {
                if (companyDetails.ImagePath != "")
                {
                    paramList = new List<string>();
                    paramList.Add(companyDetails.ImagePath);
                    paramList.Add(companyDetails.Name);
                    insertQuery("UPDATE Company SET ImagePath = @0 WHERE Name = @1", paramList);
                }

                paramList = new List<string>();
                paramList.Add(companyDetails.URL);
                paramList.Add(companyDetails.PaypalEmail);
                paramList.Add(companyDetails.PaypalAbsorbFees.ToString());
                paramList.Add(companyDetails.Phone);
                paramList.Add(companyDetails.Email);
                paramList.Add(companyDetails.Address1);
                paramList.Add(companyDetails.Address2);
                paramList.Add(companyDetails.City);
                paramList.Add(companyDetails.State);
                paramList.Add(companyDetails.Postcode);
                paramList.Add(companyDetails.FaceBookURL);
                paramList.Add(companyDetails.Country);
                paramList.Add(companyDetails.Description);
                paramList.Add(companyDetails.Name);

                result = insertQuery("UPDATE Company SET URL = @0, PaypalEmail = @1, PaypalAbsorbFees = @2, Phone=@3, Email=@4, " +
                    "Address1 = @5, Address2 = @6, City = @7, State = @8, Postcode = @9, FaceBookURL = @10, Country = @11, Description = @12 WHERE Name = @13",
                    paramList);
            }

            paramList = new List<string>();
            paramList.Add(companyDetails.ID.ToString());
            paramList.Add(System.Web.HttpContext.Current.User.Identity.GetUserId());
            return insertQuery("UPDATE UserProfile SET Company = @0 WHERE UserId = @1", paramList);
        }

        public Guid getCompanyID(string companyName)
        {
            List<string> paramList = new List<string>();
            paramList.Add(companyName);
            Guid id = Guid.Empty;
            try
            {
                id = new Guid(runQueryValue("SELECT ID FROM Company WHERE Name=@0", paramList));
            }
            catch { id = Guid.Empty; }
            return id;
        }

        public int getCompanyTypeID(string companyTypeName)
        {
            List<string> paramList = new List<string>();
            paramList.Add(companyTypeName);
            var strID = Convert.ToString(runQueryValue("SELECT CompanyType.ID FROM CompanyType WHERE CompanyType.Name = {0}", paramList));
            return strID;
        }

        public string getCompanyType(string company)
        {
            List<string> paramList = new List<string>();
            paramList.Add(company);
            var strID = Convert.ToString(runQueryValue("SELECT CompanyType.Name FROM CompanyType Join Company ON Company.Type = CompanyType.ID WHERE Company.Name = @0", paramList));
            return strID;
        }

        public string updateCompanyType(string companyID, string typeID)
        {
            List<string> paramList = new List<string>();
            paramList.Add(companyID);
            paramList.Add(typeID);
            var strID = Convert.ToString(runQueryValue("UPDATE Company SET Type = @1 WHERE Company.ID = @0", paramList));
            return strID;
        }

        public string getCompanyCountry(string company)
        {
            List<string> paramList = new List<string>();
            paramList.Add(company);
            var strImage = Convert.ToString(runQueryValue("SELECT Countries.Name FROM Company INNER JOIN Countries ON Company.Country = Countries.IsoCode2 WHERE Company.Name = @0", paramList));
            return strImage;
        }

        public string getCompanyCountryCode(string company)
        {
            List<string> paramList = new List<string>();
            paramList.Add(company);
            var strImage = Convert.ToString(runQueryValue("SELECT Country FROM Company WHERE Company.Name = @0", paramList));
            return strImage;
        }

        public string getCompanyCountryISOCode(string company)
        {
            List<string> paramList = new List<string>();
            paramList.Add(company);
            var strImage = Convert.ToString(runQueryValue("SELECT Countries.IsoCode2 FROM Company INNER JOIN Countries ON Company.Country = Countries.IsoCode2 WHERE Company.Name = @0", paramList));
            return strImage;
        }

        public string getCompanyURL(string company)
        {
            List<string> paramList = new List<string>();
            paramList.Add(company);
            var url = runQueryValue("SELECT URL FROM Company WHERE Name = @0", paramList);

            if (!String.IsNullOrEmpty(url))
                return url;
            else
                return "";
        }

        public int getMaximumUsers(string company)
        {
            List<string> paramList = new List<string>();
            paramList.Add(company);
            return (int)runQueryValue("SELECT CompanyType.MaxUsers FROM CompanyType JOIN Company on CompanyType.ID = Company.Type WHERE Company.Name = {0}", paramList);
        }

        public IEnumerable<dynamic> getUsers(CompanyDetails companyDetails)
        {
            List<string> paramList = new List<string>();
            paramList.Add(companyDetails.Name);
            return runQuery("SELECT DISTINCT UserProfile.UserId, UserProfile.Email, UserProfile.Firstname, UserProfile.Lastname, " +
                "UserProfile.Phone, UserProfile.Address1, UserProfile.Address2, UserProfile.City, UserProfile.State, UserProfile.Country, " +
                "UserProfile.Company as OwnerOf, UserProfile.Postcode, AspNetRoles.Name as RoleName FROM " +
                "UserProfile INNER JOIN AspNetUserRoles ON AspNetUserRoles.UserID = UserProfile.UserID " +
                "INNER JOIN AspNetRoles ON AspNetUserRoles.RoleId = AspNetRoles.Id " +
                "WHERE AspNetRoles.Company = @0 and AspNetRoles.Name <> @0 ", paramList);
        }   //rolename company is everyone including clients. company users are basic_*, editor_* and admin_*

        public bool deleteRoles(string userID, string company)
        {
            List<string> paramList = new List<string>();
            paramList.Add(userID.ToString());
            paramList.Add(company);
            IEnumerable<dynamic> roles = runQuery("SELECT AspNetRoles.Id FROM AspNetUserRoles JOIN AspNetRoles ON AspNetUserRoles.RoleID = AspNetRoles.ID " +
                "WHERE AspNetUserRoles.UserId = {0} and AspNetRoles.Company= {1} ", paramList);

            bool success = false;
            foreach (var role in roles)
            {
                paramList = new List<string>();
                paramList.Add(userID.ToString());
                paramList.Add(role.RoleId.ToString());
                success = insertQuery("DELETE FROM AspNetUserRoles " +
                "WHERE UserId = {0} and RoleId = {1} ", paramList);
                if (!success)
                {
                    break;
                }

            }
            return success;
        }

        public string getCompanyFromClass(string classID)
        {
            List<string> paramList = new List<string>();
            paramList.Add(classID);
            return (string)runQueryValue("SELECT Company FROM Class WHERE ID = @0", paramList);
        }

        public List<PersonDetails> getClients(string companyName)
        {

            List<String> paramList = new List<string>();
            paramList.Add(companyName);

            IEnumerable<dynamic> result = runQuery("SELECT DISTINCT UserProfile.UserId as ClientID, UserProfile.Email as userEmail, UserProfile.Firstname, UserProfile.Lastname, UserProfile.Company, " +
                "UserProfile.Phone, UserProfile.Address1, UserProfile.Address2, UserProfile.City, UserProfile.State, UserProfile.Country, UserProfile.Postcode, 'user' as UserType, " +
                "Company.ID as CompanyID, Company.Name as CompanyName FROM " +
                "UserProfile INNER JOIN AspNetUserRoles ON AspNetUserRoles.UserID = UserProfile.UserID " +
                "INNER JOIN AspNetRoles ON AspNetUserRoles.RoleId = AspNetRoles.Id " +
                "LEFT JOIN BookingDetails ON UserProfile.UserId = BookingDetails.UserID " +
                "LEFT JOIN Company ON UserProfile.Company = Company.ID " +
                "LEFT JOIN Class ON BookingDetails.ClassID = Class.ID " +
                "WHERE AspNetRoles.Name = @0 OR Class.Company = @0 " +
                "UNION SELECT Clients.ID as ClientID, Clients.Email as userEmail, Clients.Firstname, Clients.Lastname, Company, Clients.Phone, " +
                "Clients.Address1, Clients.Address2, Clients.City, Clients.State, Clients.Country, Clients.Postcode, 'client' as UserType,  " +
                "Company.ID as CompanyID, Company.Name as CompanyName FROM " +
                "Clients LEFT JOIN Company on Clients.Company = Company.ID WHERE Company = @0 ORDER BY Lastname  ", paramList);

            List<PersonDetails> people = new List<PersonDetails>();
            if (result.Count() != 0) { 
                foreach (var person in result)
                {
                    Functions.PersonType type = Functions.PersonType.Client;
                    if (person.UserType == "user")
                    {
                        type = Functions.PersonType.User;
                    }

                    people.Add(new PersonDetails(person.ClientID.ToString(), person.FirstName, person.LastName, person.CompanyID, person.CompanyName, person.Address1, person.Address2, "",
                        person.City, person.State, person.Postcode, person.Country, person.Phone, person.userEmail, type));
                }
            }
            return people;

        }

        public IEnumerable<dynamic> getClientDetail(string company, PersonDetails client)
        {
            List<string> paramList = new List<string>();
            paramList.Add(company);
            paramList.Add(client.ID);

            if (client.Type == Functions.PersonType.User)
            {
                return runQuery("SELECT DISTINCT UserProfile.UserId as ClientID, UserProfile.Email, UserProfile.Firstname, UserProfile.Lastname, " +
                "UserProfile.Phone, UserProfile.Address1, UserProfile.Address2, UserProfile.City, UserProfile.State, UserProfile.Country, UserProfile.Postcode, 'user' as ClientType FROM " +
                "UserProfile INNER JOIN AspNetUserRoles ON AspNetUserRoles.UserId = UserProfile.UserID " +
                "INNER JOIN AspNetRoles ON AspNetUserRoles.RoleId = AspNetRoles.Id " +
                "LEFT JOIN BookingDetails ON UserProfile.UserId = BookingDetails.UserID " +
                "LEFT JOIN Class ON BookingDetails.ClassID = Class.ID " +
                "WHERE UserProfile.UserId = @1 AND (AspNetRoles.Name = @0 OR Class.Company = @0) ", paramList);
            }
            else
            {
                return runQuery("SELECT ID as ClientID, Email, Firstname, Lastname, Phone, Address1, Address2, City, State, Country, Postcode, 'client' as ClientType FROM " +
                "Clients WHERE Company = @0 AND Clients.ID = @1 ORDER BY Lastname  ", paramList);
            }

        }

        public string getCompanyImage(string company)
        {
            List<string> paramList = new List<string>();
            paramList.Add(company);
            var strImage = Convert.ToString(runQueryValue("SELECT ImagePath FROM Company WHERE Name = @0", paramList));
            return strImage;
        }

        public bool clientPaid(string BookingDetailsID, Boolean paid)
        {
            List<string> paramList = new List<string>();
            paramList.Add(paid.ToString());
            paramList.Add(BookingDetailsID);
            return insertQuery("UPDATE BookingDetails SET paid = @0 WHERE ID = @1", paramList);
        }

        public bool clientCancelled(string BookingDetailsID, Boolean cancelled)
        {
            List<string> paramList = new List<string>();
            paramList.Add(cancelled.ToString());
            paramList.Add(BookingDetailsID);
            bool success = insertQuery("UPDATE BookingDetails SET cancelled = @0 WHERE ID = @1", paramList);
            if (success)
            {
                return insertQuery("UPDATE Booking SET Cancelled = @0 WHERE BookingID = @1", paramList);
            }
            else {
                return false;
            }

        }

        public bool clientSingleCancelled(string BookingID, Boolean cancelled)
        {
            List<string> paramList = new List<string>();
            paramList.Add(cancelled.ToString());
            paramList.Add(BookingID);
            return insertQuery("UPDATE Booking SET cancelled = @0 WHERE ID = @1", paramList);
        }




        #endregion

        #region "Location Details"

        public bool AddLocation(string locationName, string longitude, string latitude, string locationDescription,
        string locationAddress1, string locationAddress2, string locationCity, string locationState, string locationPostcode, string company)
        {
            List<string> paramList = new List<string>();
            paramList.Add(locationName);
            paramList.Add(locationDescription);
            paramList.Add(longitude);
            paramList.Add(latitude);
            paramList.Add(locationAddress1);
            paramList.Add(locationAddress2);
            paramList.Add(locationCity);
            paramList.Add(locationState);
            paramList.Add(locationPostcode);
            paramList.Add(company);

            string strSQL = "INSERT INTO Location (Name, Description, Longitude, Latitude, Address1, Address2, City, State, Postcode, Company) VALUES " +
                "(@0,@1,@2,@3,@4,@5,@6,@7,@8,@9)";


            return insertQuery(strSQL, paramList);
        }

        public bool UpdateLocation(string locationID, string locationName, string longitude, string latitude, string locationDescription,
            string locationAddress1, string locationAddress2, string locationCity, string locationState, string locationPostcode, string company)
        {
            List<string> paramList = new List<string>();
            paramList.Add(locationName);
            paramList.Add(locationDescription);
            paramList.Add(longitude);
            paramList.Add(latitude);
            paramList.Add(locationAddress1);
            paramList.Add(locationAddress2);
            paramList.Add(locationCity);
            paramList.Add(locationState);
            paramList.Add(locationPostcode);
            paramList.Add(locationID);
            paramList.Add(company);

            string strSQL = "UPDATE Location SET Name=@0, Description=@1, Longitude=@2, Latitude=@3, " +
                "Address1=@4, Address2=@5, City=@6, State=@7, Postcode=@8 WHERE ID=@9 AND Company=@10 ";
            return insertQuery(strSQL, paramList);
        }

        public IEnumerable<dynamic> getLocationDetails(string locationID, string company)
        {
            List<string> paramList = new List<string>();
            paramList.Add(locationID);
            paramList.Add(company);
            return runQuery("SELECT * FROM Location WHERE Location.ID = @0 AND Company = @1", paramList);
        }

        public bool checkLocation(string locationID, string company)
        {
            List<string> paramList = new List<string>();
            paramList.Add(locationID);
            paramList.Add(company);

            string strSQL = "SELECT ID FROM Location WHERE ID = @0 AND Company = @1";
            var result = runQueryValue(strSQL, paramList);
            if (result != null)
                return true;
            else
                return false;

        }

        public IEnumerable<dynamic> listCountries(bool reservedList)
        {
            string strSQL = "SELECT * FROM Countries ";
            if (reservedList)
            {
                strSQL += " WHERE Reserved = 'true' ORDER BY Name ASC";
            }
            return runQuery(strSQL, null);

        }

        public IEnumerable<dynamic> listCounties(string CountryCode)
        {
            List<string> paramList = new List<string>();
            paramList.Add(CountryCode);

            string strSQL = "SELECT * FROM Counties WHERE Country = @0 ORDER BY Name ASC ";

            return runQuery(strSQL, paramList);

        }

        public IEnumerable<dynamic> getLocationList(string company)
        {
            List<string> paramList = new List<string>();
            paramList.Add(company);
            return runQuery("SELECT * FROM Location WHERE Company = @0 ORDER BY Name", paramList);
        }

        public IEnumerable<dynamic> getClassLocation(string classID)
        {
            List<string> paramList = new List<string>();
            paramList.Add(classID);
            return runQuery("SELECT Class.Name as ClassName, Location.Latitude, Location.Longitude FROM Location " +
                "INNER JOIN Class on Class.LocationID = Location.ID WHERE Class.ID = @0",
                paramList);
        }

        public string getLocationID(string locationName, string company)
        {
            List<string> paramList = new List<string>();
            paramList.Add(locationName);
            paramList.Add(company);

            string strSQL = "SELECT ID FROM Location WHERE Name = @0 AND Company = @1";
            var result = runQueryValue(strSQL, paramList);
            if (result != null)
                return result.ToString();
            else
                return "";
        }

        public string getLocationName(string locationID, string company)
        {
            List<string> paramList = new List<string>();
            paramList.Add(locationID);
            paramList.Add(company);

            string strSQL = "SELECT Name FROM Location WHERE ID = @0 AND Company = @1";
            var result = runQueryValue(strSQL, paramList);
            if (result != null)
                return result;
            else
                return "";
        }


        #endregion

        #region "Class Details"

        public Guid getClassID(string name, string Date, string company)
        {
            List<String> paramList = new List<string>();
            paramList.Add(name);
            paramList.Add(Date);
            paramList.Add(company);

            return Convert.ToInt32(runQueryValue("Select ID from Class where name = @0 and startdate = @1 and company = @2",
                paramList));

        }

        public Guid getClassID(string name, DateTime Date, string company)
        {
            List<String> paramList = new List<string>();
            paramList.Add(name);
            paramList.Add(Date.ToString("yyyy-MM-dd HH:mm"));
            paramList.Add(company);

            var guid = runQueryValue("Select ID from Class where name = @0 and startdate = @1 and company = @2", paramList);

            if(guid == null)
            {
                return new Guid();
            }
            else
            {
                return new Guid(guid);

            }
            
        }

        public string createClass(GroupClass gClass)
        {
            var result = "";

            //check if class exists
            var classID = getClassID(gClass.Name, gClass.StartDate, gClass.Company);
            result = classID.ToString();

            string[] adminUsers = new string[5];
            adminUsers = gClass.AdminIDList;

            var strSQL = "";

            List<String> paramList = new List<string>();

            if (classID == new Guid())
            {
                classID = gClass.ID;
                paramList = new List<string>();
                paramList.Add(gClass.ID.ToString());
                paramList.Add(gClass.Name);
                paramList.Add(gClass.SubCategoryID);
                paramList.Add(gClass.StartDate.ToString("yyyy-MM-dd HH:mm:00"));
                paramList.Add(gClass.RepeatDays);
                paramList.Add(gClass.Repeated.ToString());
                paramList.Add(gClass.NumberOfLessons.ToString());
                paramList.Add(gClass.RepeatFrequency.ToString());
                paramList.Add(gClass.ClientName);
                paramList.Add(gClass.ClientEmail);
                paramList.Add(gClass.MaxCapacity.ToString());
                paramList.Add(gClass.CostOfCourse.ToString());
                paramList.Add(gClass.CostOfSession.ToString());
                paramList.Add(adminUsers[0]);
                //paramList.Add(gClass.LevelID);
                paramList.Add(gClass.EndDate.ToString("yyyy-MM-dd HH:mm:00"));
                paramList.Add(gClass.Duration.ToString());
                paramList.Add(gClass.LocationID.ToString());
                paramList.Add(gClass.Image);
                paramList.Add(gClass.Description);
                paramList.Add(gClass.AllowDropIn.ToString());
                paramList.Add(gClass.AbsorbFee.ToString());
                paramList.Add(gClass.AllowReservation.ToString());
                paramList.Add(gClass.AutoReservation.ToString());
                paramList.Add(gClass.AllowPayment.ToString());
                paramList.Add(gClass.Company);
                paramList.Add(gClass.IsPrivate.ToString());
                paramList.Add(gClass.IsCourse.ToString());
                paramList.Add(gClass.ClassType);
                paramList.Add(gClass.Slug);

                for (int i = 1; i < 5; i++)
                {
                    if (adminUsers.Count() > i && adminUsers[i] != null)
                    {
                        paramList.Add(adminUsers[i]);
                    }
                    else
                    {
                        paramList.Add(null);
                    }
                }

                //update class table
                strSQL = "INSERT INTO Class (ID, Name, SubCategoryID, StartDate, DaysOfWeek, Repeated, NumberOfLessons, RepeatFrequency, " +
                    " ClientName, ClientEmail, MaxCapacity, CostPerPerson, CostOfSession, " +
                    "AdminID, EndDate, ClassDuration, LocationID, ImageUrl, Description, " +
                    "AllowDropin, AbsorbFee, AllowReservation, AutoReservation, AllowPayment, Company, Private, IsCourse, ClassType, Slug, " +
                    "AdminID2, AdminID3, AdminID4, AdminID5 ) Values " +
                    "(@0,@1,@2,@3,@4,@5,@6,@7,@8,@9,@10,@11,@12,@13,@14,@15,@16,@17,@18,@19," +
                    "@20,@21,@22,@23,@24,@25,@26,@27,@28,@29,@30,@31,@32)";


                if (!insertQuery(strSQL, paramList))
                    return "Error updating database.";
            }
            else
                return "Error: A class has already been set up on that date at the same time and with the same name.";

            //update schedule table
            var date = gClass.StartDate;
            int count = gClass.NumberOfLessons;

            if (gClass.Repeated == Functions.Repeat.never)
            {
                paramList = new List<string>();
                paramList.Add(Guid.NewGuid().ToString());
                paramList.Add(gClass.ID.ToString());
                paramList.Add(date.ToString("yyyy-MM-dd HH:mm:ss"));

                strSQL = "INSERT INTO Schedule (ID, ClassID, Date, Number, Cancelled) VALUES (@0,@1,@2, 1, 'False')";
                if (!insertQuery(strSQL, paramList))
                    return "Error updating database.";
            }
            else if (gClass.Repeated == Functions.Repeat.Day)
            {
                for (int i = 1; i <= gClass.NumberOfLessons; i++)
                {
                    paramList = new List<string>();
                    paramList.Add(Guid.NewGuid().ToString());
                    paramList.Add(gClass.ID.ToString());
                    paramList.Add(date.AddDays((i - 1) * gClass.RepeatFrequency).ToString("yyyy-MM-dd HH:mm:ss"));
                    paramList.Add(i.ToString());

                    strSQL = "INSERT INTO Schedule (ID, ClassID, Date, Number, Cancelled) VALUES (@0,@1, @2,@3, 'False')";
                    if (!insertQuery(strSQL, paramList))
                        return "Error updating database.";
                }
            }
            else if (gClass.Repeated == Functions.Repeat.Week)
            {
                int classCount = 1;
                foreach (DateTime cDate in gClass.ClassDates)
                {
                    paramList = new List<string>();
                    paramList.Add(Guid.NewGuid().ToString());
                    paramList.Add(gClass.ID.ToString());
                    paramList.Add(cDate.ToString("yyyy-MM-dd HH:mm:ss"));
                    paramList.Add(classCount.ToString());
                    classCount++;

                    strSQL = "INSERT INTO Schedule (ID, ClassID, Date, Number, Cancelled) VALUES (@0,@1, @2, @3, 'False')";
                    if (!insertQuery(strSQL, paramList))
                        return "Error updating database.";
                }

            }
            //else if (gClass.Repeated == qt.Repeat.Month)
            //{
            //    for (int i = 1; i <= count; i++)
            //    {
            //        paramList = new List<string>();
            //        paramList.Add(classID.ToString());
            //        paramList.Add(date.AddMonths((i) - 1).ToString("yyyy-MM-dd HH:mm:ss"));
            //        paramList.Add(i.ToString());

            //        strSQL = "INSERT INTO Schedule (ClassID, Date, Number) VALUES (@0,@1, @2)";
            //        if (!insertQuery(strSQL, paramList))
            //            return "Error updating database.";
            //    }
            //}

            //if appointment then book the user in.
            if (gClass.SubCategoryName == "Appointment")
            {
                var guid = Guid.NewGuid().ToString();
                gClass.ID = classID;
                Guid bookingID = InsertBooking(gClass, gClass.ClientID, gClass.ClientEmail, "", gClass.CostOfCourse,
                    0, 0, true, guid, false, true, "Appointment", gClass.ClientType, "");
            }

            return result;
        }
        
        public string updateClass(GroupClass gClass_new, GroupClass gClass_original)
        {
            var result = "";

            //check if class exists
            var classID = getClassID(gClass_original.Name, gClass_original.StartDate, gClass_original.Company);
            var strSQL = "";

            string[] adminUsers = new string[5];
            adminUsers = gClass_new.AdminIDList;

            List<String> paramList = new List<string>();

            if (classID == gClass_original.ID)
            {
                paramList = new List<string>();
                paramList.Add(gClass_new.Name);
                paramList.Add(gClass_new.SubCategoryID);
                paramList.Add(gClass_new.StartDate.ToString("yyyy-MM-dd HH:mm:00"));
                paramList.Add(gClass_new.RepeatDays);
                paramList.Add(gClass_new.Repeated.ToString());
                paramList.Add(gClass_new.NumberOfLessons.ToString());
                paramList.Add(gClass_new.RepeatFrequency.ToString());
                paramList.Add(gClass_new.ClientName);
                paramList.Add(gClass_new.ClientEmail);
                paramList.Add(gClass_new.MaxCapacity.ToString());
                paramList.Add(gClass_new.CostOfCourse.ToString());
                paramList.Add(gClass_new.CostOfSession.ToString());
                paramList.Add(gClass_new.AdminIDList[0]);
                //paramList.Add(gClass_new.LevelID);
                paramList.Add(gClass_new.EndDate.ToString("yyyy-MM-dd HH:mm:00"));
                paramList.Add(gClass_new.Duration.ToString());
                paramList.Add(gClass_new.LocationID.ToString());
                paramList.Add(gClass_new.Image);
                paramList.Add(gClass_new.Description);
                paramList.Add(gClass_new.AllowDropIn.ToString());
                paramList.Add(gClass_new.AbsorbFee.ToString());
                paramList.Add(gClass_new.AllowReservation.ToString());
                paramList.Add(gClass_new.AutoReservation.ToString());
                paramList.Add(gClass_new.AllowPayment.ToString());
                paramList.Add(gClass_new.IsPrivate.ToString());
                paramList.Add(gClass_new.ClassType.ToString());
                paramList.Add(gClass_new.Slug);

                for (int i = 1; i < 5; i++)
                {
                    if (adminUsers.Count() > i && adminUsers[i] != null)
                    {
                        paramList.Add(adminUsers[i]);
                    }
                    else
                    {
                        paramList.Add(null);
                    }
                }
                paramList.Add(gClass_original.ID.ToString());

                //update class table
                strSQL = "UPDATE Class SET Name=@0, SubCategoryID=@1, StartDate=@2, DaysOfWeek=@3, Repeated=@4, NumberOfLessons=@5, " +
                    "RepeatFrequency=@6, ClientName=@7, ClientEmail=@8, " +
                    "MaxCapacity=@9, CostPerPerson=@10, CostOfSession=@11, AdminID=@12, EndDate=@13, " +
                    "ClassDuration=@14, LocationID=@15, " +
                    "ImageUrl=@16, Description=@17, AllowDropIn=@18, AbsorbFee=@19, AllowReservation=@20, " +
                    "AutoReservation=@21, AllowPayment=@22, Private=@23, ClassType=@24, Slug=@25, " +
                    "AdminID2=@26, AdminID3=@27, AdminID4=@28, AdminID5=@29 WHERE ID=@30 ";

                if (!insertQuery(strSQL, paramList))
                    return "Error updating database.";
            }
            else
                return "Error: The class you are trying to update does not exist.";

            if (gClass_new.NumberOfLessons == gClass_original.NumberOfLessons)
            {

                int i = 1;
                foreach (DateTime cDate in gClass_new.ClassDates)
                {
                    paramList = new List<String>();
                    paramList.Add(gClass_original.ID.ToString());
                    paramList.Add(i.ToString());
                    paramList.Add(cDate.ToString("yyyy-MM-dd HH:mm:ss"));

                    strSQL = "UPDATE Schedule SET Date=@2 WHERE ClassID=@0 AND Number=@1";
                    if (!insertQuery(strSQL, paramList))
                        return "Error updating database.";
                    i++;
                }

            }
            else
            {
                //delete any schedules not needed
                if (gClass_original.NumberOfLessons > gClass_new.NumberOfLessons)
                {
                    for (int i = gClass_new.NumberOfLessons + 1; i <= gClass_original.NumberOfLessons; i++)
                    {
                        paramList = new List<string>();
                        paramList.Add(gClass_original.ID.ToString());
                        paramList.Add(gClass_original.ClassDates[i - 1].ToString("yyyy-MM-dd HH:mm:ss"));
                        paramList.Add(i.ToString());

                        strSQL = "DELETE FROM Schedule WHERE ClassID=@0 AND Date=@1 AND Number=@2";
                        if (!insertQuery(strSQL, paramList))
                            return "Error updating database.";
                    }
                }

                //update existing schedules
                int count = 1;
                while (count <= gClass_original.NumberOfLessons
                    && count <= gClass_new.NumberOfLessons)
                {
                    paramList = new List<String>();
                    paramList.Add(gClass_original.ID.ToString());
                    paramList.Add(count.ToString());
                    paramList.Add(gClass_new.ClassDates[count - 1].ToString("yyyy-MM-dd HH:mm:ss"));

                    strSQL = "UPDATE Schedule SET Date=@2 WHERE ClassID=@0 AND Number=@1";
                    if (!insertQuery(strSQL, paramList))
                        return "Error updating database.";
                    count++;
                }

                //insert any new schedules
                while (count <= gClass_new.NumberOfLessons)
                {
                    paramList = new List<String>();
                    paramList.Add(Guid.NewGuid().ToString());
                    paramList.Add(gClass_original.ID.ToString());
                    paramList.Add(gClass_new.ClassDates[count - 1].ToString("yyyy-MM-dd HH:mm:ss"));
                    paramList.Add(count.ToString());

                    strSQL = "INSERT INTO Schedule (ID, ClassID, Date, Number, Cancelled) VALUES (@0,@1, @2,@3, 'False')";
                    if (!insertQuery(strSQL, paramList))
                        return "Error updating database.";
                    count++;
                }
            }


            //update booking details if this is an appointment
            if (gClass_new.SubCategoryName == "Appointment")
            {

            }

            return result;
        }
        
        private int maxCapacity(string strID)
        {
            List<String> paramList = new List<string>();
            paramList.Add(strID);
            var result = runQueryValue("SELECT MaxCapacity FROM Class WHERE ID=@0", paramList);
            return (Convert.ToInt16(result));
        }

        public void checkMaxCapacity(GroupClass gClass, int capacity = 0)
        { 
            if (capacity == 0)
            {
                capacity = maxCapacity(gClass.ID.ToString());
            }

            List<string> paramList = new List<string>();
            paramList.Add(gClass.ID.ToString());
            IEnumerable<dynamic> result = runQuery("SELECT COUNT(Schedule.ID) as idCount, Schedule.Date as SessionDate " +
                "FROM Schedule INNER JOIN Booking ON Booking.ScheduleID = Schedule.ID " +
                "INNER JOIN BookingDetails ON BookingDetails.ID = Booking.BookingID " +
                "WHERE (Schedule.ClassID = @0) AND BookingDetails.cancelled = 'False' " +
                "GROUP BY Schedule.Date ORDER BY Schedule.Date ", paramList);


            gClass.RemainingCapacityList = new List<int>();
            for (int i = 0; i < gClass.ClassDates.Count(); i++)
            {
                gClass.RemainingCapacityList.Add(gClass.MaxCapacity);
            }

            foreach (var item in result)
            {
                var s2 = item.idCount;

                if (item.SessionDate != null)
                {
                    DateTime sd = Convert.ToDateTime(item.SessionDate);
                    int i = gClass.ClassDates.IndexOf(Convert.ToDateTime(item.SessionDate));


                    gClass.RemainingCapacityList[i] -= s2;
                }
                else
                {
                    gClass.RemainingCapacity -= item.idCount;
                }
            }

            for (int i = 0; i < gClass.RemainingCapacityList.Count(); i++)
            {
                if (gClass.RemainingCapacityList[i] < 0)
                {
                    gClass.RemainingCapacityList[i] = 0;
                }
                if (gClass.RemainingCapacityList[i] < gClass.RemainingCapacity)
                {
                    gClass.RemainingCapacity = gClass.RemainingCapacityList[i];
                }
            }

        }

        private IEnumerable<dynamic> getSchedule(string id)
        {
            List<String> paramList = new List<string>();
            paramList.Add(id);
            return runQuery("SELECT ID, Date FROM Schedule WHERE ClassID = @0", paramList);
        }

        private IEnumerable<dynamic> getSchedule(string id, DateTime fromDate)
        {
            List<String> paramList = new List<string>();
            paramList.Add(id);
            paramList.Add(fromDate.ToString("yyyy-MM-dd HH:mm:ss"));
            return runQuery("SELECT ID, Date FROM Schedule WHERE ClassID = @0 AND Date >= @1 ", paramList);
        }

        public List<string> getClassInstructors(GroupClass classID)
        {
            List<string> instructorList = new List<string>();
            foreach (var i in classID.AdminIDList)
            {
                var uDetails = this.getUserDetails(i, "user");
                instructorList.Add(uDetails.FirstName + " " + uDetails.LastName);
            }
            return instructorList;
        }

        public int getClassCount(CompanyDetails companyDetails)
        {
            List<String> paramList = new List<string>();
            paramList.Add(companyDetails.Name);
            paramList.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            var result = runQueryValue("SELECT Count(Class.ID) FROM Class Where Class.Company = @0 AND " +
                "Class.EndDate >= @1 AND Class.Name <> 'Appointment'", paramList);
            if (result == null)
            {
                return 0;
            }
            return result;
        }

        public GroupClass selectClassDetail(string id)
        {
            var userId = "";
            if (HttpContext.Current.User.Identity.IsAuthenticated)
            {
                userId = HttpContext.Current.User.Identity.GetUserId().ToString();
            }


            List<string> paramList = new List<string>();
            paramList.Add(id);
            paramList.Add(userId);


            var tempClass = runQuery("SELECT Class.ID, Class.Name, Class.Description, " +
                "CONVERT(NVARCHAR(50),Category.ID) AS CategoryID, Category.Name AS CategoryName, CONVERT(NVARCHAR(50),SubCategory.ID) AS SubCategoryID, SubCategory.Name AS SubCategoryName, " +
                "CONVERT(NVARCHAR(50),Level.ID) AS LevelID, Level.Name AS LevelName, " +
                "Class.StartDate, Class.EndDate, Class.ClassDuration, Class.NumberOfWeeks, Class.Repeated, Class.NumberOfLessons, " +
                "Class.RepeatFrequency, " +
                "Class.DaysOfWeek, round(Class.CostPerPerson, 2) as CostPerPerson, round(Class.CostOFSession, 2) as CostOfSession, " +
                "Class.ImageURL, Class.MaxCapacity, " +
                "Class.Company, Company.ImagePath as CompanyImage, Class.Private, Class.IsCourse, Class.ClassType, " +
                "Class.AdminID, Class.AdminID2, Class.AdminID3, Class.AdminID4, Class.AdminID5, " +
                "UserProfile.Firstname + ' ' + UserProfile.Lastname AS Admin, " +
                "Company.Phone as ContactPhone, Company.Email as ContactEmail, " +
                "Class.AllowDropIn, Class.AbsorbFee, Class.AllowReservation, Class.AutoReservation, Class.AllowPayment, Class.Private, " +
                "Location.ID as LocationID, Location.Name As LocName, Location.State as LocState, Location.Longitude as LocLng, Location.Latitude as LocLat, " +
                "UserFavourites.UserId as Fav " +
                "FROM Class INNER JOIN SubCategory ON Class.SubCategoryID = SubCategory.ID  " +
                "INNER JOIN Category ON SubCategory.CategoryID = Category.ID " +
                "INNER JOIN UserProfile ON Class.AdminID = UserProfile.userID " +
                "INNER JOIN Location ON Class.LocationID = Location.ID " +
                "INNER JOIN Company ON Class.Company = Company.Name " +
                "LEFT JOIN Level ON Class.LevelID = Level.ID " +
                "LEFT JOIN UserFavourites ON (UserFavourites.ClassId = Class.ID AND UserFavourites.UserId = @1 ) " +
                "WHERE  Class.ID = @0 ", paramList);

            if (tempClass.Count() != 0)
            {
                string adminList = tempClass.ElementAt(0).AdminID.ToString();
                if (tempClass.ElementAt(0).AdminID2 != null)
                { adminList += "," + tempClass.ElementAt(0).AdminID2.ToString(); }
                if (tempClass.ElementAt(0).AdminID3 != null)
                { adminList += "," + tempClass.ElementAt(0).AdminID3.ToString(); }
                if (tempClass.ElementAt(0).AdminID4 != null)
                { adminList += "," + tempClass.ElementAt(0).AdminID4.ToString(); }
                if (tempClass.ElementAt(0).AdminID5 != null)
                { adminList += "," + tempClass.ElementAt(0).AdminID5.ToString(); }

                bool fav = false;
                if(!String.IsNullOrEmpty(tempClass.ElementAt(0).Fav))
                {
                    fav = true;
                }

                GroupClass gClass = new GroupClass(new Guid(tempClass.ElementAt(0).ID.ToString()), tempClass.ElementAt(0).Company, tempClass.ElementAt(0).CompanyImage, tempClass.ElementAt(0).Name,
                    tempClass.ElementAt(0).CategoryID, tempClass.ElementAt(0).CategoryName, tempClass.ElementAt(0).SubCategoryID, tempClass.ElementAt(0).SubCategoryName,
                    //tempClass.ElementAt(0).LevelID, tempClass.ElementAt(0).LevelName,
                    Convert.ToInt16(tempClass.ElementAt(0).ClassDuration), tempClass.ElementAt(0).Repeated,
                    tempClass.ElementAt(0).RepeatFrequency, tempClass.ElementAt(0).DaysOfWeek,
                    Convert.ToInt16(tempClass.ElementAt(0).NumberOfLessons), Convert.ToDateTime(tempClass.ElementAt(0).StartDate), Convert.ToInt16(tempClass.ElementAt(0).MaxCapacity),
                    Convert.ToDouble(tempClass.ElementAt(0).CostPerPerson), Convert.ToDouble(tempClass.ElementAt(0).CostOfSession),
                    adminList.Split(','), tempClass.ElementAt(0).Admin,
                    tempClass.ElementAt(0).LocationID, tempClass.ElementAt(0).LocName, tempClass.ElementAt(0).LocState,
                    tempClass.ElementAt(0).LocLng, tempClass.ElementAt(0).LocLng,
                    tempClass.ElementAt(0).Description, tempClass.ElementAt(0).ImageURL,
                    Convert.ToBoolean(tempClass.ElementAt(0).IsCourse),
                    Convert.ToBoolean(tempClass.ElementAt(0).AllowDropIn), Convert.ToBoolean(tempClass.ElementAt(0).AbsorbFee),
                    Convert.ToBoolean(tempClass.ElementAt(0).AllowReservation), Convert.ToBoolean(tempClass.ElementAt(0).AutoReservation),
                    Convert.ToBoolean(tempClass.ElementAt(0).AllowPayment),
                    Convert.ToBoolean(tempClass.ElementAt(0).Private), tempClass.ElementAt(0).ClassType, fav);

                if(tempClass.ElementAt(0).Admin == null || String.IsNullOrEmpty(tempClass.ElementAt(0).Admin.ToString()))
                { 
                    gClass.ContactName = tempClass.ElementAt(0).Company;
                }
                else
                {
                    gClass.ContactName = tempClass.ElementAt(0).Admin.ToString();
                }
                if (tempClass.ElementAt(0).ContactPhone != null && !String.IsNullOrEmpty(tempClass.ElementAt(0).ContactPhone.ToString()))
                {
                    gClass.Phone = tempClass.ElementAt(0).ContactPhone.ToString();
                }
                gClass.Email = tempClass.ElementAt(0).ContactEmail.ToString();

                return gClass;

                //return new GroupClass(tempClass.ElementAt(0).ID.ToString(), tempClass.ElementAt(0).Company, tempClass.ElementAt(0).Name, tempClass.ElementAt(0).TypeID.ToString(), tempClass.ElementAt(0).TypeName,
                //    tempClass.ElementAt(0).LevelID.ToString(), tempClass.ElementAt(0).LevelName, Convert.ToInt16(tempClass.ElementAt(0).ClassDuration), tempClass.ElementAt(0).Repeated,
                //    Convert.ToInt16(tempClass.ElementAt(0).NumberOfLessons), Convert.ToDateTime(tempClass.ElementAt(0).StartDate), Convert.ToInt16(tempClass.ElementAt(0).MaxCapacity),
                //    Convert.ToDouble(tempClass.ElementAt(0).CostPerPerson), tempClass.ElementAt(0).Admin, tempClass.ElementAt(0).Location,
                //    tempClass.ElementAt(0).Description, Convert.ToBoolean(tempClass.ElementAt(0).Private));
            }
            else
            { return null; }
            //"WHERE (GETDATE() <= Class.EndDate) AND Class.ID = '{0}'", id));
        }

        public GroupClass selectClassDetailFromSlug(string slug)
        {
            List<string> paramList = new List<string>();
            paramList.Add(slug);
            var id = (string)runQueryValue("SELECT Class.ID from Class Where Slug = @0", paramList);

            if (id != null)
            {
                return selectClassDetail(id);
            }
            else
                return null;
        }


        public GroupClass selectClassDetailPublic(string id)
        {
            List<String> paramList = new List<string>();
            paramList.Add(id);
            var tempClass = runQuery("SELECT Class.ID, Class.Name, Class.Description, " +
                "Category.ID AS CategoryID, Category.Name AS CategoryName, SubCategory.ID AS SubCategoryID, SubCategory.Name AS SubCategoryName, " +
                "Level.ID AS LevelID, Level.Name AS LevelName, Class.StartDate, Class.EndDate, Class.ClassDuration, " +
                "Class.NumberOfWeeks, Class.Repeated, Class.NumberOfLessons, Class.RepeatFrequency, " +
                "Class.DaysOfWeek, round(Class.CostPerPerson, 2) as CostPerPerson, round(Class.CostOfSession, 2) as CostOfSession, " +
                "Class.ImageURL, Class.MaxCapacity, Class.IsCourse, Class.ClassType, " +
                "Class.Company, Class.Private, Class.AdminID, Class.AdminID2, " +
                "Class.AdminID3, Class.AdminID4, Class.AdminID5, UserProfile.Firstname + ' ' + UserProfile.Lastname AS Admin, " +
                "Class.AllowDropIn, Class.AbsorbFee, Class.AllowReservation, Class.AutoReservation, Class.AllowPayment, Class.Private, " +
                "Location.ID as LocationID, Location.Name As LocName, Location.State as LocState, Location.Longitude as LocLng, Location.Latitude as LocLat " +
                "FROM Class INNER JOIN SubCategory ON Class.SubCategoryID = SubCategory.ID  " +
                "INNER JOIN Category ON SubCategory.CategoryID = Category.ID " +
                "INNER JOIN UserProfile ON Class.AdminID = UserProfile.userID " +
                "INNER JOIN Location ON Class.LocationID = Location.ID " +
                "INNER JOIN Company ON Class.Company = Company.Name " +
                "LEFT JOIN Level ON Class.LevelID = Level.ID " +
                "WHERE (GETDATE() <= Class.EndDate) AND Class.Private = 'False' AND Class.ID = @0", paramList);

            if (tempClass.Count() != 0)
            {
                string adminList = tempClass.ElementAt(0).AdminID.ToString();
                if (tempClass.ElementAt(0).AdminID2 != null)
                { adminList += "," + tempClass.ElementAt(0).AdminID2.ToString(); }
                if (tempClass.ElementAt(0).AdminID3 != null)
                { adminList += "," + tempClass.ElementAt(0).AdminID3.ToString(); }
                if (tempClass.ElementAt(0).AdminID4 != null)
                { adminList += "," + tempClass.ElementAt(0).AdminID4.ToString(); }
                if (tempClass.ElementAt(0).AdminID5 != null)
                { adminList += "," + tempClass.ElementAt(0).AdminID5.ToString(); }

                return new GroupClass(new Guid(tempClass.ElementAt(0).ID.ToString()), tempClass.ElementAt(0).Company, tempClass.ElementAt(0).CompanyImage, tempClass.ElementAt(0).Name,
                    tempClass.ElementAt(0).CategoryID, tempClass.ElementAt(0).CategoryName, tempClass.ElementAt(0).SubCategoryID, tempClass.ElementAt(0).SubCategoryName,
                //tempClass.ElementAt(0).LevelID, tempClass.ElementAt(0).LevelName, 
                Convert.ToInt16(tempClass.ElementAt(0).ClassDuration),
                tempClass.ElementAt(0).Repeated, Convert.ToInt16(tempClass.ElementAt(0).RepeatFrequency), tempClass.ElementAt(0).DaysOfWeek,
                Convert.ToInt16(tempClass.ElementAt(0).NumberOfLessons), Convert.ToDateTime(tempClass.ElementAt(0).StartDate),
                Convert.ToInt16(tempClass.ElementAt(0).MaxCapacity),
                Convert.ToDouble(tempClass.ElementAt(0).CostPerPerson), Convert.ToDouble(tempClass.ElementAt(0).CostOfSession),
                adminList.Split(','), tempClass.ElementAt(0).Admin,
                tempClass.ElementAt(0).LocationID, tempClass.ElementAt(0).LocName, tempClass.ElementAt(0).LocState,
                tempClass.ElementAt(0).LocLng, tempClass.ElementAt(0).LocLat,
                tempClass.ElementAt(0).Description, tempClass.ElementAt(0).ImageURL,
                    Convert.ToBoolean(tempClass.ElementAt(0).IsCourse),
                Convert.ToBoolean(tempClass.ElementAt(0).AllowDropIn), Convert.ToBoolean(tempClass.ElementAt(0).AbsorbFee),
                Convert.ToBoolean(tempClass.ElementAt(0).AllowReservation), Convert.ToBoolean(tempClass.ElementAt(0).AutoReservation),
                Convert.ToBoolean(tempClass.ElementAt(0).AllowPayment),
                Convert.ToBoolean(tempClass.ElementAt(0).Private), tempClass.ElementAt(0).ClassType);
            }
            else { return null; };
        }

        public bool IsEnrolled(string classID, string userID)
        {
            List<String> paramList = new List<string>();
            paramList.Add(classID);
            paramList.Add(userID);
            var result = runQueryValue("SELECT BookingDetails.ID FROM BookingDetails INNER JOIN UserProfile ON BookingDetails.UserID = UserProfile.UserID WHERE " +
                "BookingDetails.classID = @0 AND BookingDetails.UserType = 'user' AND BookingDetails.cancelled = 'false' AND UserProfile.UserId = @1", paramList);
            if (result == null)
            {
                return false;
            }
            else
            {
                return true;
            }

        }

        public List<PersonDetails> getAttendeeList(string classID)
        {

            List<String> paramList = new List<string>();
            paramList.Add(classID);

            IEnumerable<dynamic> result = runQuery("SELECT DISTINCT UserProfile.Firstname, UserProfile.Lastname, UserProfile.Email AS Email, UserProfile.Phone, UserProfile.UserId as ClientID, 'user' as UserType, " +
                "UserProfile.Address1, UserProfile.Address2, UserProfile.Address3, UserProfile.City, UserProfile.State, UserProfile.Postcode, UserProfile.Country, Company.ID as CompanyID, Company.Name as CompanyName " +
                "FROM BookingDetails INNER JOIN UserProfile ON BookingDetails.UserID = UserProfile.UserID " +
                "LEFT JOIN Class ON BookingDetails.ClassID = Class.ID " +
                "LEFT JOIN Company ON Class.Company = Company.Name " +
                "WHERE BookingDetails.classID = @0 AND BookingDetails.UserType = 'user' and BookingDetails.cancelled = 'False' " +
                "UNION " +
                "SELECT DISTINCT Clients.Firstname, Clients.Lastname, Clients.Email AS Email, Clients.Phone, Clients.ID as ClientID, 'client' as UserType, " +
                "Clients.Address1, Clients.Address2, Clients.Address3, Clients.City, Clients.State, Clients.Postcode, Clients.Country, Company.ID as CompanyID, Company.Name as CompanyName " +
                "FROM BookingDetails INNER JOIN Clients ON BookingDetails.UserID = Clients.ID " +
                "LEFT JOIN Class ON BookingDetails.ClassID = Class.ID " +
                "LEFT JOIN Company ON Class.Company = Company.Name " + 
                "WHERE BookingDetails.classID = @0 AND BookingDetails.UserType = 'client' and BookingDetails.cancelled = 'False' ", paramList);

            List<PersonDetails> people = new List<PersonDetails>();
            foreach (var person in result)
            {
                Functions.PersonType type = Functions.PersonType.Client;
                if (person.UserType == "user")
                {
                    type = Functions.PersonType.User;
                }

                people.Add(new PersonDetails(person.ClientID.ToString(), person.FirstName, person.LastName, person.CompanyID, person.CompanyName, person.Address1, person.Address2, person.Address3,
                    person.City, person.State, person.Postcode, person.Country, person.Phone, person.Email, type));
            }
            return people;

        }

        public IEnumerable<dynamic> getAttendeeList_Detailed(string id, bool includeCancelled, string orderBy = "")
        {
            List<String> paramList = new List<string>();
            paramList.Add(id);
            string strSQL = "SELECT BookingDetails.ID as BookingID, Booking.ScheduleID, Schedule.Date as ScheduleDate, BookingDetails.paid, UserProfile.Firstname, UserProfile.Lastname, " +
                "UserProfile.Email AS Email, UserProfile.Phone, UserProfile.Address1, UserProfile.UserId as UserID, 'User' as UserType, " +
                "Booking.Cancelled " +
                 "FROM BookingDetails INNER JOIN UserProfile ON BookingDetails.UserID = UserProfile.UserID " +
                 "INNER JOIN Booking ON Booking.BookingID = BookingDetails.ID " +
                 "INNER JOIN Schedule ON Schedule.ID = Booking.ScheduleID " +
                 "WHERE BookingDetails.classID = @0 AND BookingDetails.UserType = 'user' " +
                 "UNION " +
                 "SELECT BookingDetails.ID as BookingID, Booking.ScheduleID, Schedule.Date as ScheduleDate, BookingDetails.paid, Clients.Firstname, Clients.Lastname, " +
                 "Clients.Email AS Email, Clients.Phone, Clients.Address1, Clients.ID as UserID, 'Client' as UserType, " +
                 "Booking.Cancelled " +
                 "FROM BookingDetails " +
                 "INNER JOIN Booking ON Booking.BookingID = BookingDetails.ID " +
                 "INNER JOIN Schedule ON Schedule.ID = Booking.ScheduleID " +
                 "INNER JOIN Clients ON BookingDetails.UserID = Clients.ID WHERE BookingDetails.classID = @0 AND BookingDetails.UserType = 'client' ";

            if (!includeCancelled)
            {
                strSQL += " and BookingDetails.cancelled = 'False' ";
            }

            if (orderBy == "client")
            {
                strSQL = "(" + strSQL + ") ORDER BY Email, Firstname, Lastname ";
            }
            else if (orderBy == "bookingDetails")
            {
                strSQL = "(" + strSQL + ") ORDER BY BookingDetails.ID, Email, Firstname, Lastname ";
            }
            else
            {
                strSQL = "(" + strSQL + ") ORDER BY ScheduleDate, Email ";
            }

            return runQuery(strSQL, paramList);
        }

        public int getAttendeeNumbers(string classID)
        {
            List<String> paramList = new List<string>();
            paramList.Add(classID);
            return (int)runQueryValue("SELECT COUNT(ID) as idCount FROM BookingDetails WHERE (ClassID = @0)", paramList);
        }

        public IEnumerable<dynamic> getClassSchedule(string classID)
        {
            List<string> paramList = new List<string>();
            paramList.Add(classID);
            return runQuery("SELECT Class.ID, Class.Name, Class.MaxCapacity, Class.CostPerPerson, Class.StartDate,  " +
              "Class.ClassDuration, Location.Name As Location, Class.Description, Class.ImageURL, Level.Name as LevelName, " +
              "UserProfile.Firstname + ' ' + UserProfile.Lastname as AdminName, " +
                "Category.ID AS CategoryID, Category.Name AS CategoryName, SubCategory.ID AS SubCategoryID, SubCategory.Name AS SubCategoryName, " +
                "Schedule.Date as ScheduleDate, Schedule.Number as ClassNumber FROM Class LEFT JOIN Level ON Class.LevelID = Level.ID  " +
                "INNER JOIN SubCategory ON Class.SubCategoryID = SubCategory.ID " +
                "INNER JOIN Category ON SubCategory.CategoryID = Category.ID " +
                "INNER JOIN Schedule ON Class.ID = Schedule.ClassID INNER JOIN UserProfile " +
              "ON Class.AdminID = UserProfile.UserID INNER JOIN Location ON Class.LocationID = Location.ID WHERE Class.ID = @0   " +
              "ORDER BY ScheduleDate",
              paramList);
        }

        public string deleteClass(string id)
        {
            bool result = true;

            //delete bookings linked to schedule
            foreach (var scheduleID in getSchedule(id))
            {
                List<String> paramList = new List<string>();
                paramList.Add(Convert.ToString(scheduleID.ID));

                if (!insertQuery("DELETE FROM Booking WHERE Booking.ScheduleID =@0", paramList))
                {
                    result = false;
                    break;
                }
            }

            if (result)
            {
                //delete schedules linked to class
                List<String> paramList = new List<string>();
                paramList.Add(id);

                result = insertQuery("DELETE FROM Schedule WHERE ClassID = @0", paramList);
            }

            if (result)
            {
                //delete class
                List<String> paramList = new List<string>();
                paramList.Add(id);
                result = insertQuery("DELETE FROM Class WHERE ID = @0", paramList);
            }

            if (result)
                return "";
            else
                return "fail";
        }

        public List<ClientAttendance> getUnconfirmedRequests(string company)
        {
            List<string> paramList = new List<string>();
            paramList.Add(company);

            IEnumerable<dynamic> results = runQuery("SELECT BookingDetails.guid, BookingDetails.UserID, BookingDetails.ClassID, UserProfile.Email, " +
                "UserProfile.Firstname, UserProfile.Lastname, UserProfile.Phone, Class.Name as ClassName, " +
                "Category.ID AS CategoryID, Category.Name AS CategoryName, SubCategory.ID AS SubCategoryID, SubCategory.Name AS SubCategoryName, " +
                "Level.Name as LevelName, " +
                "Class.Startdate, Location.Name as Location FROM BookingDetails INNER JOIN Class on BookingDetails.ClassID = Class.ID " +
                "INNER JOIN UserProfile ON BookingDetails.UserID = UserProfile.UserID " +
                "INNER JOIN SubCategory ON Class.SubCategoryID = SubCategory.ID " +
                "INNER JOIN Category ON SubCategory.CategoryID = Category.ID " +
                "LEFT JOIN Level ON Class.LevelID = Level.ID " +
                "INNER JOIN Location on Class.LocationID = Location.ID " +
                "WHERE BookingDetails.confirmed = 'false' AND Class.Company = @0 and BookingDetails.cancelled = 'false' ", paramList);

            List<ClientAttendance> unconfirmedRequests = new List<ClientAttendance>();
            foreach (var client in results)
            {
                unconfirmedRequests.Add(new ClientAttendance(client.UserID, client.guid, client.ClassID, client.Firstname, client.Lastname,
                    client.Email, client.Phone, Functions.PersonType.User));
            }
            return unconfirmedRequests;
        }
        
        public int getUnconfirmedRequestCount(string company)
        {
            List<string> paramList = new List<string>();
            paramList.Add(company);
            return (int)runQueryValue("SELECT Count(BookingDetails.guid) FROM BookingDetails " +
                "INNER JOIN Class on BookingDetails.ClassID = Class.ID " +
                "WHERE BookingDetails.confirmed = 'false' AND Class.Company = @0 and BookingDetails.cancelled = 'false' ", paramList);
        }

        public bool checkSlugExists(GroupClass gClass)
        {
            List<string> paramList = new List<string>();
            paramList.Add(gClass.Slug);
            paramList.Add(gClass.Company);
            paramList.Add(gClass.StartDate.ToString("yyyy-MM-dd"));
            paramList.Add(gClass.ID.ToString());
            var i = (int)runQueryValue("SELECT Count(Class.ID) FROM Class " +
                "WHERE Class.Slug = @0 AND Class.Company = @1 AND Convert(Date, StartDate) = @2 AND Class.ID <> @3", paramList);
            if (i > 0)
                return true;
            return false;
        }

        public bool updateClassSlug(GroupClass gClass)
        {
            List<string> paramList = new List<string>();
            paramList.Add(gClass.Slug);
            paramList.Add(gClass.ID.ToString());
            return insertQuery("Update Class set Slug=@0 " +
                "WHERE Class.ID = @1 ", paramList);

        }

        #endregion

        #region "Categoires"

        public IEnumerable<dynamic> getCategoryList()
        {
            return runQuery("SELECT * FROM Category ORDER BY Name", null);
        }

        public IEnumerable<dynamic> getCategoryList(string company)
        {
            List<string> paramList = new List<string>();
            paramList.Add(company);
            return runQuery("SELECT * FROM Category WHERE Company IN ( @0, 'DuckRow') ORDER BY Name", paramList);
        }

        public IEnumerable<dynamic> getSubCategoryList()
        {
            return runQuery("SELECT * FROM SubCategory WHERE Company = 'DuckRow' ORDER BY Name", null);
        }

        public string getCategoryID(string categoryName, string companyName)
        {
            List<string> paramList = new List<string>();
            paramList.Add(categoryName);
            paramList.Add(companyName);
            var result = runQueryValue("SELECT ID FROM Category WHERE Name = @0 and Company = @1 ORDER BY Name", paramList);
            if (result != null)
                return result.ToString();
            else
                return "";
        }

        public string getCategoryName(string categoryID)
        {
            List<string> paramList = new List<string>();
            paramList.Add(categoryID);
            var result = runQueryValue("SELECT Name FROM Category WHERE ID = @0 ", paramList);
            if (result != null)
                return result.ToString();
            else
                return "";
        }

        public IEnumerable<dynamic> getSubCategoryList(string company)
        {
            List<string> paramList = new List<string>();
            paramList.Add(company);
            return runQuery("SELECT * FROM SubCategory WHERE Company IN ( @0, 'DuckRow') ORDER BY Name", paramList);
        }

        public IEnumerable<dynamic> getSubCategoryList(string company, int category)
        {
            List<string> paramList = new List<string>();
            paramList.Add(company);
            paramList.Add(category.ToString());
            return runQuery("SELECT * FROM SubCategory JOIN Category ON (Category.ID = SubCategory.CategoryID) " +
                "WHERE SubCategory.Company IN ( @0, 'DuckRow') AND Category.ID = @1 ORDER BY SubCategory.Name", paramList);
        }

        public string getSubCategoryID(string subCategoryName, string companyName)
        {
            List<string> paramList = new List<string>();
            paramList.Add(subCategoryName);
            paramList.Add(companyName);
            var result = runQueryValue("SELECT ID FROM SubCategory WHERE Name = @0 and Company = @1 ORDER BY Name", paramList);
            if (result != null)
                return result.ToString();
            else
                return "";
        }

        public string getSubCategoryName(string subCategoryID)
        {
            List<string> paramList = new List<string>();
            paramList.Add(subCategoryID);
            var result = runQueryValue("SELECT Name FROM SubCategory WHERE ID = @0 ", paramList);
            if (result != null)
                return result.ToString();
            else
                return "";
        }

        public bool AddSubCategory(string subCategoryName, string categoryID, string company)
        {
            List<string> paramList = new List<string>();
            paramList.Add(subCategoryName);
            paramList.Add(categoryID);
            paramList.Add(company);
            string strSQL = "INSERT INTO SubCategory (Name, CategoryID, Company) VALUES " +
                "(@0,@1,@2)";
            return insertQuery(strSQL, paramList);
        }

        /*Check if a Category or SubCategory has a dedicated page with SEO*/
        public bool checkCategoryPageExists(string category)
        {
            List<string> paramList = new List<string>();
            paramList.Add(category);

            //check against category names
            category = runQueryValue("SELECT Name From Category WHERE Name = @0", paramList);
            if (!String.IsNullOrEmpty(category))
                return true;

            //check against sub-category names
            category = runQueryValue("SELECT Name From SubCategory WHERE Name = @0", paramList);
            if (!String.IsNullOrEmpty(category))
                return true;

            return false;
        }


        #endregion

        #region "Advert Details"

        public void createAdvert(GroupClass advert)
        {
            var result = "";
            var strSQL = "";

            GroupClass a1 =  selectAdvertDetail(advert.ID.ToString());
            if (a1 != null)
            {
                updateAdvert(advert, advert);
            }
            else { 

                List<String> paramList = new List<string>();

                if(advert.ID == new Guid())
                {
                    advert.ID = Guid.NewGuid();
                }

                paramList.Add(advert.ID.ToString());
                paramList.Add(advert.Name);
                paramList.Add(advert.Description);
                paramList.Add(advert.Image);
                paramList.Add(advert.Email);
                paramList.Add(advert.Phone);
                paramList.Add(advert.Address1);
                paramList.Add(advert.Address2);
                paramList.Add(advert.City);
                paramList.Add(advert.State);
                paramList.Add(advert.ContactName);
                paramList.Add(advert.Website);
                paramList.Add(advert.SubCategoryID.ToString());
                paramList.Add(advert.AdvertType);
                paramList.Add(advert.IsActive.ToString());
                paramList.Add(advert.Cost.ToString());
                paramList.Add(advert.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss"));
                paramList.Add(advert.CompanyID.ToString());
                paramList.Add(advert.UserID.ToString());
                paramList.Add(advert.BillingID); //Billing ID

                //update advert table
                strSQL = "INSERT INTO Advert (ID, Name, Description, Image, Email, Phone, Address1, Address2, City, State, " +
                    "ContactName, Website, SubCategoryID, AdvertType, IsActive, Cost, CreatedDate, CompanyID, UserID";
                if (advert.BillingID != "")
                {
                    strSQL += ", BillingID";
                }
                strSQL += ") Values (@0,@1,@2,@3,@4,@5,@6,@7,@8,@9,@10,@11,@12,@13,@14,@15,@16,@17,@18";
                if (advert.BillingID != "")
                {
                    strSQL = ",@19";
                }
                strSQL += ")";

                insertQuery(strSQL, paramList).ToString();
            }

            //if (advert.ID.ToString() == "0")
            //{
            //    return "Error updating database.";
            //}
            //else
            //{
            //    return advert.ID.ToString();
            //}
        }

        public string updateAdvert(GroupClass advert, GroupClass advert_original)
        {
            var result = "";
            var strSQL = "";

            List<String> paramList = new List<string>();
            paramList.Add(advert.Name);
            paramList.Add(advert.Description);
            paramList.Add(advert.Image);
            paramList.Add(advert.Email);
            paramList.Add(advert.Phone);
            paramList.Add(advert.Address1);
            paramList.Add(advert.Address2);
            paramList.Add(advert.City);
            paramList.Add(advert.State);
            paramList.Add(advert.ContactName);
            paramList.Add(advert.Website);
            paramList.Add(advert.SubCategoryID);
            paramList.Add(advert.AdvertType);
            paramList.Add(advert.IsActive.ToString());
            paramList.Add(advert.Cost.ToString());
            paramList.Add(advert.CompanyID.ToString());
            paramList.Add(advert.UserID);
            paramList.Add(advert_original.ID.ToString());

            //update advert table
            strSQL = "UPDATE Advert set  Name=@0, Description=@1, Image=@2, Email=@3, Phone=@4, Address1=@5, Address2=@6, " +
                "City=@7, State=@8, ContactName=@9, Website=@10, SubCategoryID=@11, AdvertType=@12, " +
                "IsActive=@13, Cost=@14, CompanyID=@15, UserID=@16  " +
                "WHERE ID=@17";


            if (!insertQuery(strSQL, paramList))
                return "Error updating database.";
            else
                return "";
        }

        public string insertAdvertPayment(GroupClass advert, string ipn_track_id, string txn_id, string PayerName)
        {
            var result = "";
            var strSQL = "";

            List<String> paramList = new List<string>();
            paramList.Add(advert.Cost.ToString());
            paramList.Add(advert.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss"));
            paramList.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            paramList.Add(ipn_track_id);
            paramList.Add(txn_id);
            paramList.Add(PayerName);
            paramList.Add(advert.UserID);

            //update advert table
            strSQL = "INSERT INTO Billing (ID, CompanyID, Cost, Paid, Category, Comment, IssueDate, PaidDate, Reference, " +
                "PaymentFee, ipn_track_id, txn_id, PayerName, ExpiresDate, UserID) " +
                "Values (newid(),'0',@0, 'True', 'Advert', '', @1, @2, '', 0, @3, @4, @5, '', @6)";
            insertQuery(strSQL, paramList);

            advert.BillingID = getBillingID(txn_id);

            if (advert.BillingID.ToString() == "")
            {
                return "Error updating database.";
            }

            paramList = new List<string>();
            paramList.Add(advert.ID.ToString());
            paramList.Add(advert.BillingID.ToString());

            //update advert table
            strSQL = "UPDATE Advert set BillingID=@1 WHERE ID=@0";

            if (!insertQuery(strSQL, paramList))
                return "Error updating database.";
            else
                return advert.BillingID.ToString();
        }

        public GroupClass selectAdvertDetail(string id)
        {
            List<string> paramList = new List<string>();
            paramList.Add(id);

            var tempAdvert = runQuery("SELECT Advert.ID, Advert.Name, Advert.Description, Advert.Image, Advert.Email, Advert.Phone, " +
                "Advert.Address1, Advert.Address2, Advert.City, Advert.State, Advert.ContactName, Advert.Website, " +
                "Category.ID AS CategoryID, Category.Name AS CategoryName, Advert.SubCategoryID, SubCategory.Name AS SubCategoryName, " +
                "Advert.AdvertType, Advert.IsActive, Advert.Cost, Advert.CreatedDate, Company.Name as CompanyName, " +
                "Advert.BillingID, Advert.UserID " +
                "FROM Advert " +
                "INNER JOIN SubCategory ON Advert.SubCategoryID = SubCategory.ID " +
                "INNER JOIN Category ON SubCategory.CategoryID = Category.ID " +
                "INNER JOIN Company ON Advert.CompanyId = Company.ID " +
                "WHERE  Advert.ID = @0", paramList);

            if (tempAdvert.Count() != 0)
            {
                GroupClass advert = new GroupClass(new Guid(tempAdvert.ElementAt(0).ID.ToString()), tempAdvert.ElementAt(0).Name, tempAdvert.ElementAt(0).Description,
                    tempAdvert.ElementAt(0).Image, tempAdvert.ElementAt(0).Email, tempAdvert.ElementAt(0).Phone,
                    tempAdvert.ElementAt(0).ContactName, tempAdvert.ElementAt(0).Website, tempAdvert.ElementAt(0).CategoryID.ToString(),
                    tempAdvert.ElementAt(0).CategoryName.ToString(), tempAdvert.ElementAt(0).SubCategoryID.ToString(),
                    tempAdvert.ElementAt(0).SubCategoryName.ToString(), tempAdvert.ElementAt(0).AdvertType,
                    Convert.ToDouble(tempAdvert.ElementAt(0).Cost.ToString()),
                    tempAdvert.ElementAt(0).Address1, tempAdvert.ElementAt(0).Address2, tempAdvert.ElementAt(0).City, tempAdvert.ElementAt(0).State,
                    Convert.ToDateTime(tempAdvert.ElementAt(0).CreatedDate.ToString("yyyy-MM-dd HH:mm:ss")), tempAdvert.ElementAt(0).CompanyName,
                    tempAdvert.ElementAt(0).UserID.ToString(), "", true);
                if(String.IsNullOrEmpty(tempAdvert.ElementAt(0).BillingID))
                {
                    advert.BillingID = tempAdvert.ElementAt(0).BillingID;
                }
                return advert;
            }
            else
            { return null; }
        }

        public bool deleteAdvert(string id)
        {
            List<String> paramList = new List<string>();
            paramList.Add(id);
            return insertQuery("DELETE FROM Advert WHERE ID = @0", paramList);
        }

        public bool hasAdvertAccess(string userID, string advertID)
        {
            List<String> paramList = new List<string>();
            paramList.Add(userID);
            paramList.Add(advertID);

            //if the user created the advert
            int found = (int)runQueryValue("SELECT Count(UserId) FROM Advert Where UserID = @0 and ID = @1", paramList);
            if (found == 1)
            {
                return true;
            }

            //or if its a company adver and the user is an admin member of the company
            var company = runQueryValue("Select CompanyName From Advert Where ID = @1", paramList);
            if (!String.IsNullOrEmpty(company))
            {
                if (Authenticate.Admin(company))
                    return true;
            }


            return false;


        }

        public List<GroupClass> getMyAdverts(string userID)
        {
            List<string> paramList = new List<string>();
            paramList.Add(userID);

            var results = runQuery("SELECT Advert.ID, Advert.Name, Advert.Description, Advert.Image, Advert.Email, Advert.Phone, " +
                "Advert.Address1, Advert.Address2, Advert.City, Advert.State, Advert.ContactName, Advert.Website, " +
                "Advert.CategoryID, Category.Name AS CategoryName, Advert.SubCategoryID, SubCategory.Name AS SubCategoryName, " +
                "Advert.AdvertType, Advert.IsActive, Advert.Cost, Advert.CreatedDate, Advert.CompanyName, " +
                "Advert.BillingID, Advert.UserID " +
                "FROM Advert " +
                "INNER JOIN SubCategory ON Advert.SubCategoryID = SubCategory.ID " +
                "INNER JOIN Category ON SubCategory.CategoryID = Category.ID " +
                "WHERE  Advert.UserID = @0", paramList);

            List<GroupClass> adverts = new List<GroupClass>();
            foreach (var result in results)
            {
                adverts.Add(new GroupClass(result.ID.ToString(), result.Name, result.Description,
                    result.Image, result.Email, result.Phone,
                    result.ContactName, result.Website, result.CategoryID.ToString(),
                    result.CategoryName.ToString(), result.SubCategoryID.ToString(),
                    result.SubCategoryName.ToString(), result.AdvertType,
                    Convert.ToDouble(result.Cost.ToString()),
                    result.Address1, result.Address2, result.City, result.State,
                    Convert.ToDateTime(result.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss")), result.CompanyName,
                    result.UserID.ToString(), result.BillingID.ToString(), true)
                );
            }
            return adverts;
        }

        public List<GroupClass> getMyAdverts(CompanyDetails companyDetails)
        {
            List<string> paramList = new List<string>();
            paramList.Add(companyDetails.Name);

            var results = runQuery("SELECT Advert.ID, Advert.Name, Advert.Description, Advert.Image, Advert.Email, Advert.Phone, " +
                "Advert.Address1, Advert.Address2, Advert.City, Advert.State, Advert.ContactName, Advert.Website, " +
                "Advert.CategoryID, Category.Name AS CategoryName, Advert.SubCategoryID, SubCategory.Name AS SubCategoryName, " +
                "Advert.AdvertType, Advert.IsActive, Advert.Cost, Advert.CreatedDate, Advert.CompanyName, " +
                "Advert.BillingID, Advert.UserID " +
                "FROM Advert " +
                "INNER JOIN SubCategory ON Advert.SubCategoryID = SubCategory.ID " +
                "INNER JOIN Category ON SubCategory.CategoryID = Category.ID " +
                "WHERE  Advert.CompanyName = @0", paramList);

            List<GroupClass> adverts = new List<GroupClass>();
            foreach (var result in results)
            {
                adverts.Add(new GroupClass(result.ID.ToString(), result.Name, result.Description,
                    result.Image, result.Email, result.Phone,
                    result.ContactName, result.Website, result.CategoryID.ToString(),
                    result.CategoryName.ToString(), result.SubCategoryID.ToString(),
                    result.SubCategoryName.ToString(), result.AdvertType,
                    Convert.ToDouble(result.Cost.ToString()),
                    result.Address1, result.Address2, result.City, result.State,
                    Convert.ToDateTime(result.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss")), result.CompanyName,
                    result.UserID.ToString(), result.BillingID.ToString(), true)
                );
            }
            return adverts;
        }

        public int getMyAdvertCount(string userID)
        {
            List<string> paramList = new List<string>();
            paramList.Add(userID);

            var results = runQuery("SELECT Advert.ID, Advert.Name, Advert.Description, Advert.Image, Advert.Email, Advert.Phone, " +
                "Advert.Address1, Advert.Address2, Advert.City, Advert.State, Advert.ContactName, Advert.Website, " +
                "Advert.CategoryID, Category.Name AS CategoryName, Advert.SubCategoryID, SubCategory.Name AS SubCategoryName, " +
                "Advert.AdvertType, Advert.IsActive, Advert.Cost, Advert.CreatedDate, Advert.CompanyName, " +
                "Advert.BillingID, Advert.UserID " +
                "FROM Advert " +
                "INNER JOIN SubCategory ON Advert.SubCategoryID = SubCategory.ID " +
                "INNER JOIN Category ON SubCategory.CategoryID = Category.ID " +
                "WHERE  Advert.UserID = @0", paramList);

            return results.Count();

        }

        public int getCompanyAdvertCount(string companyName)
        {
            List<string> paramList = new List<string>();
            paramList.Add(companyName);

            var results = runQuery("SELECT Advert.ID, Advert.Name, Advert.Description, Advert.Image, Advert.Email, Advert.Phone, " +
                "Advert.Address1, Advert.Address2, Advert.City, Advert.State, Advert.ContactName, Advert.Website, " +
                "Category.ID as CategoryID, Category.Name AS CategoryName, Advert.SubCategoryID, SubCategory.Name AS SubCategoryName, " +
                "Advert.AdvertType, Advert.IsActive, Advert.Cost, Advert.CreatedDate, Advert.CompanyID, Company.Name as CompanyName, " +
                "Advert.BillingID, Advert.UserID " +
                "FROM Advert " +
                "INNER JOIN SubCategory ON Advert.SubCategoryID = SubCategory.ID " +
                "INNER JOIN Category ON SubCategory.CategoryID = Category.ID " +
                "INNER JOIN Company ON Advert.CompanyID = Company.ID " +
                "WHERE  Company.Name = @0", paramList);

            return results.Count();

        }
         
        
        #endregion

        #region "Booking Details"

        public Guid InsertBooking(GroupClass gClass, string userID, string payerEmail, string txn_id, double amount, double qtFee,
        double paypalFee, bool confirmed, string guid, bool paid, bool claimed, string claimedBy, string userType, string comments,
        string sessionDate = "1950-01-01 00:00:00")
        {

            var strSQL = "";
            var bookingDetailID = Guid.NewGuid();
            DateTime sDate = Convert.ToDateTime(sessionDate);

            List<string> paramList = new List<string>();
            paramList.Add(bookingDetailID.ToString());
            paramList.Add(userID.ToString());
            paramList.Add(gClass.ID.ToString());
            paramList.Add(payerEmail);
            paramList.Add(""); //payerName
            paramList.Add(txn_id);
            paramList.Add(confirmed.ToString());
            paramList.Add(guid);
            paramList.Add(paid.ToString());
            paramList.Add(userType);
            paramList.Add(amount.ToString());
            paramList.Add(qtFee.ToString());
            paramList.Add(paypalFee.ToString());
            paramList.Add(claimed.ToString());
            paramList.Add(claimedBy);
            if (claimed)
            {
                paramList.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            else
            {
                paramList.Add(null);
            }
            paramList.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            paramList.Add("False");
            paramList.Add(comments);
            paramList.Add(sDate.ToString("yyyy-MM-dd HH:mm:ss"));

            strSQL = "INSERT INTO BookingDetails (ID, UserID, ClassID, PayerEmail, PayerName, txn_id, confirmed, guid, paid, UserType, " +
                "Amount, QTFee, PaypalFee, Claimed, ClaimedBy, ClaimedDate, BookedDate, cancelled, Comments, SessionDate ) " +
                "VALUES (@0,@1,@2,@3,@4,@5,@6,@7,@8,@9,@10,@11,@12,@13,@14,@15,@16, @17, @18, @19)";

            insertQuery(strSQL, paramList);

            //if (bookingDetailID != 0)
            //{
                //get schedule
                var schedules = getSchedule(gClass.ID.ToString(), DateTime.Now);

                //create booking table entry for each date
                foreach (var schedule in schedules)
                {
                    DateTime dt = schedule.Date;
                    if (sDate.Year == 1950 || sDate == schedule.Date)
                    {

                        paramList = new List<string>();
                        paramList.Add(schedule.ID.ToString());
                        paramList.Add(userID.ToString());
                        paramList.Add(bookingDetailID.ToString());
                        insertQuery("INSERT INTO booking (ScheduleID, UserID, BookingID, Cancelled) values (@0,@1,@2, 'False')", paramList);
                    }
                }

                //enter payment received.
                if (paid)
                {
                    paramList = new List<string>();
                    paramList.Add(userID.ToString());
                    paramList.Add(gClass.ID.ToString());
                    paramList.Add(amount.ToString());
                    paramList.Add(qtFee.ToString());
                    paramList.Add(paypalFee.ToString());
                    paramList.Add(claimed.ToString());
                    paramList.Add(bookingDetailID.ToString());
                    insertQuery("INSERT INTO Payments (UserID, ClassID, Amount, QTFee, PayPalFee, Claimed, BookingID) values (@0,@1,@2,@3,@4,@5, @6)", paramList);
                }
            //}

            return bookingDetailID;
        }


        public int getBookingID(string bookingDetailsID, DateTime scheduledDate)
        {
            List<String> paramList = new List<string>();
            paramList.Add(bookingDetailsID);
            paramList.Add(scheduledDate.ToString("yyyy-MM-dd HH:mm:ss"));
            var result = runQueryValue("SELECT Booking.ID FROM Booking LEFT JOIN Schedule ON Booking.ScheduleID = Schedule.ID " +
                "WHERE Booking.BookingID=@0 AND Schedule.Date=@1", paramList);
            return (Convert.ToInt16(result));
        }

        public IEnumerable<dynamic> getBookingDetails(string guid)
        {
            List<string> paramList = new List<string>();
            paramList.Add(guid);
            return runQuery("SELECT * FROM BookingDetails WHERE guid = @0", paramList);
        }

        public bool confirmBookingID(string guid)
        {
            List<string> paramList = new List<string>();
            paramList.Add(guid);
            return insertQuery("UPDATE BookingDetails SET confirmed = 'true' WHERE guid = @0", paramList);
        }

        public bool cancelBookingID(string guid)
        {
            List<string> paramList = new List<string>();
            paramList.Add(guid);
            return insertQuery("UPDATE BookingDetails SET confirmed = 'true', cancelled= 'true'  WHERE guid = @0", paramList);
        }



        #endregion

        #region "Billing Details"

        public string getBillingID(string txn_ID)
        {
            List<string> paramList = new List<string>();
            paramList.Add(txn_ID);
            Guid gg = (Guid)runQueryValue("SELECT ID FROM Billing WHERE (txn_id = @0)", paramList);
            return gg.ToString();
        }

        public Guid newSubscription(Subscription sub)
        {
            Guid ID = Guid.NewGuid();

            List<String> paramList = new List<string>();
            paramList.Add(ID.ToString());
            paramList.Add(sub.CompanyDetails.ID.ToString());
            paramList.Add(sub.ProfileID);
            paramList.Add(sub.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss"));
            paramList.Add(sub.NextPaymentDate.ToString("yyyy-MM-dd HH:mm:ss"));
            paramList.Add(sub.Frequency);
            paramList.Add(sub.Period);
            paramList.Add(sub.Amount.ToString());
            paramList.Add(sub.ProfileStatus);
            paramList.Add(sub.PayerID);
            paramList.Add(sub.LastPaymentDate.ToString("yyyy-MM-dd HH:mm:ss"));

            insertQuery("INSERT INTO Subscriptions (ID, CompanyID, ProfileID, CreatedDate, NextPaymentDate, Frequency, Period, Amount, " +
                "ProfileStatus, PayerID, LastPaymentDate ) VALUES (@0, @1,@2,@3,@4,@5,@6,@7,@8,@9,@10)", paramList);

            return ID;
        }

        public Boolean updateSubscription(Subscription sub)
        {
            List<String> paramList = new List<string>();
            paramList.Add(sub.ID.ToString());
            paramList.Add(sub.NextPaymentDate.ToString("yyyy-MM-dd HH:mm:ss"));
            paramList.Add(sub.LastPaymentDate.ToString("yyyy-MM-dd HH:mm:ss"));
            paramList.Add(sub.ProfileStatus);
            return insertQuery("UPDATE Subscriptions SET NextPaymentDate = @1, LastPaymentDate = @2, ProfileStatus = @3 WHERE ID = @0", paramList);
        }

        public IEnumerable<dynamic> getRecurringPaymentDetails(Guid companyID)
        {
            List<String> paramList = new List<string>();
            paramList.Add(companyID.ToString());

            return runQuery("SELECT * FROM Subscriptions WHERE CompanyID = @0 and ProfileStatus = 'Active' ", paramList);
        }

        public IEnumerable<dynamic> getRecurringPaymentDetailsFromProfileID(string profileID)
        {
            List<String> paramList = new List<string>();
            paramList.Add(profileID);

            return runQuery("SELECT Subscriptions.ID, Subscriptions.CompanyID, Subscriptions.ProfileID, Subscriptions.CreatedDate, " +
                "Subscriptions.NextPaymentDate, Subscriptions.Frequency, Subscriptions.Period, Subscriptions.Amount, " +
                "Subscriptions.ProfileStatus, Subscriptions.PayerID, Subscriptions.LastPaymentDate, Company.Name as CompanyName " +
                "FROM Subscriptions JOIN Company on Subscriptions.CompanyID = Company.ID WHERE ProfileID = @0  ", paramList);
        }

        public bool cancelSubscription(Subscription sub)
        {
            List<String> paramList = new List<string>();
            paramList.Add(sub.ID.ToString());
            paramList.Add(sub.ProfileStatus);
            return insertQuery("UPDATE Subscriptions SET ProfileStatus = @1 WHERE ID = @0", paramList);
        }

        public bool InsertPayment(Guid companyID, DateTime startingDate, DateTime expiresDate, string cost, string category, string comment, string reference, string fee, string ipn_track_id, string txn_id, string payerName, int userID)
        {
            List<string> paramList = new List<string>();
            paramList.Add(companyID.ToString());
            paramList.Add(cost);
            paramList.Add(category);
            paramList.Add(comment);
            paramList.Add(startingDate.ToString("yyyy/MM/dd HH:mm:ss"));
            paramList.Add(reference);
            paramList.Add(fee);
            paramList.Add(ipn_track_id);
            paramList.Add(txn_id);
            paramList.Add(payerName);
            paramList.Add(expiresDate.ToString("yyyy/MM/dd HH:mm:ss"));
            paramList.Add(userID.ToString());

            return insertQuery("INSERT INTO Billing (ID, CompanyID, Cost, Paid, Category, Comment, IssueDate, PaidDate, Reference, PaymentFee, ipn_track_id, txn_id, PayerName, ExpiresDate, UserID ) " +
                "VALUES (newid(), @0, @1, 'True', @2, @3, @4, getdate(), @5, @6, @7, @8, @9, @10,@11) ",
                paramList);
        }

        #endregion

        #region "Search Details"

        public List<GroupClass> selectCurrentClasses(string company)
        {
            List<String> paramList = new List<string>();
            paramList.Add(company);

            IEnumerable<dynamic> result = runQuery("SELECT Class.ID, Class.Name, Class.Description, " +
                "Category.ID AS CategoryID, Category.Name AS CategoryName, SubCategory.ID AS SubCategoryID, SubCategory.Name AS SubCategoryName, " +
                "Class.LevelID AS LevelName, Class.StartDate, Class.ClassDuration, Class.NumberOfWeeks, Class.Repeated, Class.NumberOfLessons, " +
                "Class.DaysOfWeek, Class.CostPerPerson, Class.MaxCapacity, Class.Description, Class.ImageURL, Class.Company, UserProfile.Firstname + ' ' + UserProfile.Lastname AS Admin, " +
                "Location.Name As LocName, Location.State as LocState, Location.Longitude as LocLng, Location.Latitude as LocLat " +
                "Class.AllowDropIn, Class.AbsorbFee, Class.AllowReservation, Class.AutoReservation, Class.AllowPayment, Class.IsCourse, Class.Private, Class.ClassType " +
                "FROM Class INNER JOIN SubCategory ON Class.SubCategoryID = SubCategory.ID  " +
                "INNER JOIN Category ON SubCategory.CategoryID = Category.ID " +
                "INNER JOIN UserProfile ON Class.AdminID = UserProfile.userID " +
                "INNER JOIN Location ON Class.LocationID = Location.ID " +
                "WHERE (GETDATE() <= Class.EndDate) AND Class.Company = @0", paramList);

            List<GroupClass> classes = new List<GroupClass>();
            foreach (var item in result)
            {
                classes.Add(new GroupClass(new Guid(item.ID), company, item.CompanyImage, item.Name,
                    item.CategoryID.ToString(), item.CategoryName, item.SubCategoryID.ToString(), item.SubCategoryName,
                    //item.LevelID.ToString(), item.LevelName,
                    Convert.ToInt16(item.ClassDuration), item.Repeated,
                    Convert.ToInt16(item.RepeatFrequency), item.DaysOfWeek,
                    Convert.ToInt16(item.NumberOfLessons), Convert.ToDateTime(item.StartDate),
                    Convert.ToInt16(item.MaxCapacity),
                    Convert.ToDouble(item.CostPerPerson), Convert.ToDouble(item.CostOfSession), item.AdminID.ToString(), item.Admin,
                    item.LocationID, item.LocName, item.LocState, item.LocLng, item.LocLat, item.Description, item.ImageURL,
                    Convert.ToBoolean(item.IsCourse),
                    Convert.ToBoolean(item.AllowDropIn), Convert.ToBoolean(item.AbsorbFee),
                    Convert.ToBoolean(item.AllowReservation), Convert.ToBoolean(item.AutoReservation),
                    Convert.ToBoolean(item.AllowPayment), Convert.ToBoolean(item.Private), ""));
            }
            return classes;
        }

        public List<GroupClass> selectCurrentPublicClasses(string company, List<string> subCategoryList = null, List<string> instructorList = null)
        {
            List<String> paramList = new List<string>();
            paramList.Add(company);

            string strSQL = "SELECT Class.ID, Class.Name, Class.Description, " +
                "Category.ID AS CategoryID, Category.Name AS CategoryName, SubCategory.ID AS SubCategoryID, SubCategory.Name AS SubCategoryName, " +
                "Level.ID As LevelID, Level.Name AS LevelName, Class.StartDate, Class.EndDate, Class.ClassDuration, " +
                "Class.NumberOfWeeks, Class.Repeated, Class.RepeatFrequency, Class.NumberOfLessons, Class.DaysOfWeek, " +
                "Class.CostPerPerson, Class.CostOfSession," +
                "Class.MaxCapacity, Class.Description, Class.ImageURL, Class.Company, Class.AdminID, Class.AdminID2, " +
                "Class.AdminID3, Class.AdminID4, Class.AdminID5, Class.IsCourse, " +
                "Class.AllowDropIn, Class.AbsorbFee, Class.AllowReservation, Class.AutoReservation, Class.AllowPayment, Class.Private, Class.ClassType, " +
                "UserProfile.Firstname + ' ' + UserProfile.Lastname AS Admin, " +
                "Location.ID as LocationID, Location.Name As LocName, Location.State as LocState, Location.Longitude as LocLng, Location.Latitude as LocLat, " +
                "Company.ImagePath as CompanyImage " +
                "FROM Class INNER JOIN SubCategory ON Class.SubCategoryID = SubCategory.ID  " +
                "INNER JOIN Category ON SubCategory.CategoryID = Category.ID " +
                "INNER JOIN UserProfile ON Class.AdminID = UserProfile.userID " +
                "INNER JOIN Location ON Class.LocationID = Location.ID " +
                "INNER JOIN Company ON Class.Company = Company.Name " +
                "LEFT JOIN Level on Class.LevelID = Level.ID " +
                "WHERE (GETDATE() <= Class.EndDate) AND Class.Private = 'False' AND Class.Company = @0 ";

            int i = 1;

            if (subCategoryList.Count != 0)
            {
                strSQL += " AND SubCategory.Name IN (";
                foreach (string s in subCategoryList)
                {
                    paramList.Add(s);
                    strSQL += " @" + i.ToString();
                    i++;
                    if (s != subCategoryList.Last())
                    {
                        strSQL += ", ";
                    }
                }
                strSQL += ") ";
            }
            if (instructorList.Count != 0)
            {
                strSQL += " AND Class.AdminID IN (";
                foreach (string s in instructorList)
                {
                    paramList.Add(s);
                    strSQL += " @" + i.ToString();
                    i++;
                    if (s != instructorList.Last())
                    {
                        strSQL += ", ";
                    }
                }
                strSQL += ") ";
            }

            strSQL += " ORDER BY Class.StartDate";

            IEnumerable<dynamic> result = runQuery(strSQL, paramList);

            List<GroupClass> classes = new List<GroupClass>();
            foreach (var item in result)
            {
                string adminList = item.AdminID.ToString();
                if (item.AdminID2 != null)
                { adminList += "," + item.AdminID2.ToString(); }
                if (item.AdminID3 != null)
                { adminList += "," + item.AdminID3.ToString(); }
                if (item.AdminID4 != null)
                { adminList += "," + item.AdminID4.ToString(); }
                if (item.AdminID5 != null)
                { adminList += "," + item.AdminID5.ToString(); }

                classes.Add(new GroupClass(new Guid(item.ID), company, item.CompanyImage, item.Name,
                    item.CategoryID.ToString(), item.CategoryName, item.SubCategoryID.ToString(), item.SubCategoryName,
                    //item.LevelID.ToString(), item.LevelName, 
                    Convert.ToInt16(item.ClassDuration), item.Repeated,
                    Convert.ToInt16(item.RepeatFrequency), item.DaysOfWeek,
                    Convert.ToInt16(item.NumberOfLessons), Convert.ToDateTime(item.StartDate),
                    Convert.ToInt16(item.MaxCapacity),
                    Convert.ToDouble(item.CostPerPerson), Convert.ToDouble(item.CostOfSession), adminList.Split(','), item.Admin,
                    item.LocationID, item.LocName, item.LocState, item.LocLng, item.LocLat, item.Description, item.ImageURL,
                    Convert.ToBoolean(item.IsCourse),
                    Convert.ToBoolean(item.AllowDropIn), Convert.ToBoolean(item.AbsorbFee),
                    Convert.ToBoolean(item.AllowReservation), Convert.ToBoolean(item.AutoReservation), Convert.ToBoolean(item.AllowPayment),
                    Convert.ToBoolean(item.Private), ""));
            }
            return classes;
        }

        public List<GroupClass> selectCurrentPublicClasses(string subTypeName, string company)
        {
            List<String> paramList = new List<string>();
            paramList.Add(subTypeName);
            paramList.Add(company);

            IEnumerable<dynamic> result = runQuery("SELECT Class.ID, Class.Name, Class.Description, " +
                "Category.ID AS CategoryID, Category.Name AS CategoryName, SubCategory.ID AS SubCategoryID, SubCategory.Name AS SubCategoryName, " +
                "Level.ID As LevelID, Level.Name AS LevelName, Class.StartDate, Class.EndDate, Class.ClassDuration, " +
                "Class.NumberOfWeeks, Class.Repeated, Class.NumberOfLessons, Class.DaysOfWeek, Class.RepeatFrequency, " +
                "Class.CostPerPerson, Class.CostOfSession, " +
                "Class.MaxCapacity, Class.Description, Class.ImageURL, Class.Company, Class.AdminID, Class.AdminID2, " +
                "Class.AdminID3, Class.AdminID4, Class.AdminID5, Class.IsCourse, " +
                "Class.AllowDropIn, Class.AbsorbFee, Class.AllowReservation, Class.AutoReservation, Class.AllowPayment, Class.Private, Class.ClassType, " +
                "UserProfile.Firstname + ' ' + UserProfile.Lastname AS Admin, " +
                "Location.ID as LocationId, Location.Name As LocName, Location.State as LocState, Location.Longitude as LocLng, Location.Latitude as LocLat, " +
                "Company.ImagePath as CompanyImage " +
                "FROM Class INNER JOIN SubCategory ON Class.SubCategoryID = SubCategory.ID  " +
                "INNER JOIN Category ON SubCategory.CategoryID = Category.ID " +
                "INNER JOIN UserProfile ON Class.AdminID = UserProfile.userID " +
                "INNER JOIN Location ON Class.LocationID = Location.ID " +
                "INNER JOIN Company ON Class.Company = Company.Name " +
                "LEFT JOIN Level on Class.LevelID = Level.ID " +
                "WHERE (GETDATE() <= Class.EndDate) AND Class.Private = 'False' AND SubCategory.Name=@0 and Class.Company = @1", paramList);

            List<GroupClass> classes = new List<GroupClass>();
            foreach (var item in result)
            {
                string adminList = item.AdminID.ToString();
                if (item.AdminID2 != null)
                { adminList += "," + item.AdminID2.ToString(); }
                if (item.AdminID3 != null)
                { adminList += "," + item.AdminID3.ToString(); }
                if (item.AdminID4 != null)
                { adminList += "," + item.AdminID4.ToString(); }
                if (item.AdminID5 != null)
                { adminList += "," + item.AdminID5.ToString(); }

                classes.Add(new GroupClass(new Guid(item.ID), company, item.CompanyImage, item.Name,
                    item.CategoryID.ToString(), item.CategoryName, item.SubCategoryID.ToString(), item.SubCategoryName,
                    //item.LevelID.ToString(), item.LevelName, 
                    Convert.ToInt16(item.ClassDuration), item.Repeated,
                    Convert.ToInt16(item.RepeatFrequency), item.DaysOfWeek,
                    Convert.ToInt16(item.NumberOfLessons), Convert.ToDateTime(item.StartDate),
                    Convert.ToInt16(item.MaxCapacity),
                    Convert.ToDouble(item.CostPerPerson),
                    Convert.ToDouble(item.CostOfSession),
                    adminList.Split(','), item.Admin, item.LocationID, item.LocName, item.LocState, item.LocLng, item.LocLat,
                    item.Description, item.ImageURL,
                    Convert.ToBoolean(item.IsCourse),
                    Convert.ToBoolean(item.AllowDropIn), Convert.ToBoolean(item.AbsorbFee),
                    Convert.ToBoolean(item.AllowReservation), Convert.ToBoolean(item.AutoReservation), Convert.ToBoolean(item.AllowPayment),
                    Convert.ToBoolean(item.Private), ""));

            }
            return classes;
        }

        public List<GroupClass> searchAllPublicClasses(string location, string searchText, string company = null)
        {
            var userId = "";
            if (HttpContext.Current.User.Identity.IsAuthenticated)
            {
                userId = HttpContext.Current.User.Identity.GetUserId();
            };

            List<String> paramList = new List<string>();
            paramList.Add(location);
            paramList.Add(company);
            paramList.Add(userId);

            if (searchText != "")
            {
                foreach (string word in searchText.Trim().Split(' '))
                {
                    paramList.Add("%" + word + "%");
                }
            }

            string strQuery = "SELECT Class.ID, Class.Name, Class.Description, " +
                "Category.ID AS CategoryID, Category.Name AS CategoryName, SubCategory.ID AS SubCategoryID, SubCategory.Name AS SubCategoryName, " +
                "Level.ID As LevelID, Level.Name AS LevelName, Class.StartDate, Class.EndDate, Class.ClassDuration, " +
                "Class.NumberOfWeeks, Class.Repeated, Class.RepeatFrequency, Class.NumberOfLessons, Class.DaysOfWeek, " +
                "Class.CostPerPerson, Class.CostOfSession," +
                "Class.MaxCapacity, Class.Description, Class.ImageURL, Class.Company, Class.AdminID, Class.AdminID2, " +
                "Class.AdminID3, Class.AdminID4, Class.AdminID5, Class.IsCourse, " +
                "Class.AllowDropIn, Class.AbsorbFee, Class.AllowReservation, Class.AutoReservation, Class.AllowPayment, Class.Private, Class.ClassType, " +
                "UserProfile.Firstname + ' ' + UserProfile.Lastname AS Admin, " +
                "Location.ID as LocationId, Location.Name As LocName, Location.State as LocState, Location.Longitude as LocLng, Location.Latitude as LocLat, " +
                "Company.ImagePath as CompanyImage, " +
                "UserFavourites.UserId as Fav " + 
                "FROM Class INNER JOIN SubCategory ON Class.SubCategoryID = SubCategory.ID " +
                "INNER JOIN Category ON Category.ID = SubCategory.CategoryID  " +
                "INNER JOIN UserProfile ON Class.AdminID = UserProfile.userID " +
                "INNER JOIN Location ON Class.LocationID = Location.ID " +
                "INNER JOIN Company ON Class.Company = Company.Name " +
                "LEFT JOIN Level on Class.LevelID = Level.ID " +
                "LEFT JOIN UserFavourites on (Class.ID = UserFavourites.ClassId AND UserFavourites.UserId in ('', @2) )" + 
                "WHERE (Class.Private = 'False') AND ";
            //if(String.IsNullOrEmpty(company))
            //{
            //    strQuery += "(( (Class.IsCourse = 'false' OR Class.AllowDropIn = 'true') AND GETDATE() <= Class.EndDate) OR GETDATE() <= Class.StartDate) ";
            //}
            //else
            //{
            //    strQuery += "(( (Class.IsCourse = 'false' OR Class.AllowDropIn = 'true') AND GETDATE() <= Class.EndDate) OR DATEADD(week, -4, GETDATE()) <= Class.StartDate) ";
            //}
            strQuery += "( GETDATE() <= Class.EndDate ) ";



            if (!string.IsNullOrEmpty(location))
            {
                strQuery += " AND Location.State = @0 ";
            }

            if (!string.IsNullOrEmpty(company))
            {
                strQuery += " AND Company.Name = @1 ";
            }

            for (int i = 3; i < paramList.Count(); i++)
            {
                if (i == 3)
                {
                    strQuery += " AND (";
                }
                else
                {
                    strQuery += " AND ";
                }
                strQuery += " (Class.Name LIKE @" + i + " OR Class.Description LIKE @" + i + " " +
                  " OR Category.Name LIKE @" + i + " OR SubCategory.Name LIKE @" + i +
                  " OR Location.Name LIKE @" + i + " " + " OR Location.State LIKE @" + i + " " +
                  " OR UserProfile.Firstname LIKE @" + i + " OR UserProfile.Lastname LIKE @" + i +
                  " OR Class.Company LIKE @" + i + " ) ";

                // full-text indexing not available on shared host.
            }

            if (paramList.Count() > 3)
            {
                strQuery += " ) ";
            }

            strQuery += " ORDER BY Class.StartDate ";

            IEnumerable<dynamic> result = runQuery(strQuery, paramList);

            List<GroupClass> classes = new List<GroupClass>();
            foreach (var item in result)
            {
                var i = item.ID;
                string adminList = item.AdminID.ToString();
                if (item.AdminID2 != null)
                { adminList += "," + item.AdminID2.ToString(); }
                if (item.AdminID3 != null)
                { adminList += "," + item.AdminID3.ToString(); }
                if (item.AdminID4 != null)
                { adminList += "," + item.AdminID4.ToString(); }
                if (item.AdminID5 != null)
                { adminList += "," + item.AdminID5.ToString(); }

                bool fav = false;
                if (!String.IsNullOrEmpty(item.Fav)){
                    fav = true;
                }

                classes.Add(new GroupClass(new Guid(item.ID), item.Company, item.CompanyImage, item.Name,
                    item.CategoryID.ToString(), item.CategoryName, item.SubCategoryID.ToString(), item.SubCategoryName,
                    //item.LevelID.ToString(), item.LevelName, 
                    Convert.ToInt16(item.ClassDuration), item.Repeated.ToString(),
                    Convert.ToInt16(item.RepeatFrequency), item.DaysOfWeek,
                    Convert.ToInt16(item.NumberOfLessons),
                    Convert.ToDateTime(item.StartDate), Convert.ToInt16(item.MaxCapacity),
                    Convert.ToDouble(item.CostPerPerson), Convert.ToDouble(item.CostOfSession),
                    adminList.Split(','), item.Admin, item.LocationID, item.LocName,
                    item.LocState, item.LocLng, item.LocLat, item.Description,
                    item.ImageURL,
                    Convert.ToBoolean(item.IsCourse),
                    Convert.ToBoolean(item.AllowDropIn), Convert.ToBoolean(item.AbsorbFee),
                    Convert.ToBoolean(item.AllowReservation), Convert.ToBoolean(item.AutoReservation), Convert.ToBoolean(item.AllowPayment),
                    Convert.ToBoolean(item.Private), item.ClassType, fav));
                //"", false);

            }


            //add adverts
            paramList = new List<string>();
            paramList.Add(location);
            paramList.Add(company);

            if (searchText != "")
            {
                foreach (string word in searchText.Trim().Split(' '))
                {
                    paramList.Add("%" + word + "%");
                }
            }

            strQuery = "SELECT Advert.ID, Advert.Name, Advert.Description, Advert.Image, Advert.Email, Advert.Phone, " +
                "Advert.Address1, Advert.Address2, Advert.City, Advert.State, Advert.ContactName, Advert.Website, " +
                "Company.Name as CompanyName, " +
                "Category.ID as CategoryID, Category.Name AS CategoryName, Advert.SubCategoryID, SubCategory.Name AS SubCategoryName, " +
                "Advert.AdvertType, Advert.IsActive, Advert.Cost, Advert.CreatedDate, " +
                "Advert.BillingID, Advert.UserID FROM Advert " +
                "INNER JOIN Company ON Advert.CompanyID = Company.ID " +
                "INNER JOIN SubCategory ON Advert.SubCategoryID = SubCategory.ID " +
                "INNER JOIN Category ON SubCategory.CategoryID = Category.ID " +
                "WHERE (Advert.IsActive = 'True') ";

            if (!string.IsNullOrEmpty(location))
            {
                strQuery += " AND Advert.State = @0 ";
            }

            if (!string.IsNullOrEmpty(company))
            {
                strQuery += " AND Company.Name = @1 ";
            }

            for (int i = 2; i < paramList.Count(); i++)
            {
                if (i == 2)
                {
                    strQuery += " AND (";
                }
                else
                {
                    strQuery += " AND ";
                }
                strQuery += " (Advert.Name LIKE @" + i + " OR Advert.Description LIKE @" + i + " OR Company.Name LIKE @" + i +
                    " OR Advert.State LIKE @" + i + " OR Advert.City LIKE @" + i +
                    " OR SubCategory.Name LIKE @" + i + " OR Category.Name LIKE @" + i + " ) ";

                // full-text indexing not available on shared host.
            }

            if (paramList.Count() > 2)
            {
                strQuery += " ) ";
            }

            strQuery += " ORDER BY Advert.CreatedDate ";
            result = runQuery(strQuery, paramList);

            if (result != null)
            {
                foreach (var item in result)
                {
                    classes.Add(new GroupClass(new Guid(item.ID), item.Name, item.Description, item.Image, item.Email, item.Phone,
                        item.ContactName, item.Website, item.CategoryID.ToString(), item.CategoryName,
                        item.SubCategoryID.ToString(), item.SubCategoryName,
                        item.AdvertType, item.Cost, item.Address1,
                        item.Address2, item.City, item.State, Convert.ToDateTime(item.CreatedDate), item.CompanyName,
                        item.UserID.ToString(), item.BillingID, true));
                }
            }


            //list.OrderByDescending(x => x.Product.Name).ToList();
            classes = classes.OrderBy(d => d.NextClass()).ToList();
            return classes;
        }

        public List<GroupClass> searchFinitePublicClasses(string offSet, string rows, string companyName = "")
        {
            List<String> paramList = new List<string>();
            paramList.Add(companyName);

            string strQuery = "SELECT Class.ID, Class.Name, Class.Description, " +
                "Category.ID AS CategoryID, Category.Name AS CategoryName, SubCategory.ID AS SubCategoryID, SubCategory.Name AS SubCategoryName, " +
                "Level.ID As LevelID, Level.Name AS LevelName, Class.StartDate, Class.EndDate, Class.ClassDuration, " +
                "Class.NumberOfWeeks, Class.Repeated, Class.RepeatFrequency, Class.NumberOfLessons, Class.DaysOfWeek, " +
                "Class.CostPerPerson, Class.CostOfSession," +
                "Class.MaxCapacity, Class.Description, Class.ImageURL, Class.Company, Class.AdminID, Class.AdminID2, " +
                "Class.AdminID3, Class.AdminID4, Class.AdminID5, Class.IsCourse, " +
                "Class.AllowDropIn, Class.AbsorbFee, Class.AllowReservation, Class.AutoReservation, Class.AllowPayment, Class.Private, Class.ClassType, " +
                "UserProfile.Firstname + ' ' + UserProfile.Lastname AS Admin, " +
                "Location.ID as LocationId, Location.Name As LocName, Location.State as LocState, Location.Longitude as LocLng, Location.Latitude as LocLat, " +
                "Company.ImagePath as CompanyImage " +
                "FROM Class INNER JOIN SubCategory ON Class.SubCategoryID = SubCategory.ID  " +
                "INNER JOIN Category ON SubCategory.CategoryID = Category.ID " +
                "INNER JOIN UserProfile ON Class.AdminID = UserProfile.userID " +
                "INNER JOIN Location ON Class.LocationID = Location.ID " +
                "INNER JOIN Company ON Class.Company = Company.Name " +
                "LEFT JOIN Level on Class.LevelID = Level.ID " +
                "WHERE (Class.Private = 'False') AND ";
            if (String.IsNullOrEmpty(companyName))
            {
                strQuery += "(( (Class.IsCourse = 'false' OR Class.AllowDropIn = 'true') AND GETDATE() <= Class.EndDate) OR GETDATE() <= Class.StartDate) ";
            }
            else
            {
                strQuery += "(( (Class.IsCourse = 'false' OR Class.AllowDropIn = 'true') AND GETDATE() <= Class.EndDate) OR DATEADD(week, -4, GETDATE()) <= Class.StartDate) " +
                    " AND Company.Name = @0 ";
            }


            strQuery += " ORDER BY Class.StartDate OFFSET " + offSet + " ROWS FETCH NEXT " + rows + " ROWS ONLY";

            IEnumerable<dynamic> result = runQuery(strQuery, paramList);

            List<GroupClass> classes = new List<GroupClass>();
            foreach (var item in result)
            {
                var i = item.ID;
                string adminList = item.AdminID.ToString();
                if (item.AdminID2 != null)
                { adminList += "," + item.AdminID2.ToString(); }
                if (item.AdminID3 != null)
                { adminList += "," + item.AdminID3.ToString(); }
                if (item.AdminID4 != null)
                { adminList += "," + item.AdminID4.ToString(); }
                if (item.AdminID5 != null)
                { adminList += "," + item.AdminID5.ToString(); }

                classes.Add(new GroupClass(new Guid(item.ID), item.Company, item.CompanyImage, item.Name,
                    item.CategoryID.ToString(), item.CategoryName, item.SubCategoryID.ToString(), item.SubCategoryName,
                    //item.LevelID.ToString(), item.LevelName, 
                    Convert.ToInt16(item.ClassDuration), item.Repeated.ToString(),
                    Convert.ToInt16(item.RepeatFrequency), item.DaysOfWeek,
                    Convert.ToInt16(item.NumberOfLessons),
                    Convert.ToDateTime(item.StartDate), Convert.ToInt16(item.MaxCapacity),
                    Convert.ToDouble(item.CostPerPerson), Convert.ToDouble(item.CostOfSession),
                    adminList.Split(','), item.Admin, item.LocationID, item.LocName,
                    item.LocState, item.LocLng, item.LocLat, item.Description,
                    item.ImageURL,
                    Convert.ToBoolean(item.IsCourse),
                    Convert.ToBoolean(item.AllowDropIn), Convert.ToBoolean(item.AbsorbFee),
                    Convert.ToBoolean(item.AllowReservation), Convert.ToBoolean(item.AutoReservation), Convert.ToBoolean(item.AllowPayment),
                    Convert.ToBoolean(item.Private), ""));
                //"", false);

            }

            //list.OrderByDescending(x => x.Product.Name).ToList();
            classes = classes.OrderBy(d => d.NextClass()).ToList();
            return classes;
        }

        public List<GroupClass> searchPublicClasses(string company, string searchText)
        {
            List<String> paramList = new List<string>();
            paramList.Add(company);
            paramList.Add("%" + searchText + "%");

            IEnumerable<dynamic> result = runQuery("SELECT Class.ID, Class.Name, Class.Description, " +
                "Category.ID AS CategoryID, Category.Name AS CategoryName, SubCategory.ID AS SubCategoryID, SubCategory.Name AS SubCategoryName, " +
                "Level.ID As LevelID, Level.Name AS LevelName, Class.StartDate, Class.EndDate, Class.ClassDuration, " +
                "Class.NumberOfWeeks, Class.Repeated, Class.RepeatFrequency, Class.NumberOfLessons, Class.DaysOfWeek, " +
                "Class.CostPerPerson, Class.CostOfSession, " +
                "Class.MaxCapacity, Class.Description, Class.ImageURL, Class.Company, Class.AdminID, Class.AdminID2, " +
                "Class.AdminID3, Class.AdminID4, Class.AdminID5, Class.IsCourse, Class.ClassType, " +
                "Class.AllowDropIn, Class.AbsorbFee, Class.AllowReservation, Class.AutoReservation, Class.AllowPayment, Class.Private, " +
                "UserProfile.Firstname + ' ' + UserProfile.Lastname AS Admin, " +
                "Location.ID as LocationID, Location.Name As LocName, Location.State as LocState, Location.Longitude as LocLng, Location.Latitude as LocLat, " +
                "Company.ImagePath as CompanyImage " +
                "FROM Class INNER JOIN SubCategory ON Class.SubCategoryID = SubCategory.ID  " +
                "INNER JOIN Category ON SubCategory.CategoryID = Category.ID " +
                "INNER JOIN UserProfile ON Class.AdminID = UserProfile.userID " +
                "INNER JOIN Location ON Class.LocationID = Location.ID " +
                "INNER JOIN Company ON Class.Company = Company.Name " +
                "LEFT JOIN Level on Class.LevelID = Level.ID " +
                "WHERE (GETDATE() <= Class.EndDate) AND (Class.Private = 'False') AND (Class.Company = @0) AND " +
                "(Class.Name LIKE @1 OR Class.Description LIKE @1 " +
                "OR Category.Name LIKE @1 OR SubCategory.Name OR Location.Name LIKE @1 " +
                "OR UserProfile.Firstname LIKE @1 OR UserProfile.Lastname LIKE @1 )", paramList);

            List<GroupClass> classes = new List<GroupClass>();
            foreach (var item in result)
            {
                string adminList = item.AdminID.ToString();
                if (item.AdminID2 != null)
                { adminList += "," + item.AdminID2.ToString(); }
                if (item.AdminID3 != null)
                { adminList += "," + item.AdminID3.ToString(); }
                if (item.AdminID4 != null)
                { adminList += "," + item.AdminID4.ToString(); }
                if (item.AdminID5 != null)
                { adminList += "," + item.AdminID5.ToString(); }

                classes.Add(new GroupClass(new Guid(item.ID), company, item.CompanyImage, item.Name,
                    item.CategoryID.ToString(), item.CategoryName, item.SubCategoryID.ToString(), item.SubCategoryName,
                    //item.LevelID.ToString(), item.LevelName, 
                    Convert.ToInt16(item.ClassDuration), item.Repeated,
                    Convert.ToInt16(item.RepeatFrequency), item.DaysOfWeek,
                    Convert.ToInt16(item.NumberOfLessons), Convert.ToDateTime(item.StartDate),
                    Convert.ToInt16(item.MaxCapacity), Convert.ToDouble(item.CostPerPerson), Convert.ToDouble(item.CostOfSession),
                    adminList.Split(','), item.Admin, item.LocationID, item.LocName, item.LocState, item.LocLng, item.LocLat,
                    item.Description, item.ImageURL,
                    Convert.ToBoolean(item.IsCourse),
                    Convert.ToBoolean(item.AllowDropIn), Convert.ToBoolean(item.AbsorbFee),
                    Convert.ToBoolean(item.AllowReservation), Convert.ToBoolean(item.AutoReservation), Convert.ToBoolean(item.AllowPayment),
                    Convert.ToBoolean(item.Private), ""));

            }
            return classes;

        }

        public List<GroupClass> searchPublicClassesByDate(DateTime startDate, DateTime endDate, string company, string subCategory)
        {
            List<String> paramList = new List<string>();
            paramList.Add(startDate.ToString("yyyy/MM/dd"));
            paramList.Add(endDate.ToString("yyyy/MM/dd"));

            string searchParameters = " WHERE ((Class.StartDate >= @0 AND Class.StartDate <= @1) " +
                                      " OR (Class.EndDate <= @1 AND Class.EndDate >= @0) " +
                                      " OR (Class.StartDate <= @0 AND Class.EndDate >= @1)) ";

            if (company != "")
            {
                paramList.Add(company);
                searchParameters += " AND Company.Name = @2 ";

                if (subCategory != "")
                {
                    paramList.Add(subCategory);
                    searchParameters += " AND SubCategory.Name = @3 ";
                }
            }


            IEnumerable<dynamic> result = runQuery("SELECT Class.ID, Class.Name, Class.Description, " +
                "Category.ID AS CategoryID, Category.Name AS CategoryName, SubCategory.ID AS SubCategoryID, SubCategory.Name AS SubCategoryName, " +
                "Level.ID As LevelID, Level.Name AS LevelName, Class.StartDate, Class.EndDate, Class.ClassDuration, " +
                "Class.NumberOfWeeks, Class.Repeated, Class.RepeatFrequency, Class.NumberOfLessons, Class.DaysOfWeek, " +
                "Class.CostPerPerson, Class.CostOfSession, " +
                "Class.MaxCapacity, Class.Description, Class.ImageURL, Class.Company, Class.AdminID, Class.AdminID2, " +
                "Class.AdminID3, Class.AdminID4, Class.AdminID5, Class.IsCourse, Class.ClassType, " +
                "Class.AllowDropIn, Class.AbsorbFee, Class.AllowReservation, Class.AutoReservation, Class.AllowPayment, Class.Private, " +
                "UserProfile.Firstname + ' ' + UserProfile.Lastname AS Admin, " +
                "Location.ID as LocationID, Location.Name As LocName, Location.State as LocState, Location.Longitude as LocLng, Location.Latitude as LocLat, " +
                "Company.ImagePath as CompanyImage " +
                "FROM Class INNER JOIN SubCategory ON Class.SubCategoryID = SubCategory.ID  " +
                "INNER JOIN Category ON SubCategory.CategoryID = Category.ID " +
                "INNER JOIN UserProfile ON Class.AdminID = UserProfile.userID " +
                "INNER JOIN Location ON Class.LocationID = Location.ID " +
                "INNER JOIN Company ON Class.Company = Company.Name " +
                "LEFT JOIN Level on Class.LevelID = Level.ID " +
                searchParameters, paramList);

            List<GroupClass> classes = new List<GroupClass>();
            foreach (var item in result)
            {
                string adminList = item.AdminID.ToString();
                if (item.AdminID2 != null)
                { adminList += "," + item.AdminID2.ToString(); }
                if (item.AdminID3 != null)
                { adminList += "," + item.AdminID3.ToString(); }
                if (item.AdminID4 != null)
                { adminList += "," + item.AdminID4.ToString(); }
                if (item.AdminID5 != null)
                { adminList += "," + item.AdminID5.ToString(); }

                classes.Add(new GroupClass(new Guid(item.ID), company, item.CompanyImage, item.Name,
                    item.CategoryID.ToString(), item.CategoryName, item.SubCategoryID.ToString(), item.SubCategoryName,
                    //item.LevelID.ToString(), item.LevelName, 
                    Convert.ToInt16(item.ClassDuration), item.Repeated,
                    Convert.ToInt16(item.RepeatFrequency), item.DaysOfWeek,
                    Convert.ToInt16(item.NumberOfLessons), Convert.ToDateTime(item.StartDate),
                    Convert.ToInt16(item.MaxCapacity), Convert.ToDouble(item.CostPerPerson), Convert.ToDouble(item.CostOfSession),
                    adminList.Split(','), item.Admin, item.LocationID, item.LocName, item.LocState, item.LocLng, item.LocLat,
                    item.Description, item.ImageURL,
                    Convert.ToBoolean(item.IsCourse),
                    Convert.ToBoolean(item.AllowDropIn), Convert.ToBoolean(item.AbsorbFee),
                    Convert.ToBoolean(item.AllowReservation), Convert.ToBoolean(item.AutoReservation), Convert.ToBoolean(item.AllowPayment),
                    Convert.ToBoolean(item.Private), ""));

            }
            return classes;

        }

        public List<GroupClass> searchArchivedClasses(string company)
        {
            List<String> paramList = new List<string>();
            paramList.Add(company);

            IEnumerable<dynamic> result = runQuery("SELECT Class.ID, Class.Name, Class.Description, " +
                "Category.ID AS CategoryID, Category.Name AS CategoryName, SubCategory.ID AS SubCategoryID, SubCategory.Name AS SubCategoryName, " +
                "Level.ID As LevelID, Level.Name AS LevelName, Class.StartDate, Class.EndDate, Class.ClassDuration, " +
                "Class.NumberOfWeeks, Class.Repeated, Class.RepeatFrequency, Class.NumberOfLessons, Class.DaysOfWeek, " +
                "Class.CostPerPerson, Class.CostOfSession, " +
                "Class.MaxCapacity, Class.Description, Class.ImageURL, Class.Company, Class.AdminID, Class.AdminID2, " +
                "Class.AdminID3, Class.AdminID4, Class.AdminID5, Class.IsCourse, Class.ClassType, " +
                "Class.AllowDropIn, Class.AbsorbFee, Class.AllowReservation, Class.AutoReservation, Class.AllowPayment, Class.Private, " +
                "UserProfile.Firstname + ' ' + UserProfile.Lastname AS Admin, " +
                "Location.ID as LocationID, Location.Name As LocName, Location.State as LocState, Location.Longitude as LocLng, Location.Latitude as LocLat, " +
                "Company.ImagePath as CompanyImage " +
                "FROM Class INNER JOIN SubCategory ON Class.SubCategoryID = SubCategory.ID  " +
                "INNER JOIN Category ON SubCategory.CategoryID = Category.ID " +
                "INNER JOIN UserProfile ON Class.AdminID = UserProfile.userID " +
                "INNER JOIN Location ON Class.LocationID = Location.ID " +
                "INNER JOIN Company ON Class.Company = Company.Name " +
                "LEFT JOIN Level on Class.LevelID = Level.ID " +
                "WHERE (DATEADD(month, -6, GETDATE()) <= Class.EndDate) AND (GETDATE() >= Class.EndDate) " +
                "AND (Class.Private = 'False') AND (Class.Company = @0)", paramList);

            List<GroupClass> classes = new List<GroupClass>();
            foreach (var item in result)
            {
                string adminList = item.AdminID.ToString();
                if (item.AdminID2 != null)
                { adminList += "," + item.AdminID2.ToString(); }
                if (item.AdminID3 != null)
                { adminList += "," + item.AdminID3.ToString(); }
                if (item.AdminID4 != null)
                { adminList += "," + item.AdminID4.ToString(); }
                if (item.AdminID5 != null)
                { adminList += "," + item.AdminID5.ToString(); }

                classes.Add(new GroupClass(new Guid(item.ID), company, item.CompanyImage, item.Name,
                    item.CategoryID.ToString(), item.CategoryName, item.SubCategoryID.ToString(), item.SubCategoryName,
                    //item.LevelID.ToString(), item.LevelName, 
                    Convert.ToInt16(item.ClassDuration), item.Repeated,
                    Convert.ToInt16(item.RepeatFrequency), item.DaysOfWeek,
                    Convert.ToInt16(item.NumberOfLessons), Convert.ToDateTime(item.StartDate),
                    Convert.ToInt16(item.MaxCapacity), Convert.ToDouble(item.CostPerPerson), Convert.ToDouble(item.CostOfSession),
                    adminList.Split(','), item.Admin, item.LocationID, item.LocName, item.LocState, item.LocLng, item.LocLat,
                    item.Description, item.ImageURL,
                    Convert.ToBoolean(item.IsCourse),
                    Convert.ToBoolean(item.AllowDropIn), Convert.ToBoolean(item.AbsorbFee),
                    Convert.ToBoolean(item.AllowReservation), Convert.ToBoolean(item.AutoReservation), Convert.ToBoolean(item.AllowPayment),
                    Convert.ToBoolean(item.Private), ""));

            }
            return classes;

        }

        #endregion

    }
}
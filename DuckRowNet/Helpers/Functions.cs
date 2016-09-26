using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DuckRowNet.Helpers.Object;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DuckRowNet.Helpers
{
    public class Functions
    {
        public enum Repeat { never, Day, Week, Month };

        public enum PersonType { User, Client };

        public static int getDayOfWeekAsInt(string day)
        {
            if (day == "Monday")
                return 1;
            if (day == "Tuesday")
                return 2;
            if (day == "Wednesday")
                return 3;
            if (day == "Thursday")
                return 4;
            if (day == "Friday")
                return 5;
            if (day == "Saturday")
                return 6;
            if (day == "Sunday")
                return 7;
            else
                return -1;
        }

        public static List<DateTime> getClassDates(GroupClass gClass)
        {
            List<DateTime> classDates = new List<DateTime>();
            int dayCount = 0;

            string[] days = gClass.RepeatDays.Split('|');
            if (String.IsNullOrEmpty(gClass.RepeatDays) || gClass.Repeated != Functions.Repeat.Week)
            {
                days[0] = gClass.StartDate.DayOfWeek.ToString();
            }

            //calculate which "repeat day" to start on by matching it to the startdate dayofweek
            //should this move the start day to the next day?
            if (gClass.RepeatDays != "")
            {
                if (days.Contains(gClass.StartDate.DayOfWeek.ToString()))
                {
                    foreach (string d1 in days)
                    {
                        dayCount++;
                        if (d1 == gClass.StartDate.DayOfWeek.ToString())
                            break;
                    }
                }
                else
                {
                    int d = getDayOfWeekAsInt(gClass.StartDate.DayOfWeek.ToString());

                    foreach (string d1 in days)
                    {
                        dayCount++;
                        if (getDayOfWeekAsInt(d1) >= d)
                        {
                            gClass.StartDate = gClass.StartDate.AddDays(getDayOfWeekAsInt(d1) - d);
                            break;
                        }
                        else if (d1 == days.Last())
                        { //first day is next week.
                            gClass.StartDate = gClass.StartDate.AddDays(7 + (getDayOfWeekAsInt(days.First()) - d));
                            dayCount = 1;
                        }
                    }
                }
            }

            DateTime dateTracker = gClass.StartDate;
            classDates.Add(dateTracker);

            if (gClass.Repeated == Functions.Repeat.Day)
            {
                for (int i = 1; i < gClass.NumberOfLessons; i++)
                {
                    dateTracker = gClass.StartDate.AddDays(i * gClass.RepeatFrequency);
                    classDates.Add(dateTracker);
                }
            }
            else if (gClass.Repeated == Functions.Repeat.Week)
            {
                for (int i = 2; i <= gClass.NumberOfLessons; i++)
                {
                    if (dayCount >= days.Count())
                    {
                        dayCount = 0;
                    }

                    int currentDay = getDayOfWeekAsInt(dateTracker.DayOfWeek.ToString());
                    int nextDay = getDayOfWeekAsInt(days[dayCount]);

                    if (nextDay <= currentDay)
                    {
                        dateTracker = dateTracker.AddDays(gClass.RepeatFrequency * 7);
                    }
                    dateTracker = dateTracker.AddDays(nextDay - currentDay);
                    classDates.Add(dateTracker);

                    dayCount++;
                }
            }

            return classDates;
        }

        public static void ResizeImage(string ImageFile, int NewWidth, int MaxHeight, bool OnlyResizeIfWider)
        {
            //if(ImageFile.StartsWith("~"))
            //{
            //    ImageFile = ImageFile.Substring(2);   
            //}

            System.Drawing.Image FullsizeImage = System.Drawing.Image.FromFile(HttpContext.Current.Server.MapPath(ImageFile));

            // Prevent using images internal thumbnail
            FullsizeImage.RotateFlip(System.Drawing.RotateFlipType.Rotate180FlipNone);
            FullsizeImage.RotateFlip(System.Drawing.RotateFlipType.Rotate180FlipNone);

            if (OnlyResizeIfWider)
            {
                if (FullsizeImage.Width <= NewWidth)
                {
                    NewWidth = FullsizeImage.Width;
                }
            }

            int NewHeight = FullsizeImage.Height * NewWidth / FullsizeImage.Width;
            if (NewHeight < 200)
            {
                NewHeight = 200;
                NewWidth = FullsizeImage.Width * NewHeight / FullsizeImage.Height;
            }

            if (NewHeight > MaxHeight)
            {
                // Resize with height instead
                NewWidth = FullsizeImage.Width * MaxHeight / FullsizeImage.Height;
                NewHeight = MaxHeight;
            }

            System.Drawing.Image NewImage = FullsizeImage.GetThumbnailImage(NewWidth, NewHeight, null, IntPtr.Zero);

            // Clear handle to original file so that we can overwrite it if necessary
            FullsizeImage.Dispose();

            // Save resized picture
            NewImage.Save(HttpContext.Current.Server.MapPath(ImageFile));


            //create cropped thumb for FB
            if (NewHeight > 100)
            {
                NewHeight = 100;
            }
            if (NewWidth > 100)
            {
                NewWidth = 100;
            }
            Rectangle cropArea = new Rectangle(0, 0, NewWidth, NewHeight);
            Bitmap bmImage = new Bitmap(NewImage);
            bmImage = bmImage.Clone(cropArea, bmImage.PixelFormat);

            String filename = HttpContext.Current.Server.MapPath(ImageFile);
            filename = filename.Substring(0, filename.Length - 4) + "_FB" + filename.Substring(filename.Length - 4);
            bmImage.Save(filename);


        }

        public static Double calculateTotalCost(double costPerPerson)
        {
            double fee = calculateDuckRowFee(costPerPerson);
            fee += calculatePaypalFee(costPerPerson);
            return fee + costPerPerson;
        }

        public static Double calculateDuckRowFee(double costPerPerson)
        {
            return 0;

            //double fee = costPerPerson * 0.02;
            //if (fee < 1.8)
            //{
            //    fee = 1.79;
            //}
            //return fee;
        }

        public static Double calculatePaypalFee(double costPerPerson)
        {
            double fee = (costPerPerson * 0.034) + 0.35;
            return fee;
        }

        public static Double calculateFee(double costPerPerson)
        {
            double fee = calculateDuckRowFee(costPerPerson);
            fee += calculatePaypalFee(costPerPerson);
            return fee;
        }

        public static String convertToSlug(string slug)
        {
            if(!String.IsNullOrEmpty(slug))
                return Regex.Replace(slug, @"[^A-Za-z0-9_\.~]+", "-");

            return "";
        }


    }
}
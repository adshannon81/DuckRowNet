using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DuckRowNet.Helpers.Object
{
    public class SubscriptionType
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int MaxUsers { get; set; }
        public int MaxAdverts { get; set; }
        public int MaxClasses { get; set; }
        public double MonthlyCost { get; set; }


        public SubscriptionType()
        {
            //Default
            ID = 0;
            Name = "";
            MaxUsers = 0;
            MaxAdverts = 0;
            MaxClasses = 0;
            MonthlyCost = 0;
        }

        public SubscriptionType(int id, string name, int maxUsers, int maxAdverts, int maxClasses, double monthlyCost)
        {
            ID = id;
            Name = name;
            MaxUsers = maxUsers;
            MaxAdverts = maxAdverts;
            MaxClasses = maxClasses;
            MonthlyCost = monthlyCost;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DuckRowNet.Helpers.Object
{
    public class ScheduledItem
    {
        public string ID { get; set; }
        public DateTime StartDate { get; set; }
        public bool Paid { get; set; }
        public bool Cancelled { get; set; }

        public ScheduledItem()
        { }

        public ScheduledItem(string id, DateTime startDate, bool paid, bool cancelled)
        {
            ID = id;
            StartDate = startDate;
            Paid = paid;
            Cancelled = cancelled;
        }
    }
}
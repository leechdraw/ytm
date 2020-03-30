using System;

namespace ytm.UI.Models
{
    public class LogItemModel
    {
        public LogItemModel()
        {
            Time = DateTime.Now;
        }
        public string Text { get; set; }
        public DateTime Time { get; set; }
        public LogItemType Type { get; set; }
    }
}
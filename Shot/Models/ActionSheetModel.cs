using System;
namespace Shot.Models
{
    public class ActionSheetModel
    {
        public string Title { get; set; }
        public string Cancel { get; set; }
        public string Distruction { get; set; }
        public string[] Buttons { get; set; }
    }
}

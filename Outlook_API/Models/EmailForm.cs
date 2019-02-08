using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace dotnet_tutorial.Models
{
    public class EmailForm
    {
        public string EmailToId { get; set; }
        public string EmailTo { get; set; }
        public string EmailBody { get; set; }
        public string EmailSubject { get; set; }
    }
}
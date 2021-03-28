using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CoronaIcivet.Models
{
    public class InforUser
    {
            public string openid { get; set; }
            public int is_subscriber { get; set; }
            public int from_pc { get; set; }
            public string civetno { get; set; }
            public string realname { get; set; }
            public string sex { get; set; }
            public string nickname { get; set; }
            public string area { get; set; }
            public string sign { get; set; }
            public string icon { get; set; }
            public string serviceno { get; set; }
            public string hrtype { get; set; }
            public string unit { get; set; }
            public string bg { get; set; }
            public string bu { get; set; }
            public string grade { get; set; }
            public string grade_depart { get; set; }
            public string access_token { get; set; }
            public int expires_in { get; set; }
    }
}
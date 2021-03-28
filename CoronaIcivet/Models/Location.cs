using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CoronaIcivet.Models
{
    public class Location
    {
        public string civetno { get; set; }
        public DateTime sign_date { get; set; }
        public string sign_ip { get; set; }
        public double longitude { get; set; }
        public double latitude { get; set; }
        public bool is_sign_succeed { get; set; }
        public string user_name { get; set; }
        public int sign_point_id { get; set; }
        public string sign_point_name { get; set; }
        public double p_longitude { get; set; }
        public double p_latitude { get; set; }
    }
    public class LocationData
    {
        public string code { get; set; }
        public List<Location> data { get; set; }
        public object message { get; set; }
    }
    public class inforEmp
    {
        public string ID_Card { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Warring { get; set; }
    }
}
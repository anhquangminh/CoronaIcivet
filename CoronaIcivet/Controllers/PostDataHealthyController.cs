using CoronaIcivet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Data;

namespace CoronaIcivet.Controllers
{
    public class PostDataHealthyController : ApiController
    {
        // GET: PostDataHealthy

        [System.Web.Http.HttpPost]
        public string postData(apiHealthModels apiHealth)
        {
            string res = "Fail";
            using (SqlConnection conn = new SqlConnection(@"Data Source=10.224.81.131,3000;Initial Catalog=WechatDB;Persist Security Info=True;User ID=khangbeoit;Password=foxconn168!!"))
            {
                try
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    if (apiHealth.MaThe == null) apiHealth.MaThe = "";
                    if (apiHealth.Ten == null) apiHealth.Ten = "";

                    string codeCh = DateTime.Now.ToString("yyyyMMddHHmmss") + GenerateRandom(3);
                    string empId = apiHealth.MaThe;
                    string name = apiHealth.Ten;



                    string sucKhoe = "";

                    string ipUser = getIPCom();

                    string lydoKhong = "";

                    string flagWar = "0";
                    string stateWar = "1";

                    if (apiHealth.NhietDo == null) apiHealth.NhietDo = "";
                    if (apiHealth.VitriCachly == null) apiHealth.VitriCachly = "";
                    if (apiHealth.tgbatdau == null) apiHealth.tgbatdau = "";
                    if (apiHealth.tgketthuc == null) apiHealth.tgketthuc = "";
                    if (apiHealth.quayLai == null) apiHealth.quayLai = "";

                    if (apiHealth.lydo == null) apiHealth.lydo = "";

                    if (apiHealth.SucKhoe == null) apiHealth.SucKhoe = "";


                    if (apiHealth.lydo == null) apiHealth.lydo = "";

                    if (apiHealth.quayLai == "khong")
                    {
                        lydoKhong = apiHealth.lydo;
                        flagWar = "1";
                    }

                    string date = DateTime.Now.ToString("yyyy-MM-dd");
                    sucKhoe = apiHealth.SucKhoe;

                    string strQuery = @"insert into CheckPersonalHealth(EmpNo,EmpName,Temperature1,Location,Description,State,Date,DateStart,DateEnd,CorContent,Flag,kinhTuyen,viTuyen,IPSub,CodeCheck)
values (@EmpNo,@EmpName,@Temperature1,@Location,@Description,@State,@Date,@DateStart,@DateEnd,@CorContent,@Flag,@kinhTuyen,@viTuyen,@IPSub,@CodeCheck)";

                    SqlParameter[] param = new SqlParameter[15];

                    param.SetValue(new SqlParameter("EmpNo", empId), 0);
                    param.SetValue(new SqlParameter("EmpName", name), 1);
                    param.SetValue(new SqlParameter("Temperature1", apiHealth.NhietDo), 2);
                    param.SetValue(new SqlParameter("Location", apiHealth.VitriCachly), 3);
                    param.SetValue(new SqlParameter("Description", sucKhoe), 4);

                    param.SetValue(new SqlParameter("DateStart", apiHealth.tgbatdau), 5);
                    param.SetValue(new SqlParameter("DateEnd", apiHealth.tgketthuc), 6);
                    param.SetValue(new SqlParameter("CorContent", lydoKhong), 7);
                    param.SetValue(new SqlParameter("Flag", flagWar), 8);
                    param.SetValue(new SqlParameter("kinhTuyen", apiHealth.KinhTuyen), 9);

                    param.SetValue(new SqlParameter("viTuyen", apiHealth.ViTuyen), 10);
                    param.SetValue(new SqlParameter("IPSub", ipUser), 11);
                    param.SetValue(new SqlParameter("State", stateWar), 12);
                    param.SetValue(new SqlParameter("CodeCheck", codeCh), 13);
                    param.SetValue(new SqlParameter("Date", date), 14);

                    SqlCommand cmd = new SqlCommand(strQuery, conn);
                    cmd.Parameters.AddRange(param);
                    int rowFF = cmd.ExecuteNonQuery();
                    if (rowFF > 0)
                    {
                        res = "OK";
                    }


                    if (conn.State != ConnectionState.Closed) conn.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }


            return res;
        }

        private static char[] constant =
       {
            '0','1','2','3','4','5','6','7','8','9'
        };
        private static string GenerateRandom(int Length)
        {
            System.Text.StringBuilder newRandom = new System.Text.StringBuilder(10);
            Random rd = new Random();
            for (int i = 0; i < Length; i++)
            {
                newRandom.Append(constant[rd.Next(10)]);
            }
            return newRandom.ToString();
        }

        public string getIPCom()
        {
            string ip = System.Web.HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (string.IsNullOrEmpty(ip))
            {
                ip = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }
            return ip;
        }
    }
}
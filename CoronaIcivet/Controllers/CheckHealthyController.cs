using CoronaIcivet.Common;
using CoronaIcivet.Models;
using Newtonsoft.Json;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Runtime.Remoting.Contexts;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace CoronaIcivet.Controllers
{
    public class CheckHealthyController : Controller
    {
        // GET: CheckHealthy
        public async System.Threading.Tasks.Task<ActionResult> Index()
        {
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(ValidationCertificate);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            if (Session["errorLog1"] != null) ViewBag.ErrorLog = "Vui lòng chọn tình trạng sức khỏe";

            if (Request.Params["code"] == null)
            {
               Response.Redirect(String.Format("http://civetinterface.foxconn.com/Open/oauth/?to_code=vnnumberone"));
            }
            else
            {
                string datetime = DateTime.Now.ToString("yyyy/MM/dd");
                string inputDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

                InforUser inforUser = getInforUser(Request.Params["code"]);
                if (inforUser == null)
                {
                    ViewBag.Error = "Can't get User info from iCivet. Code=" + Request.Params["code"];
                    RedirectToAction("Index", "Error");
                }
                string icivet = inforUser.civetno;
                List<Location> location;
                string a = "";
                string b = "";
               
                location = await PostDataAsync(icivet, datetime, datetime);
                if (location == null)
                {
                    ViewBag.Error = "Vui lòng chọn định vị !";
                    RedirectToAction("Index", "Error");
                }
                if (location != null)
                {
                    foreach (var lc in location)
                    {
                        a = lc.latitude.ToString();
                        b = lc.longitude.ToString();                       
                    }
                }
                ViewBag.icivet = icivet;
                ViewBag.realname = inforUser.realname;
                ViewBag.bu = inforUser.bu;
                ViewBag.kt = a;
                ViewBag.vt = b;

            }

            return View();
        }

        [HttpPost]
        public ActionResult Index(HealthModels health)
        {
            using (SqlConnection conn = new SqlConnection(@"Data Source=10.224.81.131,3000;Initial Catalog=WechatDB;Persist Security Info=True;User ID=khangbeoit;Password=foxconn168!!"))
            {
                try
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    string codeCh = DateTime.Now.ToString("yyyyMMddHHmmss") + GenerateRandom(3);
                    string datetimenow = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                    string empId = health.empNo;
                    string name = health.empName;

                    DateTime dateTimeC = DateTime.Now;


                    DateTime dateTimeS1 = Convert.ToDateTime("8:00");
                    DateTime dateTimeE1 = Convert.ToDateTime("10:00");


                    TimeSpan diff1 = dateTimeC - dateTimeS1;
                    TimeSpan diff2 = dateTimeC - dateTimeE1;
                    double hours1 = diff1.TotalHours;
                    double hours2 = diff2.TotalHours;

                    

                    DateTime  dateTimeS2 = Convert.ToDateTime("14:30");
                    DateTime dateTimeE2 = Convert.ToDateTime("16:30");

                    TimeSpan diff3 = dateTimeC - dateTimeS2;
                    TimeSpan diff4 = dateTimeC - dateTimeE2;
                    double hours3 = diff3.TotalHours;
                    double hours4 = diff4.TotalHours;

                    //(hours1 >= 0 && hours2 <= 0) || (hours3 >= 0 && hours4 <= 0)
                    if ((hours1 >= 0 && hours2 <= 0) || (hours3 >= 0 && hours4 <= 0))
                    {
                        //ViewBag.NgoaiGio = "Nằm ngoài thời gian khai báo";

                        string sucKhoe = "";

                        string ipUser = getIPCom();

                        string lydoKhong = "";

                        string flagWar = "0";
                        string stateWar = "1";
                        string kt = Convert.ToString(health.kinhtuyen);
                        string vt = Convert.ToString(health.vituyen);


                        if (health.Nhiet1 == null) health.Nhiet1 = "";
                        if (health.vitricachly == null) health.vitricachly = "";
                        if (health.tgbatdau == null) health.tgbatdau = "";
                        if (health.tgketthuc == null) health.tgketthuc = "";
                        if (health.cars == null) health.cars = "";

                        if (health.lydo == null) health.lydo = "";

                        if (health.check3 != null) sucKhoe += health.check3 + ";";
                        if (health.check2 != null) sucKhoe += health.check2 + ";";
                        if (health.check4 != null) sucKhoe += health.check4 + ";";
                        if (health.check5 != null) sucKhoe += health.check5 + ";";

                        if (health.check1 != null || health.check3 != null || health.check2 != null || health.check4 != null || health.check5 != null)
                        {
                            sucKhoe += health.check1 + ";";
                            stateWar = "1";
                        }


                        if (health.check6 != null)
                        {
                            sucKhoe = health.check6;
                            stateWar = "0";
                        }

                        if (health.lydo == null) health.lydo = "";

                        if (health.cars == "khong")
                        {
                            lydoKhong = health.lydo;
                            flagWar = "1";
                        }
                        if (health.check1 == null && health.check3 == null && health.check2 == null && health.check4 == null && health.check5 == null && health.check6 == null)
                        {
                            sucKhoe = "Không chọn tình trạng sức khỏe";
                            stateWar = "6";
                            Session["errorLog1"] = "Vui lòng chọn tình trạng sức khỏe";
                            ViewBag.ErrorLog = "Vui lòng chọn tình trạng sức khỏe";
                            return RedirectToAction("Index", "CheckHealthy");
                        }
                        string strQuery = @"insert into CheckPersonalHealth(
EmpNo ,EmpName ,Dept ,Temperature1 ,Location ,Description ,
State ,Date ,DateStart ,DateEnd ,CorContent ,Flag ,kinhTuyen ,
viTuyen ,IPSub ,CodeCheck)
values (N'{0}',N'{1}',N'{2}',N'{3}',N'{4}',
N'{5}',N'{6}',N'{7}',N'{8}',N'{9}',
N'{10}',N'{11}',N'{12}',N'{13}',N'{14}',N'{15}')";

                        strQuery = string.Format(strQuery, empId, health.empName, health.bu, health.Nhiet1, health.vitricachly, sucKhoe,
                            stateWar, datetimenow, health.tgbatdau, health.tgketthuc, lydoKhong, flagWar, kt, vt, ipUser, codeCh);
                        SqlCommand cmd = new SqlCommand(strQuery, conn);
                        //cmd.Parameters.AddRange(param);
                        int rowFF = cmd.ExecuteNonQuery();
                        if (rowFF > 0)
                        {
                            Session["Res"] = true;
                            string dateChe = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                            Session["tg"] = dateChe.ToString();
                            Session["inforuser"] = "Mã thẻ : "+empId +" , Tên : "+health.empName;
                            Session["nhiet"] = "Nhiệt độ : "+ health.Nhiet1;
                            Session["suckhoe"] = "Tình trạng sức khỏe : "+ sucKhoe;
                            Session["vitri"] = "Vị trí cách ly : " + health.vitricachly;
                            Session["dinhvi"] = " Định vị : " + kt +","+vt;
                            Session["ngay"] = "Thời gian cách ly : "+ health.tgbatdau + " - "+health.tgketthuc;

                        }
                        else
                        {
                            Session["Res"] = false;
                        }
                        if (conn.State != ConnectionState.Closed) conn.Close();

                        if (stateWar == "1")
                        {
                            //vnnumberone - key
                            //光明 - group
                            //xin - message
                            //var apiWChat = new apiWebChatModels();
                            //apiWChat.key = "vnnumberone";
                            //apiWChat.group = "光明";
                            //apiWChat.message = "Mã thẻ: "+ empId + " \r\nTên : "+ health.empName + " \r\nĐang có dấu hiệu "+ sucKhoe + " vui lòng kiểm tra";
                            //using (var client = new HttpClient())
                            //{
                            //    string URL = "http://appvn5.foxconn.com:8080/api/SendMessage";

                            //    string urlParameters = "?key="+ apiWChat.key + "&group="+apiWChat.group+"&message="+apiWChat.message+"";

                            //    client.BaseAddress = new Uri(URL);

                            //    // Add an Accept header for JSON format.
                            //    client.DefaultRequestHeaders.Accept.Add(
                            //    new MediaTypeWithQualityHeaderValue("application/json"));

                            //    // List data response.
                            //    HttpResponseMessage response = client.GetAsync(urlParameters).Result;
                            //    if (response.IsSuccessStatusCode)
                            //    {
                            //        string test = "ok";
                            //    }
                            //}
                        }
                    }
                    else
                    {
                        ViewBag.NgoaiGio = "Khai báo không thành công do nằm ngoài thời gian khai báo  ";
                        return View(health);
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return RedirectToAction("Result");
        }
        public ActionResult InsertExcel()
        {
            var sess = (UserLogin)Session[CommonConstants.USER_SESSION];
            if (sess == null)
            {
                return RedirectToAction("Login","CheckHealthy");
            }

            return View();
        }

        [HttpPost]
        public ActionResult InsertExcel(HttpPostedFileBase file)
        {
            DateTime dateTimeC = DateTime.Now;

            DateTime dateTimeS1 = Convert.ToDateTime("17:15");
            DateTime dateTimeE1 = Convert.ToDateTime("17:30");

            TimeSpan diff1 = dateTimeC - dateTimeS1;
            TimeSpan diff2 = dateTimeC - dateTimeE1;
            double hours1 = diff1.TotalHours;
            double hours2 = diff2.TotalHours;
            //(hours1 >= 0 && hours2 <= 0)
            if (1==1)
            {
                List<UserCheck> ListUser = new List<UserCheck>();
                ViewBag.Data = null;
                var sess = (UserLogin)Session[CommonConstants.USER_SESSION];
                string nameU = sess.UserID.ToLower();
                string filePath = string.Empty;
                if (file != null)
                {
                    string path = Server.MapPath("~/Uploads/");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }

                    filePath = path + Path.GetFileName(file.FileName);
                    string extension = Path.GetExtension(file.FileName);
                    file.SaveAs(filePath);

                    string fileN = nameU + "_" + file.FileName;

                    string conString = string.Empty;

                    switch (extension)
                    {
                        case ".xls": //Excel 97-03.
                            conString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + filePath + ";Extended Properties='Excel 8.0;HDR=YES'";
                            break;
                        case ".xlsx": //Excel 07 and above.
                            conString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + filePath + ";Extended Properties='Excel 8.0;HDR=YES'";
                            break;
                    }

                    DataTable dt = new DataTable();
                    conString = string.Format(conString, filePath);

                    using (OleDbConnection connExcel = new OleDbConnection(conString))
                    {
                        using (OleDbCommand cmdExcel = new OleDbCommand())
                        {
                            using (OleDbDataAdapter odaExcel = new OleDbDataAdapter())
                            {
                                cmdExcel.Connection = connExcel;

                                //Get the name of First Sheet.
                                connExcel.Open();
                                DataTable dtExcelSchema;
                                dtExcelSchema = connExcel.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                                string sheetName = dtExcelSchema.Rows[0]["TABLE_NAME"].ToString();

                                cmdExcel.CommandText = "SELECT * From [" + sheetName + "]";
                                odaExcel.SelectCommand = cmdExcel;
                                odaExcel.Fill(dt);
                                connExcel.Close();
                            }
                        }
                    }

                    conString = @"Data Source=10.224.81.131,3000;Initial Catalog=WechatDB;Persist Security Info=True;User ID=khangbeoit;Password=foxconn168!!";
                    using (SqlConnection con = new SqlConnection(conString))
                    {
                        try
                        {
                            //string strDele = @"Delete tbEmpoyeeCheck where AccUpload ='"+ nameU.Trim() + "'; ";
                            //SqlCommand cmd1 = new SqlCommand(strDele, con);
                            //cmd1.ExecuteNonQuery();

                            string ma = ""; string ten = ""; string bu = ""; string dateS = ""; string dateE = ""; string AccUp = "";

                            int kq = 0;
                            int count = 0;
                            int countupdate = 0;
                            if (dt.Rows.Count > 0)
                            {
                                insertEmpUpdate(nameU.Trim().ToUpper());
                                UserCheck user;
                                for (int i = 0; i < dt.Rows.Count; i++)
                                {
                                    if (!String.IsNullOrEmpty(dt.Rows[i]["Mathe"].ToString()))
                                    {

                                        count++;
                                        ma = dt.Rows[i]["Mathe"].ToString();
                                        ten = dt.Rows[i]["Ten"].ToString();
                                        bu = dt.Rows[i]["BU"].ToString();
                                        dateS = dt.Rows[i]["TimeStart"].ToString();
                                        dateE = dt.Rows[i]["TimeEnd"].ToString();
                                        AccUp = nameU;

                                        user = new UserCheck();
                                        user.Mathe = ma;
                                        user.Ten = ten;
                                        user.BU = bu;
                                        user.TimeStart = dateS;
                                        user.TimeEnd = dateE;
                                        user.AccUpload = AccUp.ToUpper();
                                        ListUser.Add(user);

                                        if (checkEmp(ma.ToUpper(), AccUp.ToUpper()) == true)
                                        {
                                            countupdate++;
                                            updateEmp(ma.Trim(), ten, bu.ToUpper(), dateS, dateE, AccUp);
                                        }
                                        else
                                        {
                                            bool checkFlag = insertData(ma.Trim(), ten, bu, dateS, dateE, AccUp.ToUpper());
                                            if (checkFlag) kq = kq + 1;
                                        }

                                    }
                                }

                            }

                            string ipIm = getIPCom();
                            if (con.State != ConnectionState.Open) con.Open();
                            string strInto = @"insert into ImportExcelLog(Mathe,ThoiGian,TenFile,IPImport) values (@Mathe,GETDATE(),@TenFile,@IPImport)";
                            SqlParameter[] para = new SqlParameter[3];

                            para.SetValue(new SqlParameter("Mathe", nameU), 0);
                            para.SetValue(new SqlParameter("TenFile", fileN), 1);
                            para.SetValue(new SqlParameter("IPImport", ipIm), 2);

                            SqlCommand cmd = new SqlCommand(strInto, con);
                            cmd.Parameters.AddRange(para);
                            cmd.ExecuteNonQuery();
                            int rowFF = cmd.ExecuteNonQuery();
                            if (rowFF > 0)
                            {
                                ViewBag.Message = "File Imported and excel data saved into database " + "----- \r\n Đã thêm: " + kq + "/" + count + "dòng. \r\n Đã cập nhật : " + countupdate + " dòng.";
                                if (ListUser != null)
                                {
                                    ViewBag.Data = ListUser;
                                }

                            }
                            else
                            {
                                ViewBag.Message = "Import excel fail !";
                            }
                            if (con.State != ConnectionState.Closed) con.Close();

                        }
                        catch (Exception ex)
                        {
                            ViewBag.Message = "Import data fail " + ex.Message;
                        }
                    }
                }
            }
            else
            {
                @ViewBag.Message = "Thoi gian tai len file excel khong nam trong khoang 17:15 - 17:30 hang ngay";
            }
           
            //if the code reach here means everthing goes fine and excel data is imported into database
            
            return View();
        }

        public bool checkEmp(string emp,string AccUpload)
        {
            string sql = @"SELECT * FROM [dbo].[tbEmpoyeeCheck] where Mathe='"+emp+"' and AccUpload='"+AccUpload+"'";
            string strConn = @"Data Source=10.224.81.131,3000;Initial Catalog=WechatDB;Persist Security Info=True;User ID=khangbeoit;Password=foxconn168!!";
            using (SqlConnection con = new SqlConnection(strConn))
            {
                if (con.State != ConnectionState.Open) con.Open();
                List<UserCheck> Listdp = new List<UserCheck>();
                DataTable dt = new DataTable();  
                SqlDataAdapter da = new SqlDataAdapter(sql, con);
                da.Fill(dt);
                da.Dispose();
                if (dt.Rows.Count > 0)
                {  
                    return true;
                }

                if (con.State != ConnectionState.Closed) con.Close();
            }
            return false;
        }

        public bool insertEmpUpdate(string AccUpload)
        {
            string sql = @"SELECT * FROM [dbo].[tbEmpoyeeCheck] where AccUpload='" + AccUpload + "'";
            string strConn = @"Data Source=10.224.81.131,3000;Initial Catalog=WechatDB;Persist Security Info=True;User ID=khangbeoit;Password=foxconn168!!";
            using (SqlConnection con = new SqlConnection(strConn))
            {
                if (con.State != ConnectionState.Open) con.Open();
                List<UserCheck> Listdp = new List<UserCheck>();
                DataTable dt = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(sql, con);
                da.Fill(dt);
                da.Dispose();
                if (dt.Rows.Count > 0)
                {
                    string inputDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                    for (int i = 0; i < dt.Rows.Count; i++)
                    { 
                        insertDataUpdate(dt.Rows[i]["Mathe"].ToString().ToUpper(), dt.Rows[i]["Ten"].ToString(), dt.Rows[i]["BU"].ToString(), dt.Rows[i]["TimeStart"].ToString(), dt.Rows[i]["TimeEnd"].ToString(), dt.Rows[i]["AccUpload"].ToString().ToUpper(), inputDate);
                    }
                    return true;
                }

                if (con.State != ConnectionState.Closed) con.Close();
            }
            return false;
        }

        public bool updateEmp(string ma, string ten, string bu, string dateS, string dateE, string AccUp)
        {
            DateTime timeS = Convert.ToDateTime(dateS);
            DateTime timeE = Convert.ToDateTime(dateE);

            string strInsert = @"UPDATE tbEmpoyeeCheck SET Ten=N'"+ten.Trim()+"',BU=N'"+bu.Trim()+"',TimeStart='"+timeS+"',TimeEnd='"+timeE+ "',AccUpload='"+AccUp.ToUpper()+"' where  Mathe ='" + ma.Trim()+ "' and AccUpload ='"+ AccUp.Trim() + "'";
            string strConn = @"Data Source=10.224.81.131,3000;Initial Catalog=WechatDB;Persist Security Info=True;User ID=khangbeoit;Password=foxconn168!!";
            using (SqlConnection con = new SqlConnection(strConn))
            {
                if (con.State != ConnectionState.Open) con.Open();
                SqlCommand cmd = new SqlCommand(strInsert, con);
                int rowFF = cmd.ExecuteNonQuery();
                if (rowFF > 0)
                {
                    return true;
                }
                if (con.State != ConnectionState.Closed) con.Close();
            }

            return false;
        }
        public bool insertData(string ma,string ten,string bu,string dateS,string dateE,string AccUp)
        {
            DateTime timeS = Convert.ToDateTime(dateS);
            DateTime timeE = Convert.ToDateTime(dateE);

            bool res = false;

            string strInsert = @"insert into tbEmpoyeeCheck(Mathe,Ten,BU,TimeStart,TimeEnd,AccUpload) values (N'"+ma+ "',N'" + ten + "',N'" + bu + "','" + timeS + "','" + timeE + "','" + AccUp + "') ";
            string strConn = @"Data Source=10.224.81.131,3000;Initial Catalog=WechatDB;Persist Security Info=True;User ID=khangbeoit;Password=foxconn168!!";
            using (SqlConnection con = new SqlConnection(strConn))
            {
                if (con.State != ConnectionState.Open) con.Open();

                SqlCommand cmd = new SqlCommand(strInsert, con);
                int rowFF = cmd.ExecuteNonQuery();
                
                if(rowFF > 0)
                {
                    res = true;
                }
                if (con.State != ConnectionState.Closed) con.Close();
            }

            return res;
        }

        public bool insertDataUpdate(string ma, string ten, string bu, string dateS, string dateE, string AccUp,string timeupdate)
        {
            DateTime timeS = Convert.ToDateTime(dateS);
            DateTime timeE = Convert.ToDateTime(dateE);

            bool res = false;

            string strInsert = @"insert into EmpLogUpdate(Mathe,Ten,BU,TimeStart,TimeEnd,AccUpload,TimeUpdate)  values (N'" + ma + "',N'" + ten + "',N'" + bu + "','" + timeS + "','" + timeE + "','" + AccUp + "','"+timeupdate+"') ";
            string strConn = @"Data Source=10.224.81.131,3000;Initial Catalog=WechatDB;Persist Security Info=True;User ID=khangbeoit;Password=foxconn168!!";
            using (SqlConnection con = new SqlConnection(strConn))
            {
                if (con.State != ConnectionState.Open) con.Open();

                SqlCommand cmd = new SqlCommand(strInsert, con);
                int rowFF = cmd.ExecuteNonQuery();

                if (rowFF > 0)
                {
                    res = true;
                }
                if (con.State != ConnectionState.Closed) con.Close();
            }

            return res;
        }

        public ActionResult Search(int? page)
        {

            if (page == null) page = 1;
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            string strWhere = "";
            //if (!string.IsNullOrEmpty(Mathe))
            //{
            //    string orderNo = @"EmpNo like '%" + Mathe + "%'";
            //    if (strWhere.Equals("")) strWhere += " and " + orderNo;
            //    else strWhere += " and " + orderNo;
            //}

            //if (!string.IsNullOrEmpty(Name))
            //{
            //    string orderNo = @"EmpName like '%" + Name + "%'";
            //    if (strWhere.Equals("")) strWhere += " and " + orderNo;
            //    else strWhere += " and " + orderNo;
            //}


            //if (!string.IsNullOrEmpty(Date))
            //{
            //    DateTime dateC = Convert.ToDateTime(Date);
            //    string dateChe = dateC.ToString("yyyy/MM/dd");
            //    string orderNo = @"Date like '%" + dateChe + "%'";
            //    if (strWhere.Equals("")) strWhere += " and " + orderNo;
            //    else strWhere += " and " + orderNo;
            //}

            //if (!string.IsNullOrEmpty(BU))
            //{
            //    string orderNo = @"Dept like '%" + BU + "%'";
            //    if (strWhere.Equals("")) strWhere += " and " + orderNo;
            //    else strWhere += " and " + orderNo;
            //}
            //if (Check1 != null)
            //{
            //    string orderNo = @"State = '2'";
            //    strWhere += " and " + orderNo;
            //}

            //if (Check2 != null)
            //{
            //    string orderNo = @"State = '4'";
            //    strWhere += " and " + orderNo;
            //}

            string strSear = @"select * from CheckPersonalHealth where ( State = '2' or State = '4')  ";
            string strWhereOrder = @" order by corID desc; ";

            string strQuery = strSear + strWhere + strWhereOrder;

            var modelSear = new List<HealthModels>();
            //if (page != 1)
            //{ if (Session["codeSe"] == null) { } else strQuery = Session["codeSe"].ToString(); }
            if (Session["codeSe"] == null) { } else strQuery = Session["codeSe"].ToString();
            modelSear = GetSearchModels(strQuery);
            //ViewBag.SearchQue = strQuery;
            //Session["codeSe"] = strQuery;



            return View(modelSear.ToPagedList(pageNumber, pageSize));
        }

        [HttpPost]
        public ActionResult Search(int? page, string Mathe, string Name, string Date, string BU,string Check1,string Check2,string searchInfor)
        {

            if (page == null) page = 1;
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            string strWhere = "";
            if (!string.IsNullOrEmpty(Mathe))
            {
                string orderNo = @"EmpNo like '%" + Mathe + "%'";
                if (strWhere.Equals("")) strWhere += " and " + orderNo;
                else strWhere += " and " + orderNo;
            }

            if (!string.IsNullOrEmpty(Name))
            {
                string orderNo = @"EmpName like '%" + Name + "%'";
                if (strWhere.Equals("")) strWhere += " and " + orderNo;
                else strWhere += " and " + orderNo;
            }


            if (!string.IsNullOrEmpty(Date))
            {
                DateTime dateC = Convert.ToDateTime(Date);
                string dateChe = dateC.ToString("yyyy/MM/dd");
                string orderNo = @"Date like '%" + dateChe + "%'";
                if (strWhere.Equals("")) strWhere += " and " + orderNo;
                else strWhere += " and " + orderNo;
            }

            if (!string.IsNullOrEmpty(BU))
            {
                string orderNo = @"Dept like '%" + BU + "%'";
                if (strWhere.Equals("")) strWhere += " and " + orderNo;
                else strWhere += " and " + orderNo;
            }
            if(Check1 != null)
            {
                string orderNo = @"State = '2'";
                strWhere += " and " + orderNo;
            }

            if (Check2 != null)
            {
                string orderNo = @"State = '4'";
                strWhere += " and " + orderNo;
            }

            string strSear = @"select * from CheckPersonalHealth where ( State = '2' or State = '4')  ";
            string strWhereOrder = @" order by corID desc; ";

            string strQuery = strSear + strWhere + strWhereOrder;

            var modelSear = new List<HealthModels>();
            if (page != 1)
            { if (Session["codeSe"] == null) { } else strQuery = Session["codeSe"].ToString(); }
                modelSear = GetSearchModels(strQuery);
            ViewBag.SearchQue = strQuery;
            Session["codeSe"] = strQuery;
            


            return View(modelSear.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string UserName, string Password)
        {
            var userSession = new UserLogin();
            using (SqlConnection conn = new SqlConnection(@"Data Source=10.224.81.131,3000;Initial Catalog=WechatDB;Persist Security Info=True;User ID=khangbeoit;Password=foxconn168!!"))
            {
                if(string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(Password)) ViewBag.Massa = "Tài khoản và mật khẩu không được để trống";
                else
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    DataTable tb = new DataTable();
                    string sqlQuery = @"select * from Account where EmpNo='" + UserName.Trim() + "' and PassWord = '" + Password.Trim() + "' ;";
                    SqlDataAdapter da = new SqlDataAdapter(sqlQuery, conn);
                    da.Fill(tb);

                    if (tb.Rows.Count > 0)
                    {
                        userSession.UserID = UserName.Trim();
                        userSession.UserName = Password.Trim();

                        Session.Add(CommonConstants.USER_SESSION, userSession);
                        if (conn.State != ConnectionState.Closed) conn.Close();
                        return RedirectToAction("InsertExcel", "CheckHealthy");

                    }
                    ViewBag.Massa = "Sai mật khẩu hoặc tài khoản";
                }
  
                return View();                
            }

               
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

        public async System.Threading.Tasks.Task<List<Location>> PostDataAsync(string civetno, string start_date, string end_date)
        {
            string uri = "http://icivetapps.foxconn.com/SaaS/Attendance/000017/Interface/GetAttendanceResult";
            var formContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("key", "CNSBG_Training"),
                    new KeyValuePair<string, string>("start_date",start_date),
                    new KeyValuePair<string, string>("end_date", end_date),
                    new KeyValuePair<string, string>("civetno", civetno),
                    new KeyValuePair<string, string>("pageId", "1"),
                    new KeyValuePair<string, string>("pageSize", "10000"),
                    new KeyValuePair<string, string>("status", "-1")
                });

            var myHttpClient = new HttpClient();
            myHttpClient.Timeout = TimeSpan.FromSeconds(30);
            using (var response = await myHttpClient.PostAsync(uri.ToString(), formContent))
            {
                if (response.IsSuccessStatusCode && !string.IsNullOrEmpty(response.Content.ToString()))
                {
                    var stringContent = await response.Content.ReadAsStringAsync();
                    var locationData = Newtonsoft.Json.JsonConvert.DeserializeObject<LocationData>(stringContent);
                    Console.WriteLine(stringContent);
                    Console.WriteLine(locationData.message);
                    return locationData.data;
                }

                return null;
            }

        }

        private static bool ValidationCertificate(Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            //trust any certificate!!!	
            System.Console.WriteLine("Warning, trust any certificate");
            return true;

        }

        public InforUser getInforUser(string code)
        {
            string url = String.Format("http://civetinterface.foxconn.com/open/get_user_info_bycode?code={0}&appid={1}",
            code,
            "2XA5CKB9d.hjpf1qNhznJA2");

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.Timeout = 5000;

            InforUser inf = new InforUser();
            string responseJson = null;
            try
            {
                using (var response = request.GetResponse())
                {
                    using (var sr = new StreamReader(response.GetResponseStream()))
                    {
                        responseJson = sr.ReadToEnd();
                        var locationData = Newtonsoft.Json.JsonConvert.DeserializeObject<InforUser>(responseJson);
                        return locationData;
                    }
                }
            }
            catch (WebException)
            {
                FormsAuthentication.RedirectToLoginPage();
            }
            return null;
        }

        private List<HealthModels> GetSearchModels(string strQuery)
        {
            var listHealth = new List<HealthModels>();
            using (SqlConnection conn = new SqlConnection(@"Data Source=10.224.81.131,3000;Initial Catalog=WechatDB;Persist Security Info=True;User ID=khangbeoit;Password=foxconn168!!"))
            {
                if (conn.State != ConnectionState.Open) conn.Open();
                SqlDataAdapter da = new SqlDataAdapter(strQuery, conn);
                DataTable tb = new DataTable();
                da.Fill(tb);
                if (tb.Rows.Count > 0)
                {

                    for (int i = 0; i < tb.Rows.Count; i++)
                    {
                        listHealth.Add(new HealthModels
                        {
                            empNo = tb.Rows[i]["EmpNo"].ToString(),
                            empName = tb.Rows[i]["EmpName"].ToString(),
                            Nhiet1 = tb.Rows[i]["Temperature1"].ToString(),
                            dauhieu = tb.Rows[i]["Description"].ToString(),
                            location = tb.Rows[i]["Location"].ToString(),
                            cars = tb.Rows[i]["Date"].ToString(),
                            tgbatdau = tb.Rows[i]["DateStart"].ToString(),
                            tgketthuc = tb.Rows[i]["DateEnd"].ToString(),
                            COntent = tb.Rows[i]["CorContent"].ToString(),
                            check1 = tb.Rows[i]["kinhTuyen"].ToString(),
                            check2 = tb.Rows[i]["viTuyen"].ToString(),
                        });
                    }
                }




                if (conn.State != ConnectionState.Closed) conn.Close();
            }

            return listHealth;
        }

        public ActionResult Result()
        {
            return View();
        }
        public ActionResult DownloadFile()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "Download/";
            byte[] fileBytes = System.IO.File.ReadAllBytes(path + "Employ.xlsx");
            string fileName = "Employ.xlsx";
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }
    }
}
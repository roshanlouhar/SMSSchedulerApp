using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

public partial class contactpersons : System.Web.UI.Page
{
    common Ocommon = new common();

    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["userid"] == null)
        {
            Response.Redirect(Page.ResolveUrl("~/login.aspx"));
        }
        if (!Page.IsPostBack)
        {
            if (Request.QueryString["deviceid"] != null)
            {
                Bind_Contact_Person_Information(Convert.ToInt64(Ocommon.Decrypt(Request.QueryString["deviceid"].ToString(), true)));
                Bind_SMS_Alert_Data(Convert.ToInt64(Ocommon.Decrypt(Request.QueryString["deviceid"].ToString(), true)));
            }
            HtmlGenericControl hPageTitle = (HtmlGenericControl)this.Page.Master.FindControl("hPageTitle");
            hPageTitle.InnerText = "Contact Person";
        }

    }

    private void Bind_SMS_Alert_Data(Int64 deviceID)
    {
        txtDeviceId.Text = Ocommon.Decrypt(Request.QueryString["deviceid"].ToString(), true);
        txtZone.Text = Ocommon.Decrypt(Request.QueryString["zone"].ToString(), true);
        txtAreaLocation.Text = Ocommon.Decrypt(Request.QueryString["area"].ToString(), true);
        txtStatus.Text = Ocommon.Decrypt(Request.QueryString["status"].ToString(), true);

        MySqlConnection con = new MySqlConnection("Server=115.124.109.110;Port=3306;Database=slc;Uid=slcuser;Pwd=Sc@12;Convert Zero Datetime=True;");
        DataTable dt1 = new DataTable();
        con.Open();
        MySqlCommand cmd = new MySqlCommand("Select mobile from cp where deviceid=" + deviceID + " order by mobile LIMIT 1");
        MySqlDataAdapter sda = new MySqlDataAdapter();
        cmd.Connection = con;
        sda.SelectCommand = cmd;
        sda.Fill(dt1);
        if (dt1 != null)
        {
            if (dt1.Rows.Count > 0)
            {
                txtSendMobile.Text = dt1.Rows[0]["mobile"].ToString();
            }
        }
        con.Close();
    }

    private void Bind_Contact_Person_Information(Int64 deviceID)
    {
        MySqlConnection con = new MySqlConnection("Server=115.124.109.110;Port=3306;Database=slc;Uid=slcuser;Pwd=Sc@12;Convert Zero Datetime=True;");
        DataTable dt1 = new DataTable();

        con.Open();
        MySqlCommand cmd = new MySqlCommand("Select * from cp where deviceid=" + deviceID);
        MySqlDataAdapter sda = new MySqlDataAdapter();
        cmd.Connection = con;
        sda.SelectCommand = cmd;
        sda.Fill(dt1);
        if (dt1 != null)
        {
            if (dt1.Rows.Count > 0)
            {
                gvContactPerson.DataSource = dt1;
                gvContactPerson.DataBind();
            }
        }
        con.Close();
    }

    protected void gvContactPerson_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            if ((e.Row.RowState == DataControlRowState.Normal) || (e.Row.RowState == DataControlRowState.Alternate))
            { }
        }
    }

    protected void btnAddNewAddress_Click(object sender, EventArgs e)
    {
        MySqlConnection con = new MySqlConnection("Server=115.124.109.110;Port=3306;Database=slc;Uid=slcuser;Pwd=Sc@12;Convert Zero Datetime=True;");
        DataTable dt1 = new DataTable();
        con.Open();
        MySqlCommand cmd = new MySqlCommand("Select * from cp where deviceid=" + Convert.ToInt64(Convert.ToInt64(Ocommon.Decrypt(Request.QueryString["deviceid"].ToString(), true))) + "  and mobile=" + txtMobile.Text.Trim());
        MySqlDataAdapter sda = new MySqlDataAdapter();
        cmd.Connection = con;
        sda.SelectCommand = cmd;
        sda.Fill(dt1);
        if (dt1 != null)
        {
            if (dt1.Rows.Count > 0)
            {
                spnMessage.Visible = true;
                spnMessage.Style.Add("color", "red");
                spnMessage.InnerHtml = "Mobile Number already exists!";
            }
            else
            {
                Insert_Phone();
            }
        }
        else
        {
            Insert_Phone();
        }
    }

    public void Insert_Phone()
    {
        MySqlConnection con = new MySqlConnection("Server=115.124.109.110;Port=3306;Database=slc;Uid=slcuser;Pwd=Sc@12;Convert Zero Datetime=True;");
        con.Close();
        string query = "INSERT into slc.cp (deviceid,name,address,mobile) values (" + Convert.ToInt64(Convert.ToInt64(Ocommon.Decrypt(Request.QueryString["deviceid"].ToString(), true))) + ",'" + txtName.Text.Trim() + "','" + txtAddress.Text.Trim() + "','" + txtMobile.Text.Trim() + "');";
        MySqlCommand cmd = new MySqlCommand(query, con);
        con.Open();
        spnMsg.Visible = false;
        spnMessage.Visible = true;
        spnMessage.Style.Add("color", "green");
        try
        {
            cmd.ExecuteNonQuery();
            spnMessage.InnerHtml = "Contact Person Added Successfully!";
            clearAll();
            Bind_Contact_Person_Information(Convert.ToInt64(Convert.ToInt64(Ocommon.Decrypt(Request.QueryString["deviceid"].ToString(), true))));
        }
        catch (Exception ex)
        {
            clearAll();
            spnMessage.InnerHtml = ex.Message;
        }
        finally
        {
            con.Close();
        }
    }

    public void clearAll()
    {
        txtName.Text = string.Empty;
        txtMobile.Text = string.Empty;
        txtAddress.Text = string.Empty;
    }

    protected void gvContactPerson_RowDeleting(object sender, GridViewDeleteEventArgs e)
    {
        Int32 cpid = Convert.ToInt32(gvContactPerson.DataKeys[e.RowIndex]["cpid"]);
        Int32 deviceid = Convert.ToInt32(gvContactPerson.DataKeys[e.RowIndex]["deviceid"]);

        MySqlConnection con = new MySqlConnection("Server=115.124.109.110;Port=3306;Database=slc;Uid=slcuser;Pwd=Sc@12;Convert Zero Datetime=True;");
        con.Close();
        string query = "Delete From cp where cpid=" + Convert.ToInt32(gvContactPerson.DataKeys[e.RowIndex]["cpid"]) + " and deviceid=" + Convert.ToInt32(gvContactPerson.DataKeys[e.RowIndex]["deviceid"]) + " ";
        MySqlCommand cmd = new MySqlCommand(query, con);
        con.Open();
        spnMessage.Visible = false;
        spnMsg.Visible = true;
        spnMsg.Style.Add("color", "green");
        try
        {
            cmd.ExecuteNonQuery();
            spnMsg.InnerHtml = "Contact Person Deleted Successfully!";
            Bind_Contact_Person_Information(Convert.ToInt64(Ocommon.Decrypt(Request.QueryString["deviceid"].ToString(), true)));
        }
        catch (Exception ex)
        {
            clearAll();
            spnMsg.InnerHtml = ex.Message;
        }
        finally
        {
            con.Close();
        }
    }

    //protected void btnSendSMS_Click(object sender, EventArgs e)
    //{
    //    //string Msg = "Device id :6025  zone 1 ward 1,  Address :Arilova Last Bus Sto-SS-88ABCSivaji Nagar Status: Power Failure";

    //    //Status List  -  Over Load , Over Voltage , Under Load , Over Frequency , Under Frequency , Over Temperature, 

    //    string Status = txtStatus.Text.Trim().ToLower();
    //    spnSMSSend.Visible = true;

    //    if (Session["temp-city"].ToString() == "1")
    //    {
    //        if ((Status == "Power Failure".ToLower() || Status == "Under Voltage".ToLower()))
    //        {
    //            string Msg = "Device id :" + txtDeviceId.Text.Trim() + "   Address :" + txtAreaLocation.Text.Trim() + "   Status :" + txtStatus.Text.Trim() + "   Zone :" + txtZone.Text.Trim();
    //            string OPTINS = "ESMART";
    //            string password = "esmart";
    //            string MobileNumber = txtSendMobile.Text.Trim();  //"9021191362"; 
    //            string strUrl = "http://bulksmsgateway.in/sendmessage.php?user=esmart&password=" + password + "&message=" + Msg + "&sender=" + OPTINS + "&mobile=" + MobileNumber + "&type=" + 3;
    //            System.Net.WebRequest request = System.Net.WebRequest.Create(strUrl);
    //            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
    //            Stream s = (Stream)response.GetResponseStream();
    //            StreamReader readStream = new StreamReader(s);
    //            string dataString = readStream.ReadToEnd();
    //            response.Close();
    //            s.Close();
    //            readStream.Close();
    //            spnSMSSend.InnerText = "SMS Send Successfully";
    //        }
    //        else
    //        {
    //            spnSMSSend.InnerText = "Sorry you have no authority send message.";
    //        }
    //    }
    //    else
    //    {
    //        spnSMSSend.InnerText = "Sorry you have no authority send message.";
    //    }
    //}

    protected void btnSendSMS_Click(object sender, EventArgs e)
    {
        string Status = txtStatus.Text.Trim().ToLower();
        spnSMSSend.Visible = true;
        bool send = StatusList(txtStatus.Text.Trim().ToLower());
        if (send)
        {
            string Msg = "Device Id - " + txtDeviceId.Text.Trim() + "  Address - " + txtAreaLocation.Text.Trim() + "   Status - " + txtStatus.Text.Trim() + "   Zone - " + txtZone.Text.Trim();
            string OPTINS = "SLCSMS";
            string username = "29prash";
            string password = "nn1234";
            string baseURL = "http://quicksms.in/sendsms.asp";           
            string MobileNumber = txtSendMobile.Text.Trim();  //"7588518728";	

            string strUrl = baseURL + "?user=" + username + "&password=" + password + "&PhoneNumber=" + MobileNumber + "&sender=" + OPTINS + "&Text=" + System.Web.HttpUtility.UrlEncode(Msg) + "";

            System.Net.WebRequest request = System.Net.WebRequest.Create(strUrl);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream s = (Stream)response.GetResponseStream();
            StreamReader readStream = new StreamReader(s);
            string dataString = readStream.ReadToEnd();
            response.Close();
            s.Close();
            readStream.Close();
            spnSMSSend.InnerText = "SMS Send Successfully";
        }
        else
        {
            spnSMSSend.InnerText = "Sorry you have no authority send message.";
        }
    }

    private bool StatusList(string status)
    {
        bool send = false;
        switch (status.Trim().ToLower())
        {
            case "Over Voltage":
                send = true;
                break;
            case "Under Voltage":
                send = true;
                break;
            case "Over Load":
                send = true;
                break;
            case "Power Failure":
                send = true;
                break;
            case "Group Failure":
                send = true;
                break;
            case "Power Down":
                send = true;
                break;
            case "Power Up":
                send = true;
                break;
            case "Power Fail":
                send = true;
                break;
            case "NV Fault":
                send = true;
                break;
            case "R I/P MCB Trip":
                send = true;
                break;
            case "Y I/P MCB Trip":
                send = true;
                break;
            case "B I/P MCB Trip":
                send = true;
                break;
            case "R O/P MCP Trip":
                send = true;
                break;
            case "Y O/P MCP Trip":
                send = true;
                break;
            case "B O/P MCP Trip":
                send = true;
                break;
            case "Contactor Failure":
                send = true;
                break;
            case "Panel Door Open":
                send = true;
                break;
            case "IF Fault":
                send = true;
                break;
            default:
                send = true;
                break;
        }

        return send;
    }
}
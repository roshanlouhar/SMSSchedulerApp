using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using System.Net.Http;
using MsgSchedulerApp.EMMS;

namespace MsgSchedulerApp
{
    public class SMSUtilities
    {
        private DBConnect context;
        string user { get; set; }

        string password { get; set; }

        string sender = "SLCSMS";

        string baseurl { get; set; }

        public SMSUtilities()
        {
            context = new DBConnect();
        }

        public bool processMessage(string cityId, string mobileNumber, string message)
        {
            bool result = false;
            try
            {
                var query = "select sg.* from smsauthority sa inner join SMSGateway sg on sa.smsGatewayID =sg.SMSGatewayID where sa.cityid = " + cityId + ";";
                DataTable dtInfo = context.Select(query);
                if (dtInfo != null && dtInfo.Rows.Count > 0)
                {
                    user = Convert.ToString(dtInfo.Rows[0]["SMSGatewayUserid"]);
                    password = Convert.ToString(dtInfo.Rows[0]["SMSGatewayPwd"]);
                    baseurl = Convert.ToString(dtInfo.Rows[0]["SMSGatewayURL"]);
                }

                if (cityId == "9" || cityId == "15")
                {
                    result = SendSMSIndia(mobileNumber, message);
                }
                else
                {
                    result = SendSMSSriLanka(mobileNumber, message);
                }
                return result;
            }
            catch (Exception ex)
            {
                return result;
            }
        }

        public bool SendSMSIndia(string mobileNumber, string message)
        {
            try
            {
                string baseURL = baseurl + "?user=" + user + "&password=" + password + "&PhoneNumber=" + mobileNumber + "&sender=" + sender + "&Text=" + message + "";

                using (WebClient client = new WebClient())
                {
                    string s = client.DownloadString(baseURL);

                    if (s.Contains("Submitted"))
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception exp)
            {
                return false;
            }
        }


        public bool SendSMSSriLanka(string mobileNumber, string message)
        {
            try
            {
                //string baseURL = baseurl + "?user=" + user + "&password=" + password + "&PhoneNumber=" + mobileNumber + "&sender=" + sender + "&Text=" + message + "";


                EMMS.user objuser = new EMMS.user();
                EMMS.alias alias = new EMMS.alias();
                objuser.username = "esmsusr_1bki";
                objuser.password = "2gi6tip";
                alias.alias1 = "APL_ENERGY";

                smsMessage msg = new smsMessage();
                msg.sender = alias;
                msg.message = message;
                msg.recipients = new string[] { mobileNumber };

                EnterpriseSMSWSClient client = new EnterpriseSMSWSClient();
               
                session s = client.createSession(objuser);
                int result = client.sendMessages(s, msg);
                client.closeSession(s);

                if (result == 200)
                {
                    return true;
                }
                else
                {
                    return false;
                }                
            }
            catch (Exception exp)
            {
                return false;
            }
        }

    }
}

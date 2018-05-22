using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MsgSchedulerApp
{
    public class DatabaseUtilities
    {
        public string ClientApi { get; set; }

        public bool SendSMS(string mobileNumber, string message)
        {
            try
            {
                WebClient client = new WebClient();
                string baseURL = ClientApi + "id=3528011&to='" + mobileNumber + "'&text='" + message + "'";
                client.OpenRead(baseURL);
                return true;
            }
            catch (Exception exp)
            {
                return false;
            }
        }

        public string GetClientAPI()
        {
            try
            {
                string ClientApi = string.Empty;
                return ClientApi;
            }
            catch
            {
                return null;
            }
        }

    }
}

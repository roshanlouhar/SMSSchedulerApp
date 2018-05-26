using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESMSClient.ESMSWS;



namespace ESMSClient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            ESMSWS.user user = new ESMSWS.user();
            ESMSWS.alias alias = new ESMSWS.alias();
            user.username = "esmsusr_1bki";
            user.password = "2gi6tip";
            alias.alias1 = "SLCMS";
            smsMessage msg = new smsMessage();
            msg.sender = alias;
            msg.message = body.Text;
            msg.recipients = new string[] { recipient.Text};


            EnterpriseSMSWSClient client = new EnterpriseSMSWSClient();
            
            session s=client.createSession(user);
          int test =  client.sendMessages(s, msg);
          //  smsMessage[] deliveryReports=client.getDeliveryReports(s, alias);
            //if(deliveryReports==null)Console.WriteLine("NULL");
           // for (int i = 0; i < deliveryReports.GetLength(1); i++)
           // {

//                System.Diagnostics.Debug.WriteLine(deliveryReports[i].message);
  //          }  

        }
    }
}

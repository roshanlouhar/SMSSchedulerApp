using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MsgSchedulerApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            StartApplication();
        }

        private void TxtStatusHistory_TextChanged(object sender, EventArgs e)
        {

        }

        private void BtnStartManual_Click(object sender, EventArgs e)
        {
            StartApplication();
        }

        public void StartApplication()
        {
            TxtStatusHistory.Text = "SMS Engine Started…";
        }

        public void SetUpDatabaseConnection()
        {
            try
            {
                TxtStatusHistory.Text = Environment.NewLine + "Establishing Database Connections...";
            }
            catch (Exception ex)
            {

            }
        }

        public void ReadingFaultInformation()
        {
            try
            {
                TxtStatusHistory.Text = Environment.NewLine + " Reading Device Fault Information...";
            }
            catch (Exception ex)
            {

            }
        }

        public void StartDeviceProcessing()
        {
            try
            {
                TxtStatusHistory.Text = Environment.NewLine + " START Device Processing";
                TxtStatusHistory.Text = Environment.NewLine + "  Device ID 3667……….";
                TxtStatusHistory.Text = Environment.NewLine + " Reading SMS Service Enability for this Device……….";
                TxtStatusHistory.Text = Environment.NewLine + " SMS Service NOT Enabled for this Device……….";
                TxtStatusHistory.Text = Environment.NewLine + " Processing the Data……….";
                TxtStatusHistory.Text = Environment.NewLine + " END Device Processing";


                TxtStatusHistory.Text = Environment.NewLine + " START Device Processing";
                TxtStatusHistory.Text = Environment.NewLine + " Device ID 3668……….";
                TxtStatusHistory.Text = Environment.NewLine + " Reading SMS Service Enability for this Device……….";
                TxtStatusHistory.Text = Environment.NewLine + " SMS Service Enabled for this Device……….";
                TxtStatusHistory.Text = Environment.NewLine + " Preparing SMS……….";
                TxtStatusHistory.Text = Environment.NewLine + " Sending SMS to the Users of the Device……….";
                TxtStatusHistory.Text = Environment.NewLine + " SMS sent Successfully to all Users……….";
                TxtStatusHistory.Text = Environment.NewLine + " Logging the SMS Data……….";


                Processing the Data……….
                END Device Processing


            }
            catch (Exception ex)
            {

            }
        }


    }
}

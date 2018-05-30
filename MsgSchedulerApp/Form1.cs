using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MsgSchedulerApp
{
    public partial class Form1 : Form
    {
        private DBConnect context;
        private SMSUtilities smsUtilities;
        public Form1()
        {
            InitializeComponent();
            context = new DBConnect();
            smsUtilities = new SMSUtilities();
            StartApplication();
        }

        #region control events of page.
        private void TxtStatusHistory_TextChanged(object sender, EventArgs e)
        {

        }

        private void BtnStartManual_Click(object sender, EventArgs e)
        {
            //timer1.Start();
            TxtStatusHistory.Clear();
            StartApplication();
        }

        private void btnclearLog_Click(object sender, EventArgs e)
        {
            TxtStatusHistory.Clear();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            TxtStatusHistory.Clear();
            context.CloseConnection();
            StartApplication();
        }

        #endregion

        #region device processing methods 
        public void StartApplication()
        {
            //smsUtilities.processMessage("15", "8003644328", "test");
            TxtStatusHistory.Text += " SMS Engine Started…";
            SetUpDatabaseConnection();
        }

        public void SetUpDatabaseConnection()
        {
            try
            {
                context = new DBConnect();
                if (context.OpenConnection())
                {
                    TxtStatusHistory.Text += Environment.NewLine + " Successfully establishing Database Connections...";
                    ReadingFaultInformation();
                    ExitSMSApp();
                }
                else
                {
                    TxtStatusHistory.Text += Environment.NewLine + " Error while establishing Database Connections...";
                    ExitSMSApp();
                }

            }
            catch (Exception ex)
            {
                TxtStatusHistory.Text += Environment.NewLine + " Error while establishing Database Connections...";
                ExitSMSApp();
            }
        }

        public void ReadingFaultInformation()
        {
            try
            {
                TxtStatusHistory.Text += Environment.NewLine + " Reading Device Fault Information..." + Environment.NewLine;
                string query = "select se.* from slcevents se inner join slc_devices sd on se.deviceid = sd.deviceid  where se.IsAlertProcessed = 0 ;";
                DataTable dtFaultEvent = context.Select(query);
                if (dtFaultEvent != null && dtFaultEvent.Rows.Count > 0)
                {
                    foreach (DataRow row in dtFaultEvent.Rows)
                    {
                        StartDeviceProcessing(row);
                    }
                }
                else
                {
                    TxtStatusHistory.Text += Environment.NewLine + " No fault information is found during time interval....";
                    ExitSMSApp();
                }
            }
            catch (Exception ex)
            {
                TxtStatusHistory.Text += Environment.NewLine + "Error while fetching fault information from database...";
                ExitSMSApp();
            }
        }

        public void StartDeviceProcessing(DataRow row)
        {
            try
            {
                TxtStatusHistory.Text += Environment.NewLine + " START Device Processing";
                TxtStatusHistory.Text += Environment.NewLine + " Device ID " + row["deviceid"];

                TxtStatusHistory.Text += Environment.NewLine + " Reading SMS Service Enability for this Device……….";
                string query = "select * from  slc_devices where deviceid =" + row["deviceid"] + " limit 1 ;";
                DataTable dtslcDevices = context.Select(query);
                if (dtslcDevices != null && dtslcDevices.Rows.Count > 0)
                {
                    string cityId = Convert.ToString(dtslcDevices.Rows[0]["city"]);
                    query = "select count(*) as Issend from smsauthority where cityid =" + cityId + " and issend = 1 ;";
                    int count = context.Count(query);
                    if (count > 0)
                    {
                        TxtStatusHistory.Text += Environment.NewLine + " SMS Service Enabled for this Device……….";

                        query = "select distinct  mobile from cp where deviceid =" + row["deviceid"] + "; ";
                        DataTable dtCPUser = context.Select(query);
                        if (dtCPUser != null && dtCPUser.Rows.Count > 0)
                        {
                            //string msgBody = PrepareSmsMessage(row, dtSlcFaultInfo, dtslcDevices);
                            var SelectedValues = dtCPUser.AsEnumerable().Select(s => s.Field<string>("mobile")).ToArray().Distinct();
                            string mobile = string.Join(",", SelectedValues);

                            TxtStatusHistory.Text += Environment.NewLine + " Sending SMS to the Users of the Device……….";
                            query = "select  * from slcevents where deviceid ='" + row["deviceid"] + "' and id < " + row["id"] + " order by dt desc limit 1 ; ";
                            DataTable dtPreviousRow = context.Select(query);
                            DataRow PreviousRow;
                            if (dtPreviousRow != null && dtPreviousRow.Rows.Count > 0)
                            {
                                PreviousRow = dtPreviousRow.Rows[0];
                                bool result = false;

                                string LEDMessage = PrepareLEDMessage(row, PreviousRow, dtslcDevices);
                                string FaultMessage = PrepareFaultMessage(row, PreviousRow, dtslcDevices);
                                if (!string.IsNullOrEmpty(LEDMessage))
                                {
                                    result = smsUtilities.processMessage(cityId, mobile, LEDMessage);
                                }
                                if (!string.IsNullOrEmpty(FaultMessage))
                                {
                                    result = smsUtilities.processMessage(cityId, mobile, FaultMessage);
                                }

                                if (result)
                                {
                                    TxtStatusHistory.Text += Environment.NewLine + " SMS sent Successfully to all Users……….";
                                    TxtStatusHistory.Text += Environment.NewLine + " Logging the SMS Data………." + Environment.NewLine;
                                    query = "INSERT INTO smsenthistory (deviceid,sentdate,senttime,statusid)VALUES('" + row["deviceid"] + "','" + DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + "','" + DateTime.Now.ToShortTimeString() + "', '1')";
                                    context.Insert(query);
                                    TxtStatusHistory.Text += Environment.NewLine + " END Device Processing" + Environment.NewLine + Environment.NewLine;
                                }
                                else
                                {
                                    TxtStatusHistory.Text += Environment.NewLine + " Error while sending sms to all Users………." + Environment.NewLine;
                                    TxtStatusHistory.Text += Environment.NewLine + " END Device Processing" + Environment.NewLine + Environment.NewLine;

                                    query = "INSERT INTO smsenthistory (deviceid,sentdate,senttime,statusid) VALUES ('" + row["deviceid"] + "','" + DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + "','" + DateTime.Now.ToShortTimeString() + "', '1')";
                                    context.Insert(query);
                                }
                                query = "update slcevents set IsAlertProcessed = '1' where id =" + row["id"] + "; ";
                                context.Update(query);
                            }
                            else
                            {
                                TxtStatusHistory.Text += Environment.NewLine + " Previous record not found for the devices..." + Environment.NewLine;
                                TxtStatusHistory.Text += " END Device Processing" + Environment.NewLine + Environment.NewLine;
                            }
                        }
                        else
                        {
                            TxtStatusHistory.Text += Environment.NewLine + " User is not available for this device " + row["deviceid"] + Environment.NewLine;
                            TxtStatusHistory.Text += " END Device Processing" + Environment.NewLine + Environment.NewLine;
                        }
                    }
                    else
                    {
                        TxtStatusHistory.Text += " SMS Service NOT Enabled for this Device………." + Environment.NewLine;
                        TxtStatusHistory.Text += " END Device Processing" + Environment.NewLine + Environment.NewLine;
                    }
                }
                else
                {
                    TxtStatusHistory.Text += Environment.NewLine + " Slc Devices information not found in database..." + Environment.NewLine;
                    TxtStatusHistory.Text += " END Device Processing" + Environment.NewLine + Environment.NewLine;
                }
            }
            catch (Exception ex)
            {
                TxtStatusHistory.Text += Environment.NewLine + " Error while processing data………." + Environment.NewLine;
                TxtStatusHistory.Text += " END Device Processing" + Environment.NewLine + Environment.NewLine;
            }
        }

        public string PrepareLEDMessage(DataRow CurrentRowfault, DataRow PrevoiusRowfault, DataTable dtslcDevices)
        {
            string message = string.Empty;
            DataRow rowSlcDevices = dtslcDevices.Rows[0];

            string CommonContentmessage = "STREET LIGHT ALERT " + Environment.NewLine;
            try
            {
                string SLCFaultDescription, SLCFaultSeverity = null;
                string CurrentEventid_ = Convert.ToString(CurrentRowfault["id"]),
                    Current_slcid_ = Convert.ToString(CurrentRowfault["slcid"])
                    , Currentdeviceid = Convert.ToString(CurrentRowfault["deviceid"])
                    , Current_dt_ = Convert.ToString(CurrentRowfault["dt"])
                    , Current_aux1_ = Convert.ToString(CurrentRowfault["aux1"])
                    , Current_aux2_ = Convert.ToString(CurrentRowfault["aux2"])
                    , Current_aux3_ = Convert.ToString(CurrentRowfault["aux3"])
                    , Current_aux4_ = Convert.ToString(CurrentRowfault["aux4"])
                    , Current_aux5_ = Convert.ToString(CurrentRowfault["aux5"])
                    , Current_aux6_ = Convert.ToString(CurrentRowfault["aux6"])
                    , Current_aux7_ = Convert.ToString(CurrentRowfault["aux7"])
                    , Current_aux8_ = Convert.ToString(CurrentRowfault["aux8"])
                    , Current_r_failure_ = Convert.ToString(CurrentRowfault["r_failure"])
                    , Current_y_failure_ = Convert.ToString(CurrentRowfault["y_failure"])
                    , Current_b_failure_ = Convert.ToString(CurrentRowfault["b_failure"])
                    , Current_r_ = Convert.ToString(CurrentRowfault["r"])
                    , Current_y_ = Convert.ToString(CurrentRowfault["y"])
                    , Current_b_ = Convert.ToString(CurrentRowfault["b"])
                    , Current_fault_ov_ = Convert.ToString(CurrentRowfault["fault_ov"])
                    , Current_fault_uv_ = Convert.ToString(CurrentRowfault["fault_uv"])
                    , Current_fault_OL_ = Convert.ToString(CurrentRowfault["fault_OL"])
                    , Current_fault_UL_ = Convert.ToString(CurrentRowfault["fault_UL"])
                    , Current_fault_OF_ = Convert.ToString(CurrentRowfault["fault_OF"])
                    , Current_fault_UF_ = Convert.ToString(CurrentRowfault["fault_UF"])
                    , Current_fault_OT_ = Convert.ToString(CurrentRowfault["fault_OT"])
                    , Current_fault_GF_ = Convert.ToString(CurrentRowfault["fault_GF"])
                    , Current_fault_PD_ = Convert.ToString(CurrentRowfault["fault_PD"])
                    , Current_fault_PU_ = Convert.ToString(CurrentRowfault["fault_PU"])
                    , Current_fault_ZV_ = Convert.ToString(CurrentRowfault["fault_ZV"])
                    , Current_fault_NV_ = Convert.ToString(CurrentRowfault["fault_NV"])
                    , Current_n_ = Convert.ToString(CurrentRowfault["n"])
                    , Current_s1_ = Convert.ToString(CurrentRowfault["s1"])
                    , Current_s2_ = Convert.ToString(CurrentRowfault["s2"])
                    , Current_s3_ = Convert.ToString(CurrentRowfault["s3"])
                    , Current_s4_ = Convert.ToString(CurrentRowfault["s4"])
                    , Current_boost_ = Convert.ToString(CurrentRowfault["boost"]);

                string PrevoiusRowEventid_ = Convert.ToString(PrevoiusRowfault["id"]),
                   Prevoius_slcid_ = Convert.ToString(PrevoiusRowfault["slcid"])
                   , Prevoiusdeviceid = Convert.ToString(PrevoiusRowfault["deviceid"])
                   , Prevoius_dt_ = Convert.ToString(PrevoiusRowfault["dt"])
                   , Prevoius_aux1_ = Convert.ToString(PrevoiusRowfault["aux1"])
                   , Prevoius_aux2_ = Convert.ToString(PrevoiusRowfault["aux2"])
                   , Prevoius_aux3_ = Convert.ToString(PrevoiusRowfault["aux3"])
                   , Prevoius_aux4_ = Convert.ToString(PrevoiusRowfault["aux4"])
                   , Prevoius_aux5_ = Convert.ToString(PrevoiusRowfault["aux5"])
                   , Prevoius_aux6_ = Convert.ToString(PrevoiusRowfault["aux6"])
                   , Prevoius_aux7_ = Convert.ToString(PrevoiusRowfault["aux7"])
                   , Prevoius_aux8_ = Convert.ToString(PrevoiusRowfault["aux8"])
                   , Prevoius_r_failure_ = Convert.ToString(PrevoiusRowfault["r_failure"])
                   , Prevoius_y_failure_ = Convert.ToString(PrevoiusRowfault["y_failure"])
                   , Prevoius_b_failure_ = Convert.ToString(PrevoiusRowfault["b_failure"])
                   , Prevoius_r_ = Convert.ToString(PrevoiusRowfault["r"])
                   , Prevoius_y_ = Convert.ToString(PrevoiusRowfault["y"])
                   , Prevoius_b_ = Convert.ToString(PrevoiusRowfault["b"])
                   , Prevoius_fault_ov_ = Convert.ToString(PrevoiusRowfault["fault_ov"])
                   , Prevoius_fault_uv_ = Convert.ToString(PrevoiusRowfault["fault_uv"])
                   , Prevoius_fault_OL_ = Convert.ToString(PrevoiusRowfault["fault_OL"])
                   , Prevoius_fault_UL_ = Convert.ToString(PrevoiusRowfault["fault_UL"])
                   , Prevoius_fault_OF_ = Convert.ToString(PrevoiusRowfault["fault_OF"])
                   , Prevoius_fault_UF_ = Convert.ToString(PrevoiusRowfault["fault_UF"])
                   , Prevoius_fault_OT_ = Convert.ToString(PrevoiusRowfault["fault_OT"])
                   , Prevoius_fault_GF_ = Convert.ToString(PrevoiusRowfault["fault_GF"])
                   , Prevoius_fault_PD_ = Convert.ToString(PrevoiusRowfault["fault_PD"])
                   , Prevoius_fault_PU_ = Convert.ToString(PrevoiusRowfault["fault_PU"])
                   , Prevoius_fault_ZV_ = Convert.ToString(PrevoiusRowfault["fault_ZV"])
                   , Prevoius_fault_NV_ = Convert.ToString(PrevoiusRowfault["fault_NV"])
                   , Prevoius_n_ = Convert.ToString(PrevoiusRowfault["n"])
                   , Prevoius_s1_ = Convert.ToString(PrevoiusRowfault["s1"])
                   , Prevoius_s2_ = Convert.ToString(PrevoiusRowfault["s2"])
                   , Prevoius_s3_ = Convert.ToString(PrevoiusRowfault["s3"])
                   , Prevoius_s4_ = Convert.ToString(PrevoiusRowfault["s4"])
                   , Prevoius_boost_ = Convert.ToString(PrevoiusRowfault["boost"]);

                TxtStatusHistory.Text += Environment.NewLine + " Preparing SMS……….";

                if (Convert.ToString(rowSlcDevices["phase"]) == "1")
                {
                    if (Current_r_ != Prevoius_r_)
                    {
                        string query = " select sfv.description ,sf.SLCFaultSeverity from SLCFaults sf inner join SLCFaultsValues sfv on sf.SLCFaultid = sfv.SLCFaultid ";
                        query += " where sf.SLCFaultName = 'r' and sfv.value = '" + Current_r_ + "' ;";
                        DataTable dtSlcFaultInfo = context.Select(query);

                        if (dtSlcFaultInfo != null && dtSlcFaultInfo.Rows.Count > 0)
                        {
                            SLCFaultDescription = Convert.ToString(dtSlcFaultInfo.Rows[0]["description"]);
                            SLCFaultSeverity = Convert.ToString(dtSlcFaultInfo.Rows[0]["SLCFaultSeverity"]);
                            message += SLCFaultDescription + " :OK";
                        }
                    }
                }
                if (Convert.ToString(rowSlcDevices["phase"]) == "3")
                {
                    if ((Current_r_ != Prevoius_r_) || (Current_y_ != Prevoius_y_) || (Current_b_ != Prevoius_b_))
                    {
                        string query = " select sfv.description ,sf.SLCFaultSeverity from SLCFaults  sf inner join SLCFaultsValues sfv on sf.SLCFaultid = sfv.SLCFaultid ";
                        query += "where ((sf.SLCFaultName ='r' and sfv.value = '" + Current_r_ + "') or (sf.SLCFaultName ='y' and sfv.value = '" + Current_y_ + "') or (sf.SLCFaultName ='b' and sfv.value = '" + Current_b_ + "') )";
                        DataTable dtSlcFaultInfo = context.Select(query);

                        if (dtSlcFaultInfo != null && dtSlcFaultInfo.Rows.Count > 0)
                        {
                            SLCFaultDescription = Convert.ToString(dtSlcFaultInfo.Rows[0]["description"]);
                            SLCFaultSeverity = Convert.ToString(dtSlcFaultInfo.Rows[0]["SLCFaultSeverity"]);
                            message += SLCFaultDescription + " :OK";
                        }
                    }
                }
                if (!string.IsNullOrEmpty(message))
                {
                    string result = CommonContentmessage + message + Environment.NewLine;
                    result += CurrentRowfault["dt"] + Environment.NewLine + " Zone:" + rowSlcDevices["zone"] + Environment.NewLine + " Area:" + rowSlcDevices["area"];
                    result += "-" + rowSlcDevices["Location"] + Environment.NewLine + "UnitID:" + rowSlcDevices["deviceid"] + Environment.NewLine + rowSlcDevices["phase"] + "Phase ";
                    return result;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                TxtStatusHistory.Text += Environment.NewLine + " Error while preparing message body...";
                return null;
            }
        }

        public string PrepareFaultMessage(DataRow CurrentRowfault, DataRow PrevoiusRowfault, DataTable dtslcDevices)
        {
            DataRow rowSlcDevices = dtslcDevices.Rows[0];
            string DevicePhase = Convert.ToString(rowSlcDevices["phase"]);
            string CommonContentmessage = "STREET LIGHT ALERT " + Environment.NewLine, message = string.Empty;

            try
            {
                string SLCFaultDescription, SLCFaultSeverity = null;
                string CurrentEventid_ = Convert.ToString(CurrentRowfault["id"]),
                    Current_slcid_ = Convert.ToString(CurrentRowfault["slcid"])
                    , Currentdeviceid = Convert.ToString(CurrentRowfault["deviceid"])
                    , Current_dt_ = Convert.ToString(CurrentRowfault["dt"])
                    , Current_aux1_ = Convert.ToString(CurrentRowfault["aux1"])
                    , Current_aux2_ = Convert.ToString(CurrentRowfault["aux2"])
                    , Current_aux3_ = Convert.ToString(CurrentRowfault["aux3"])
                    , Current_aux4_ = Convert.ToString(CurrentRowfault["aux4"])
                    , Current_aux5_ = Convert.ToString(CurrentRowfault["aux5"])
                    , Current_aux6_ = Convert.ToString(CurrentRowfault["aux6"])
                    , Current_aux7_ = Convert.ToString(CurrentRowfault["aux7"])
                    , Current_aux8_ = Convert.ToString(CurrentRowfault["aux8"])
                    , Current_r_failure_ = Convert.ToString(CurrentRowfault["r_failure"])
                    , Current_y_failure_ = Convert.ToString(CurrentRowfault["y_failure"])
                    , Current_b_failure_ = Convert.ToString(CurrentRowfault["b_failure"])
                    , Current_r_ = Convert.ToString(CurrentRowfault["r"])
                    , Current_y_ = Convert.ToString(CurrentRowfault["y"])
                    , Current_b_ = Convert.ToString(CurrentRowfault["b"])
                    , Current_fault_ov_ = Convert.ToString(CurrentRowfault["fault_ov"])
                    , Current_fault_uv_ = Convert.ToString(CurrentRowfault["fault_uv"])
                    , Current_fault_OL_ = Convert.ToString(CurrentRowfault["fault_OL"])
                    , Current_fault_UL_ = Convert.ToString(CurrentRowfault["fault_UL"])
                    , Current_fault_OF_ = Convert.ToString(CurrentRowfault["fault_OF"])
                    , Current_fault_UF_ = Convert.ToString(CurrentRowfault["fault_UF"])
                    , Current_fault_OT_ = Convert.ToString(CurrentRowfault["fault_OT"])
                    , Current_fault_GF_ = Convert.ToString(CurrentRowfault["fault_GF"])
                    , Current_fault_PD_ = Convert.ToString(CurrentRowfault["fault_PD"])
                    , Current_fault_PU_ = Convert.ToString(CurrentRowfault["fault_PU"])
                    , Current_fault_ZV_ = Convert.ToString(CurrentRowfault["fault_ZV"])
                    , Current_fault_NV_ = Convert.ToString(CurrentRowfault["fault_NV"])
                    , Current_n_ = Convert.ToString(CurrentRowfault["n"])
                    , Current_s1_ = Convert.ToString(CurrentRowfault["s1"])
                    , Current_s2_ = Convert.ToString(CurrentRowfault["s2"])
                    , Current_s3_ = Convert.ToString(CurrentRowfault["s3"])
                    , Current_s4_ = Convert.ToString(CurrentRowfault["s4"])
                    , Current_boost_ = Convert.ToString(CurrentRowfault["boost"]);

                string PrevoiusRowEventid_ = Convert.ToString(PrevoiusRowfault["id"]),
                   Prevoius_slcid_ = Convert.ToString(PrevoiusRowfault["slcid"])
                   , Prevoiusdeviceid = Convert.ToString(PrevoiusRowfault["deviceid"])
                   , Prevoius_dt_ = Convert.ToString(PrevoiusRowfault["dt"])
                   , Prevoius_aux1_ = Convert.ToString(PrevoiusRowfault["aux1"])
                   , Prevoius_aux2_ = Convert.ToString(PrevoiusRowfault["aux2"])
                   , Prevoius_aux3_ = Convert.ToString(PrevoiusRowfault["aux3"])
                   , Prevoius_aux4_ = Convert.ToString(PrevoiusRowfault["aux4"])
                   , Prevoius_aux5_ = Convert.ToString(PrevoiusRowfault["aux5"])
                   , Prevoius_aux6_ = Convert.ToString(PrevoiusRowfault["aux6"])
                   , Prevoius_aux7_ = Convert.ToString(PrevoiusRowfault["aux7"])
                   , Prevoius_aux8_ = Convert.ToString(PrevoiusRowfault["aux8"])
                   , Prevoius_r_failure_ = Convert.ToString(PrevoiusRowfault["r_failure"])
                   , Prevoius_y_failure_ = Convert.ToString(PrevoiusRowfault["y_failure"])
                   , Prevoius_b_failure_ = Convert.ToString(PrevoiusRowfault["b_failure"])
                   , Prevoius_r_ = Convert.ToString(PrevoiusRowfault["r"])
                   , Prevoius_y_ = Convert.ToString(PrevoiusRowfault["y"])
                   , Prevoius_b_ = Convert.ToString(PrevoiusRowfault["b"])
                   , Prevoius_fault_ov_ = Convert.ToString(PrevoiusRowfault["fault_ov"])
                   , Prevoius_fault_uv_ = Convert.ToString(PrevoiusRowfault["fault_uv"])
                   , Prevoius_fault_OL_ = Convert.ToString(PrevoiusRowfault["fault_OL"])
                   , Prevoius_fault_UL_ = Convert.ToString(PrevoiusRowfault["fault_UL"])
                   , Prevoius_fault_OF_ = Convert.ToString(PrevoiusRowfault["fault_OF"])
                   , Prevoius_fault_UF_ = Convert.ToString(PrevoiusRowfault["fault_UF"])
                   , Prevoius_fault_OT_ = Convert.ToString(PrevoiusRowfault["fault_OT"])
                   , Prevoius_fault_GF_ = Convert.ToString(PrevoiusRowfault["fault_GF"])
                   , Prevoius_fault_PD_ = Convert.ToString(PrevoiusRowfault["fault_PD"])
                   , Prevoius_fault_PU_ = Convert.ToString(PrevoiusRowfault["fault_PU"])
                   , Prevoius_fault_ZV_ = Convert.ToString(PrevoiusRowfault["fault_ZV"])
                   , Prevoius_fault_NV_ = Convert.ToString(PrevoiusRowfault["fault_NV"])
                   , Prevoius_n_ = Convert.ToString(PrevoiusRowfault["n"])
                   , Prevoius_s1_ = Convert.ToString(PrevoiusRowfault["s1"])
                   , Prevoius_s2_ = Convert.ToString(PrevoiusRowfault["s2"])
                   , Prevoius_s3_ = Convert.ToString(PrevoiusRowfault["s3"])
                   , Prevoius_s4_ = Convert.ToString(PrevoiusRowfault["s4"])
                   , Prevoius_boost_ = Convert.ToString(PrevoiusRowfault["boost"]);

                TxtStatusHistory.Text += Environment.NewLine + " Preparing SMS……….";

                if (Current_aux1_ != Prevoius_aux1_)
                {
                    string query = " select sfv.description ,sf.SLCFaultSeverity from SLCFaults  sf inner join SLCFaultsValues sfv on sf.SLCFaultid = sfv.SLCFaultid ";
                    query += "where sf.SLCFaultName = 'aux1' and sfv.value = '" + Current_aux1_ + "' ;";
                    DataTable dtSlcFaultInfo = context.Select(query);
                    if (dtSlcFaultInfo != null && dtSlcFaultInfo.Rows.Count > 0)
                    {
                        SLCFaultDescription = Convert.ToString(dtSlcFaultInfo.Rows[0]["description"]);
                        SLCFaultSeverity = Convert.ToString(dtSlcFaultInfo.Rows[0]["SLCFaultSeverity"]);
                        message += "Severity:" + SLCFaultSeverity + Environment.NewLine + SLCFaultDescription + Environment.NewLine;
                    }
                }
                if (Current_aux2_ != Prevoius_aux2_)
                {
                    if (DevicePhase != "1")
                    {
                        string query = " select sfv.description ,sf.SLCFaultSeverity from SLCFaults  sf inner join SLCFaultsValues sfv on sf.SLCFaultid = sfv.SLCFaultid ";
                        query += "where sf.SLCFaultName = 'aux2' and sfv.value = '" + Current_aux2_ + "' ;";
                        DataTable dtSlcFaultInfo = context.Select(query);
                        if (dtSlcFaultInfo != null && dtSlcFaultInfo.Rows.Count > 0)
                        {
                            SLCFaultDescription = Convert.ToString(dtSlcFaultInfo.Rows[0]["description"]);
                            SLCFaultSeverity = Convert.ToString(dtSlcFaultInfo.Rows[0]["SLCFaultSeverity"]);
                            message += "Severity:" + SLCFaultSeverity + Environment.NewLine + SLCFaultDescription + Environment.NewLine;
                        }
                    }
                }
                if (Current_aux3_ != Prevoius_aux3_)
                {
                    if (DevicePhase != "1")
                    {
                        string query = " select sfv.description ,sf.SLCFaultSeverity from SLCFaults  sf inner join SLCFaultsValues sfv on sf.SLCFaultid = sfv.SLCFaultid ";
                        query += "where sf.SLCFaultName = 'aux3' and sfv.value = '" + Current_aux3_ + "' ;";
                        DataTable dtSlcFaultInfo = context.Select(query);
                        if (dtSlcFaultInfo != null && dtSlcFaultInfo.Rows.Count > 0)
                        {
                            SLCFaultDescription = Convert.ToString(dtSlcFaultInfo.Rows[0]["description"]);
                            SLCFaultSeverity = Convert.ToString(dtSlcFaultInfo.Rows[0]["SLCFaultSeverity"]);
                            message += "Severity:" + SLCFaultSeverity + Environment.NewLine + SLCFaultDescription + Environment.NewLine;
                        }
                    }
                }
                if (Current_aux4_ != Prevoius_aux4_)
                {
                    string query = " select sfv.description ,sf.SLCFaultSeverity from SLCFaults  sf inner join SLCFaultsValues sfv on sf.SLCFaultid = sfv.SLCFaultid ";
                    query += "where sf.SLCFaultName = 'aux4' and sfv.value = '" + Current_aux4_ + "' ;";
                    DataTable dtSlcFaultInfo = context.Select(query);
                    if (dtSlcFaultInfo != null && dtSlcFaultInfo.Rows.Count > 0)
                    {
                        SLCFaultDescription = Convert.ToString(dtSlcFaultInfo.Rows[0]["description"]);
                        SLCFaultSeverity = Convert.ToString(dtSlcFaultInfo.Rows[0]["SLCFaultSeverity"]);
                        message += "Severity:" + SLCFaultSeverity + Environment.NewLine + SLCFaultDescription + Environment.NewLine;
                    }
                }
                if (Current_aux5_ != Prevoius_aux5_)
                {
                    if (DevicePhase != "1")
                    {
                        string query = " select  sfv.description ,sf.SLCFaultSeverity from SLCFaults  sf inner join SLCFaultsValues sfv on sf.SLCFaultid = sfv.SLCFaultid ";
                        query += "where sf.SLCFaultName = 'aux5' and sfv.value = '" + Current_aux5_ + "' ;";
                        DataTable dtSlcFaultInfo = context.Select(query);
                        if (dtSlcFaultInfo != null && dtSlcFaultInfo.Rows.Count > 0)
                        {
                            SLCFaultDescription = Convert.ToString(dtSlcFaultInfo.Rows[0]["description"]);
                            SLCFaultSeverity = Convert.ToString(dtSlcFaultInfo.Rows[0]["SLCFaultSeverity"]);
                            message += "Severity:" + SLCFaultSeverity + Environment.NewLine + SLCFaultDescription + Environment.NewLine;
                        }
                    }
                }
                if (Current_aux6_ != Prevoius_aux6_)
                {
                    if (DevicePhase != "1")
                    {
                        string query = " select sfv.description ,sf.SLCFaultSeverity from SLCFaults  sf inner join SLCFaultsValues sfv on sf.SLCFaultid = sfv.SLCFaultid ";
                        query += "where sf.SLCFaultName = 'aux6' and sfv.value = '" + Current_aux6_ + "' ;";
                        DataTable dtSlcFaultInfo = context.Select(query);
                        if (dtSlcFaultInfo != null && dtSlcFaultInfo.Rows.Count > 0)
                        {
                            SLCFaultDescription = Convert.ToString(dtSlcFaultInfo.Rows[0]["description"]);
                            SLCFaultSeverity = Convert.ToString(dtSlcFaultInfo.Rows[0]["SLCFaultSeverity"]);
                            message += "Severity:" + SLCFaultSeverity + Environment.NewLine + SLCFaultDescription + Environment.NewLine;
                        }
                    }
                }
                if (Current_aux7_ != Prevoius_aux7_)
                {
                    string query = " select sfv.description ,sf.SLCFaultSeverity from SLCFaults  sf inner join SLCFaultsValues sfv on sf.SLCFaultid = sfv.SLCFaultid ";
                    query += "where sf.SLCFaultName = 'aux7' and sfv.value = '" + Current_aux7_ + "' ;";
                    DataTable dtSlcFaultInfo = context.Select(query);
                    if (dtSlcFaultInfo != null && dtSlcFaultInfo.Rows.Count > 0)
                    {
                        SLCFaultDescription = Convert.ToString(dtSlcFaultInfo.Rows[0]["description"]);
                        SLCFaultSeverity = Convert.ToString(dtSlcFaultInfo.Rows[0]["SLCFaultSeverity"]);
                        message += "Severity:" + SLCFaultSeverity + Environment.NewLine + SLCFaultDescription + Environment.NewLine;
                    }
                }
                if (Current_aux8_ != Prevoius_aux8_)
                {
                    string query = " select sfv.description ,sf.SLCFaultSeverity from SLCFaults  sf inner join SLCFaultsValues sfv on sf.SLCFaultid = sfv.SLCFaultid ";
                    query += "where sf.SLCFaultName = 'aux8' and sfv.value = '" + Current_aux8_ + "' ;";
                    DataTable dtSlcFaultInfo = context.Select(query);
                    if (dtSlcFaultInfo != null && dtSlcFaultInfo.Rows.Count > 0)
                    {
                        SLCFaultDescription = Convert.ToString(dtSlcFaultInfo.Rows[0]["description"]);
                        SLCFaultSeverity = Convert.ToString(dtSlcFaultInfo.Rows[0]["SLCFaultSeverity"]);
                        message += "Severity:" + SLCFaultSeverity + Environment.NewLine + SLCFaultDescription + Environment.NewLine;
                    }
                }
                if (Current_r_failure_ != Prevoius_r_failure_)
                {
                    string query = string.Empty;
                    if (Current_r_failure_ == "0")
                    {
                        query = " select sfv.description ,sf.SLCFaultSeverity from SLCFaults  sf inner join SLCFaultsValues sfv on sf.SLCFaultid = sfv.SLCFaultid ";
                        query += "where sf.SLCFaultName = 'r_failure ' and sfv.value = '" + Current_r_failure_ + "' ;";
                    }
                    else
                    {
                        query = " select sfv.description ,sf.SLCFaultSeverity from SLCFaults  sf inner join SLCFaultsValues sfv on sf.SLCFaultid = sfv.SLCFaultid ";
                        query += "where sf.SLCFaultName = 'r_failure ' and sfv.value = '>0' ;";
                    }
                    DataTable dtSlcFaultInfo = context.Select(query);
                    if (dtSlcFaultInfo != null && dtSlcFaultInfo.Rows.Count > 0)
                    {
                        SLCFaultDescription = Convert.ToString(dtSlcFaultInfo.Rows[0]["description"]);
                        SLCFaultSeverity = Convert.ToString(dtSlcFaultInfo.Rows[0]["SLCFaultSeverity"]);
                        message += "Severity:" + SLCFaultSeverity + Environment.NewLine + SLCFaultDescription + ":" + Current_r_failure_ + Environment.NewLine;
                    }
                }
                if (Current_y_failure_ != Prevoius_y_failure_)
                {
                    if (DevicePhase != "1")
                    {
                        string query = string.Empty;
                        if (Current_y_failure_ == "0")
                        {
                            query = " select sfv.description ,sf.SLCFaultSeverity from SLCFaults  sf inner join SLCFaultsValues sfv on sf.SLCFaultid = sfv.SLCFaultid ";
                            query += "where sf.SLCFaultName = 'y_failure ' and sfv.value = '" + Current_y_failure_ + "' ;";
                        }
                        else
                        {
                            query = " select sfv.description ,sf.SLCFaultSeverity from SLCFaults  sf inner join SLCFaultsValues sfv on sf.SLCFaultid = sfv.SLCFaultid ";
                            query += "where sf.SLCFaultName = 'y_failure ' and sfv.value = '>0' ;";
                        }
                        DataTable dtSlcFaultInfo = context.Select(query);
                        if (dtSlcFaultInfo != null && dtSlcFaultInfo.Rows.Count > 0)
                        {
                            SLCFaultDescription = Convert.ToString(dtSlcFaultInfo.Rows[0]["description"]);
                            SLCFaultSeverity = Convert.ToString(dtSlcFaultInfo.Rows[0]["SLCFaultSeverity"]);
                            message += "Severity:" + SLCFaultSeverity + Environment.NewLine + SLCFaultDescription + ":" + Current_y_failure_ + Environment.NewLine;
                        }
                    }
                }
                if (Current_b_failure_ != Prevoius_b_failure_)
                {
                    if (DevicePhase != "1")
                    {
                        string query = string.Empty;
                        if (Current_y_failure_ == "0")
                        {
                            query = " select sfv.description ,sf.SLCFaultSeverity from SLCFaults  sf inner join SLCFaultsValues sfv on sf.SLCFaultid = sfv.SLCFaultid ";
                            query += "where sf.SLCFaultName = 'b_failure ' and sfv.value = '" + Current_b_failure_ + "' ;";
                        }
                        else
                        {
                            query = " select sfv.description ,sf.SLCFaultSeverity from SLCFaults  sf inner join SLCFaultsValues sfv on sf.SLCFaultid = sfv.SLCFaultid ";
                            query += "where sf.SLCFaultName = 'b_failure ' and sfv.value = '>0' ;";
                        }
                        DataTable dtSlcFaultInfo = context.Select(query);
                        if (dtSlcFaultInfo != null && dtSlcFaultInfo.Rows.Count > 0)
                        {
                            SLCFaultDescription = Convert.ToString(dtSlcFaultInfo.Rows[0]["description"]);
                            SLCFaultSeverity = Convert.ToString(dtSlcFaultInfo.Rows[0]["SLCFaultSeverity"]);
                            message += "Severity:" + SLCFaultSeverity + Environment.NewLine + SLCFaultDescription + ":" + Environment.NewLine;
                        }
                    }
                }
                if (Current_fault_ov_ != Prevoius_fault_ov_)
                {
                    string query = " select sfv.description ,sf.SLCFaultSeverity from SLCFaults  sf inner join SLCFaultsValues sfv on sf.SLCFaultid = sfv.SLCFaultid ";
                    query += "where sf.SLCFaultName = 'fault_ov ' and sfv.value = '" + Current_fault_ov_ + "' ;";
                    DataTable dtSlcFaultInfo = context.Select(query);
                    if (dtSlcFaultInfo != null && dtSlcFaultInfo.Rows.Count > 0)
                    {
                        SLCFaultDescription = Convert.ToString(dtSlcFaultInfo.Rows[0]["description"]);
                        SLCFaultSeverity = Convert.ToString(dtSlcFaultInfo.Rows[0]["SLCFaultSeverity"]);
                        message += "Severity:" + SLCFaultSeverity + Environment.NewLine + SLCFaultDescription + Environment.NewLine;
                    }
                }
                if (Current_fault_uv_ != Prevoius_fault_uv_)
                {
                    string query = " select sfv.description ,sf.SLCFaultSeverity from SLCFaults  sf inner join SLCFaultsValues sfv on sf.SLCFaultid = sfv.SLCFaultid ";
                    query += "where sf.SLCFaultName = 'fault_uv ' and sfv.value = '" + Current_fault_uv_ + "' ;";
                    DataTable dtSlcFaultInfo = context.Select(query);
                    if (dtSlcFaultInfo != null && dtSlcFaultInfo.Rows.Count > 0)
                    {
                        SLCFaultDescription = Convert.ToString(dtSlcFaultInfo.Rows[0]["description"]);
                        SLCFaultSeverity = Convert.ToString(dtSlcFaultInfo.Rows[0]["SLCFaultSeverity"]);
                        message += "Severity:" + SLCFaultSeverity + Environment.NewLine + SLCFaultDescription + Environment.NewLine;
                    }
                }
                if (Current_fault_OL_ != Prevoius_fault_OL_)
                {
                    string query = " select sfv.description ,sf.SLCFaultSeverity from SLCFaults  sf inner join SLCFaultsValues sfv on sf.SLCFaultid = sfv.SLCFaultid ";
                    query += "where sf.SLCFaultName = 'fault_OL ' and sfv.value = '" + Current_fault_OL_ + "' ;";
                    DataTable dtSlcFaultInfo = context.Select(query);
                    if (dtSlcFaultInfo != null && dtSlcFaultInfo.Rows.Count > 0)
                    {
                        SLCFaultDescription = Convert.ToString(dtSlcFaultInfo.Rows[0]["description"]);
                        SLCFaultSeverity = Convert.ToString(dtSlcFaultInfo.Rows[0]["SLCFaultSeverity"]);
                        message += "Severity:" + SLCFaultSeverity + Environment.NewLine + SLCFaultDescription + Environment.NewLine;
                    }
                }
                if (Current_fault_UL_ != Prevoius_fault_UL_)
                {
                    string query = " select sfv.description ,sf.SLCFaultSeverity from SLCFaults  sf inner join SLCFaultsValues sfv on sf.SLCFaultid = sfv.SLCFaultid ";
                    query += "where sf.SLCFaultName = 'fault_UL ' and sfv.value = '" + Current_fault_UL_ + "' ;";
                    DataTable dtSlcFaultInfo = context.Select(query);
                    if (dtSlcFaultInfo != null && dtSlcFaultInfo.Rows.Count > 0)
                    {
                        SLCFaultDescription = Convert.ToString(dtSlcFaultInfo.Rows[0]["description"]);
                        SLCFaultSeverity = Convert.ToString(dtSlcFaultInfo.Rows[0]["SLCFaultSeverity"]);
                        message += "Severity:" + SLCFaultSeverity + Environment.NewLine + SLCFaultDescription + Environment.NewLine;
                    }
                }
                if (Current_fault_OF_ != Prevoius_fault_OF_)
                {
                    string query = " select sfv.description ,sf.SLCFaultSeverity from SLCFaults  sf inner join SLCFaultsValues sfv on sf.SLCFaultid = sfv.SLCFaultid ";
                    query += "where sf.SLCFaultName = 'fault_OF ' and sfv.value = '" + Current_fault_OF_ + "' ;";
                    DataTable dtSlcFaultInfo = context.Select(query);
                    if (dtSlcFaultInfo != null && dtSlcFaultInfo.Rows.Count > 0)
                    {
                        SLCFaultDescription = Convert.ToString(dtSlcFaultInfo.Rows[0]["description"]);
                        SLCFaultSeverity = Convert.ToString(dtSlcFaultInfo.Rows[0]["SLCFaultSeverity"]);
                        message += "Severity:" + SLCFaultSeverity + Environment.NewLine + SLCFaultDescription + Environment.NewLine;
                    }
                }
                if (Current_fault_UF_ != Prevoius_fault_UF_)
                {
                    string query = " select sfv.description ,sf.SLCFaultSeverity from SLCFaults  sf inner join SLCFaultsValues sfv on sf.SLCFaultid = sfv.SLCFaultid ";
                    query += "where sf.SLCFaultName = 'fault_UF ' and sfv.value = '" + Current_fault_UF_ + "' ;";
                    DataTable dtSlcFaultInfo = context.Select(query);
                    if (dtSlcFaultInfo != null && dtSlcFaultInfo.Rows.Count > 0)
                    {
                        SLCFaultDescription = Convert.ToString(dtSlcFaultInfo.Rows[0]["description"]);
                        SLCFaultSeverity = Convert.ToString(dtSlcFaultInfo.Rows[0]["SLCFaultSeverity"]);
                        message += "Severity:" + SLCFaultSeverity + Environment.NewLine + SLCFaultDescription + Environment.NewLine;
                    }
                }
                if (Current_fault_OT_ != Prevoius_fault_OT_)
                {
                    string query = " select sfv.description ,sf.SLCFaultSeverity from SLCFaults  sf inner join SLCFaultsValues sfv on sf.SLCFaultid = sfv.SLCFaultid ";
                    query += "where sf.SLCFaultName = 'fault_OT ' and sfv.value = '" + Current_fault_OT_ + "' ;";
                    DataTable dtSlcFaultInfo = context.Select(query);
                    if (dtSlcFaultInfo != null && dtSlcFaultInfo.Rows.Count > 0)
                    {
                        SLCFaultDescription = Convert.ToString(dtSlcFaultInfo.Rows[0]["description"]);
                        SLCFaultSeverity = Convert.ToString(dtSlcFaultInfo.Rows[0]["SLCFaultSeverity"]);
                        message += "Severity:" + SLCFaultSeverity + Environment.NewLine + SLCFaultDescription + Environment.NewLine;
                    }
                }
                if (Current_fault_GF_ != Prevoius_fault_GF_)
                {
                    string query = " select sfv.description ,sf.SLCFaultSeverity from SLCFaults  sf inner join SLCFaultsValues sfv on sf.SLCFaultid = sfv.SLCFaultid ";
                    query += "where sf.SLCFaultName = 'fault_GF ' and sfv.value = '" + Current_fault_GF_ + "' ;";
                    DataTable dtSlcFaultInfo = context.Select(query);
                    if (dtSlcFaultInfo != null && dtSlcFaultInfo.Rows.Count > 0)
                    {
                        SLCFaultDescription = Convert.ToString(dtSlcFaultInfo.Rows[0]["description"]);
                        SLCFaultSeverity = Convert.ToString(dtSlcFaultInfo.Rows[0]["SLCFaultSeverity"]);
                        message += "Severity:" + SLCFaultSeverity + Environment.NewLine + SLCFaultDescription + Environment.NewLine;
                    }
                }
                if (Current_fault_PD_ != Prevoius_fault_PD_)
                {
                    string query = " select sfv.description ,sf.SLCFaultSeverity from SLCFaults  sf inner join SLCFaultsValues sfv on sf.SLCFaultid = sfv.SLCFaultid ";
                    query += "where sf.SLCFaultName = 'fault_PD ' and sfv.value = '" + Current_fault_PD_ + "' ;";
                    DataTable dtSlcFaultInfo = context.Select(query);
                    if (dtSlcFaultInfo != null && dtSlcFaultInfo.Rows.Count > 0)
                    {
                        SLCFaultDescription = Convert.ToString(dtSlcFaultInfo.Rows[0]["description"]);
                        SLCFaultSeverity = Convert.ToString(dtSlcFaultInfo.Rows[0]["SLCFaultSeverity"]);
                        message += "Severity:" + SLCFaultSeverity + Environment.NewLine + SLCFaultDescription + Environment.NewLine;
                    }
                }
                if (Current_fault_PU_ != Prevoius_fault_PU_)
                {
                    string query = " select sfv.description ,sf.SLCFaultSeverity from SLCFaults  sf inner join SLCFaultsValues sfv on sf.SLCFaultid = sfv.SLCFaultid ";
                    query += "where sf.SLCFaultName = 'fault_PU ' and sfv.value = '" + Current_fault_PU_ + "' ;";
                    DataTable dtSlcFaultInfo = context.Select(query);
                    if (dtSlcFaultInfo != null && dtSlcFaultInfo.Rows.Count > 0)
                    {
                        SLCFaultDescription = Convert.ToString(dtSlcFaultInfo.Rows[0]["description"]);
                        SLCFaultSeverity = Convert.ToString(dtSlcFaultInfo.Rows[0]["SLCFaultSeverity"]);
                        message += "Severity:" + SLCFaultSeverity + Environment.NewLine + SLCFaultDescription + Environment.NewLine;
                    }
                }
                if (Current_fault_ZV_ != Prevoius_fault_ZV_)
                {
                    string query = " select sfv.description ,sf.SLCFaultSeverity from SLCFaults  sf inner join SLCFaultsValues sfv on sf.SLCFaultid = sfv.SLCFaultid ";
                    query += "where sf.SLCFaultName = 'fault_ZV ' and sfv.value = '" + Current_fault_ZV_ + "' ;";
                    DataTable dtSlcFaultInfo = context.Select(query);
                    if (dtSlcFaultInfo != null && dtSlcFaultInfo.Rows.Count > 0)
                    {
                        SLCFaultDescription = Convert.ToString(dtSlcFaultInfo.Rows[0]["description"]);
                        SLCFaultSeverity = Convert.ToString(dtSlcFaultInfo.Rows[0]["SLCFaultSeverity"]);
                        message += "Severity:" + SLCFaultSeverity + Environment.NewLine + SLCFaultDescription + Environment.NewLine;
                    }
                }
                if (Current_fault_NV_ != Prevoius_fault_NV_)
                {
                    string query = " select sfv.description ,sf.SLCFaultSeverity from SLCFaults  sf inner join SLCFaultsValues sfv on sf.SLCFaultid = sfv.SLCFaultid ";
                    query += "where sf.SLCFaultName = 'fault_NV ' and sfv.value = '" + Current_fault_NV_ + "' ;";
                    DataTable dtSlcFaultInfo = context.Select(query);
                    if (dtSlcFaultInfo != null && dtSlcFaultInfo.Rows.Count > 0)
                    {
                        SLCFaultDescription = Convert.ToString(dtSlcFaultInfo.Rows[0]["description"]);
                        SLCFaultSeverity = Convert.ToString(dtSlcFaultInfo.Rows[0]["SLCFaultSeverity"]);
                        message += "Severity:" + SLCFaultSeverity + Environment.NewLine + SLCFaultDescription + Environment.NewLine;
                    }
                }

                #region delete n s1 s2 s3 s4 boost
                //if (Current_boost_ != Prevoius_boost_)
                //{
                //    string query = " select sfv.description ,sf.SLCFaultSeverity from SLCFaults  sf inner join SLCFaultsValues sfv on sf.SLCFaultid = sfv.SLCFaultid ";
                //    query += "where sf.SLCFaultName = 'boost ' and sfv.value = '" + Current_boost_ + "' ;";
                //    DataTable dtSlcFaultInfo = context.Select(query);
                //    if (dtSlcFaultInfo != null && dtSlcFaultInfo.Rows.Count > 0)
                //    {
                //        SLCFaultDescription = Convert.ToString(dtSlcFaultInfo.Rows[0]["description"]);
                //        SLCFaultSeverity = Convert.ToString(dtSlcFaultInfo.Rows[0]["SLCFaultSeverity"]);
                //        message += "Severity:" + SLCFaultSeverity + Environment.NewLine + SLCFaultDescription + Environment.NewLine;
                //    }
                //}
                //if (Current_n_ != Prevoius_n_)
                //{
                //    string query = " select sfv.description ,sf.SLCFaultSeverity from SLCFaults  sf inner join SLCFaultsValues sfv on sf.SLCFaultid = sfv.SLCFaultid ";
                //    query += "where sf.SLCFaultName = 'n' and sfv.value = '" + Current_n_ + "' ;";
                //    DataTable dtSlcFaultInfo = context.Select(query);
                //    if (dtSlcFaultInfo != null && dtSlcFaultInfo.Rows.Count > 0)
                //    {
                //        SLCFaultDescription = Convert.ToString(dtSlcFaultInfo.Rows[0]["description"]);
                //        SLCFaultSeverity = Convert.ToString(dtSlcFaultInfo.Rows[0]["SLCFaultSeverity"]);
                //        message += "Severity:" + SLCFaultSeverity + Environment.NewLine + SLCFaultDescription + Environment.NewLine;
                //    }
                //}
                //if (Current_s1_ != Prevoius_s1_)
                //{
                //    string query = " select sfv.description ,sf.SLCFaultSeverity from SLCFaults  sf inner join SLCFaultsValues sfv on sf.SLCFaultid = sfv.SLCFaultid ";
                //    query += "where sf.SLCFaultName = 's1' and sfv.value = '" + Current_s1_ + "' ;";
                //    DataTable dtSlcFaultInfo = context.Select(query);
                //    if (dtSlcFaultInfo != null && dtSlcFaultInfo.Rows.Count > 0)
                //    {
                //        SLCFaultDescription = Convert.ToString(dtSlcFaultInfo.Rows[0]["description"]);
                //        SLCFaultSeverity = Convert.ToString(dtSlcFaultInfo.Rows[0]["SLCFaultSeverity"]);
                //        message += "Severity:" + SLCFaultSeverity + Environment.NewLine + SLCFaultDescription + Environment.NewLine;
                //    }
                //}
                //if (Current_s2_ != Prevoius_s2_)
                //{
                //    string query = " select sfv.description ,sf.SLCFaultSeverity from SLCFaults  sf inner join SLCFaultsValues sfv on sf.SLCFaultid = sfv.SLCFaultid ";
                //    query += "where sf.SLCFaultName = 's2' and sfv.value = '" + Current_s2_ + "' ;";
                //    DataTable dtSlcFaultInfo = context.Select(query);
                //    if (dtSlcFaultInfo != null && dtSlcFaultInfo.Rows.Count > 0)
                //    {
                //        SLCFaultDescription = Convert.ToString(dtSlcFaultInfo.Rows[0]["description"]);
                //        SLCFaultSeverity = Convert.ToString(dtSlcFaultInfo.Rows[0]["SLCFaultSeverity"]);
                //        message += "Severity:" + SLCFaultSeverity + Environment.NewLine + SLCFaultDescription + Environment.NewLine;
                //    }
                //}
                //if (Current_s3_ != Prevoius_s3_)
                //{
                //    string query = " select sfv.description ,sf.SLCFaultSeverity from SLCFaults  sf inner join SLCFaultsValues sfv on sf.SLCFaultid = sfv.SLCFaultid ";
                //    query += "where sf.SLCFaultName = 's3' and sfv.value = '" + Current_s3_ + "' ;";
                //    DataTable dtSlcFaultInfo = context.Select(query);
                //    if (dtSlcFaultInfo != null && dtSlcFaultInfo.Rows.Count > 0)
                //    {
                //        SLCFaultDescription = Convert.ToString(dtSlcFaultInfo.Rows[0]["description"]);
                //        SLCFaultSeverity = Convert.ToString(dtSlcFaultInfo.Rows[0]["SLCFaultSeverity"]);
                //        message += "Severity:" + SLCFaultSeverity + Environment.NewLine + SLCFaultDescription + Environment.NewLine;
                //    }
                //}
                //if (Current_s4_ != Prevoius_s4_)
                //{
                //    string query = " select sfv.description ,sf.SLCFaultSeverity from SLCFaults  sf inner join SLCFaultsValues sfv on sf.SLCFaultid = sfv.SLCFaultid ";
                //    query += "where sf.SLCFaultName = 's1' and sfv.value = '" + Current_s1_ + "' ;";
                //    DataTable dtSlcFaultInfo = context.Select(query);
                //    if (dtSlcFaultInfo != null && dtSlcFaultInfo.Rows.Count > 0)
                //    {
                //        SLCFaultDescription = Convert.ToString(dtSlcFaultInfo.Rows[0]["description"]);
                //        SLCFaultSeverity = Convert.ToString(dtSlcFaultInfo.Rows[0]["SLCFaultSeverity"]);
                //        message += "Severity:" + SLCFaultSeverity + Environment.NewLine + SLCFaultDescription + Environment.NewLine;
                //    }
                //}
                #endregion

                if (Current_aux1_ == "0" && Current_aux2_ == "0" && Current_aux3_ == "0" && Current_aux4_ == "0" && Current_aux5_ == "0" && Current_aux6_ == "0" && Current_aux7_ == "0" && Current_aux8_ == "0" &&
                     Current_r_failure_ == "0" && Current_y_failure_ == "0" && Current_b_failure_ == "0" && Current_fault_ov_ == "0" && Current_fault_uv_ == "0" && Current_fault_OL_ == "0" && Current_fault_UL_ == "0"
                    && Current_fault_OF_ == "0" && Current_fault_UF_ == "0" && Current_fault_OT_ == "0" && Current_fault_GF_ == "0" && Current_fault_PD_ == "0" && Current_fault_PU_ == "0" && Current_fault_ZV_ == "0"
                    && Current_fault_NV_ == "0" && Current_n_ == "0" && Current_s1_ == "0" && Current_s2_ == "0" && Current_s3_ == "0" && Current_s4_ == "0" && Current_boost_ == "0")
                {
                    CommonContentmessage += " OK " + Environment.NewLine;
                }
                else
                {
                    CommonContentmessage += " PROBLEM " + Environment.NewLine;
                }
                if (!string.IsNullOrEmpty(message))
                {
                    string result = CommonContentmessage + Environment.NewLine;
                    result += CurrentRowfault["dt"] + Environment.NewLine + " Zone:" + rowSlcDevices["zone"] + Environment.NewLine + " Area:" + rowSlcDevices["area"];
                    result += "-" + rowSlcDevices["Location"] + Environment.NewLine + "UnitID:" + rowSlcDevices["deviceid"] + Environment.NewLine + rowSlcDevices["phase"] + "Phase ";
                    result += Environment.NewLine + message;
                    return result;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                TxtStatusHistory.Text += Environment.NewLine + " Error while preparing message body...";
                return null;
            }
        }

        public void ExitSMSApp()
        {
            TxtStatusHistory.Text += Environment.NewLine + " Exiting from SMS Engine...";
            context.CloseConnection();
            //Application.Exit();
        }

        #endregion      
    }
}

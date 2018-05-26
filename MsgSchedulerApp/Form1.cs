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

        private void TxtStatusHistory_TextChanged(object sender, EventArgs e)
        {

        }

        private void BtnStartManual_Click(object sender, EventArgs e)
        {
            //timer1.Start();
            TxtStatusHistory.Clear();
            StartApplication();
        }

        public void StartApplication()
        {
            //smsUtilities.processMessage("15", "8003644328", "test");
            TxtStatusHistory.Text += "SMS Engine Started…";
            SetUpDatabaseConnection();
        }

        public void SetUpDatabaseConnection()
        {
            try
            {
                context = new DBConnect();
                if (context.OpenConnection())
                {
                    TxtStatusHistory.Text += Environment.NewLine + "Successfully establishing Database Connections...";
                    ReadingFaultInformation();
                    ExitSMSApp();
                }
                else
                {
                    TxtStatusHistory.Text += Environment.NewLine + "Error while establishing Database Connections...";
                    ExitSMSApp();
                }

            }
            catch (Exception ex)
            {
                TxtStatusHistory.Text += Environment.NewLine + "Error while establishing Database Connections...";
                ExitSMSApp();
            }
        }

        public void ReadingFaultInformation()
        {
            try
            {
                TxtStatusHistory.Text += Environment.NewLine + " Reading Device Fault Information..." + Environment.NewLine;
                string query = "select se.* from slcevents se  inner join slc_devices sd on se._deviceid_ = sd._deviceid_  where se._IsAlertProcessed_ = 0 ;";
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
                    TxtStatusHistory.Text += Environment.NewLine + "No fault information is found during time interval....";
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
                TxtStatusHistory.Text += Environment.NewLine + " Device ID " + row["_deviceid_"];

                TxtStatusHistory.Text += Environment.NewLine + " Reading SMS Service Enability for this Device……….";
                string query = "select * from  slc_devices where _deviceid_ =" + row["_deviceid_"] + " limit 1 ;";
                DataTable dtslcDevices = context.Select(query);
                if (dtslcDevices != null && dtslcDevices.Rows.Count > 0)
                {
                    string cityId = Convert.ToString(dtslcDevices.Rows[0]["_city_"]);
                    query = "select count(*) as Issend from smsauthority where cityid =" + cityId + " and issend =1 ;";
                    int count = context.Count(query);
                    if (count > 0)
                    {
                        TxtStatusHistory.Text += Environment.NewLine + " SMS Service Enabled for this Device……….";

                        query = "select distinct  _mobile_ from cp where _deviceid_ =" + row["_deviceid_"] + "; ";
                        DataTable dtCPUser = context.Select(query);
                        if (dtCPUser != null && dtCPUser.Rows.Count > 0)
                        {
                            //string msgBody = PrepareSmsMessage(row, dtSlcFaultInfo, dtslcDevices);
                            var SelectedValues = dtCPUser.AsEnumerable().Select(s => s.Field<string>("_mobile_")).ToArray().Distinct();
                            string mobile = string.Join(",", SelectedValues);

                            TxtStatusHistory.Text += Environment.NewLine + " Sending SMS to the Users of the Device……….";
                            query = "select  * from slcevents where _deviceid_ ='" + row["_deviceid_"] + "' and _id_ < " + row["_id_"] + " order by _dt_ desc limit 1 ; ";
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

                                    query = "INSERT INTO smsenthistory (deviceid,sentdate,senttime,statusid)VALUES('" + row["_deviceid_"] + "','" + DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + "','" + DateTime.Now.ToShortTimeString() + "', '1')";
                                    context.Insert(query);

                                    query = "update slcevents set _IsAlertProcessed_ = '1' where _id_ =" + row["_id_"] + "; ";
                                    context.Update(query);
                                }
                                else
                                {
                                    TxtStatusHistory.Text += Environment.NewLine + " Error while sending sms to all Users………." + Environment.NewLine;

                                    query = "INSERT INTO smsenthistory (deviceid,sentdate,senttime,statusid) VALUES ('" + row["_deviceid_"] + "','" + DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + "','" + DateTime.Now.ToShortTimeString() + "', '1')";
                                    context.Insert(query);

                                    query = "update slcevents set _IsAlertProcessed_ = '1' where _id_ =" + row["_id_"] + "; ";
                                    context.Update(query);
                                }
                            }
                            else
                            {
                                TxtStatusHistory.Text += Environment.NewLine + " Previous record not found for the devices..." + Environment.NewLine;
                            }
                        }
                        else
                        {
                            TxtStatusHistory.Text += Environment.NewLine + " User is not available for this device " + row["_deviceid_"] + Environment.NewLine;
                        }
                    }
                    else
                    {
                        TxtStatusHistory.Text += Environment.NewLine + " SMS Service NOT Enabled for this Device……….";
                        TxtStatusHistory.Text += Environment.NewLine + " END Device Processing" + Environment.NewLine;
                    }
                }
                else
                {
                    TxtStatusHistory.Text += Environment.NewLine + " SLc Devices information not found in database..." + Environment.NewLine;
                }
            }
            catch (Exception ex)
            {
                TxtStatusHistory.Text += Environment.NewLine + " Error while processing data………." + Environment.NewLine;
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
                string CurrentEventid_ = Convert.ToString(CurrentRowfault["_id_"]),
                    Current_slcid_ = Convert.ToString(CurrentRowfault["_slcid_"])
                    , Current_deviceid_ = Convert.ToString(CurrentRowfault["_deviceid_"])
                    , Current_dt_ = Convert.ToString(CurrentRowfault["_dt_"])
                    , Current_aux1_ = Convert.ToString(CurrentRowfault["_aux1_"])
                    , Current_aux2_ = Convert.ToString(CurrentRowfault["_aux2_"])
                    , Current_aux3_ = Convert.ToString(CurrentRowfault["_aux3_"])
                    , Current_aux4_ = Convert.ToString(CurrentRowfault["_aux4_"])
                    , Current_aux5_ = Convert.ToString(CurrentRowfault["_aux5_"])
                    , Current_aux6_ = Convert.ToString(CurrentRowfault["_aux6_"])
                    , Current_aux7_ = Convert.ToString(CurrentRowfault["_aux7_"])
                    , Current_aux8_ = Convert.ToString(CurrentRowfault["_aux8_"])
                    , Current_r_failure_ = Convert.ToString(CurrentRowfault["_r_failure_"])
                    , Current_y_failure_ = Convert.ToString(CurrentRowfault["_y_failure_"])
                    , Current_b_failure_ = Convert.ToString(CurrentRowfault["_b_failure_"])
                    , Current_r_ = Convert.ToString(CurrentRowfault["_r_"])
                    , Current_y_ = Convert.ToString(CurrentRowfault["_y_"])
                    , Current_b_ = Convert.ToString(CurrentRowfault["_b_"])
                    , Current_fault_ov_ = Convert.ToString(CurrentRowfault["_fault_ov_"])
                    , Current_fault_uv_ = Convert.ToString(CurrentRowfault["_fault_uv_"])
                    , Current_fault_OL_ = Convert.ToString(CurrentRowfault["_fault_OL_"])
                    , Current_fault_UL_ = Convert.ToString(CurrentRowfault["_fault_UL_"])
                    , Current_fault_OF_ = Convert.ToString(CurrentRowfault["_fault_OF_"])
                    , Current_fault_UF_ = Convert.ToString(CurrentRowfault["_fault_UF_"])
                    , Current_fault_OT_ = Convert.ToString(CurrentRowfault["_fault_OT_"])
                    , Current_fault_GF_ = Convert.ToString(CurrentRowfault["_fault_GF_"])
                    , Current_fault_PD_ = Convert.ToString(CurrentRowfault["_fault_PD_"])
                    , Current_fault_PU_ = Convert.ToString(CurrentRowfault["_fault_PU_"])
                    , Current_fault_ZV_ = Convert.ToString(CurrentRowfault["_fault_ZV_"])
                    , Current_fault_NV_ = Convert.ToString(CurrentRowfault["_fault_NV_"])
                    , Current_n_ = Convert.ToString(CurrentRowfault["_n_"])
                    , Current_s1_ = Convert.ToString(CurrentRowfault["_s1_"])
                    , Current_s2_ = Convert.ToString(CurrentRowfault["_s2_"])
                    , Current_s3_ = Convert.ToString(CurrentRowfault["_s3_"])
                    , Current_s4_ = Convert.ToString(CurrentRowfault["_s4_"])
                    , Current_boost_ = Convert.ToString(CurrentRowfault["_boost_"]);

                string PrevoiusRowEventid_ = Convert.ToString(PrevoiusRowfault["_id_"]),
                   Prevoius_slcid_ = Convert.ToString(PrevoiusRowfault["_slcid_"])
                   , Prevoius_deviceid_ = Convert.ToString(PrevoiusRowfault["_deviceid_"])
                   , Prevoius_dt_ = Convert.ToString(PrevoiusRowfault["_dt_"])
                   , Prevoius_aux1_ = Convert.ToString(PrevoiusRowfault["_aux1_"])
                   , Prevoius_aux2_ = Convert.ToString(PrevoiusRowfault["_aux2_"])
                   , Prevoius_aux3_ = Convert.ToString(PrevoiusRowfault["_aux3_"])
                   , Prevoius_aux4_ = Convert.ToString(PrevoiusRowfault["_aux4_"])
                   , Prevoius_aux5_ = Convert.ToString(PrevoiusRowfault["_aux5_"])
                   , Prevoius_aux6_ = Convert.ToString(PrevoiusRowfault["_aux6_"])
                   , Prevoius_aux7_ = Convert.ToString(PrevoiusRowfault["_aux7_"])
                   , Prevoius_aux8_ = Convert.ToString(PrevoiusRowfault["_aux8_"])
                   , Prevoius_r_failure_ = Convert.ToString(PrevoiusRowfault["_r_failure_"])
                   , Prevoius_y_failure_ = Convert.ToString(PrevoiusRowfault["_y_failure_"])
                   , Prevoius_b_failure_ = Convert.ToString(PrevoiusRowfault["_b_failure_"])
                   , Prevoius_r_ = Convert.ToString(PrevoiusRowfault["_r_"])
                   , Prevoius_y_ = Convert.ToString(PrevoiusRowfault["_y_"])
                   , Prevoius_b_ = Convert.ToString(PrevoiusRowfault["_b_"])
                   , Prevoius_fault_ov_ = Convert.ToString(PrevoiusRowfault["_fault_ov_"])
                   , Prevoius_fault_uv_ = Convert.ToString(PrevoiusRowfault["_fault_uv_"])
                   , Prevoius_fault_OL_ = Convert.ToString(PrevoiusRowfault["_fault_OL_"])
                   , Prevoius_fault_UL_ = Convert.ToString(PrevoiusRowfault["_fault_UL_"])
                   , Prevoius_fault_OF_ = Convert.ToString(PrevoiusRowfault["_fault_OF_"])
                   , Prevoius_fault_UF_ = Convert.ToString(PrevoiusRowfault["_fault_UF_"])
                   , Prevoius_fault_OT_ = Convert.ToString(PrevoiusRowfault["_fault_OT_"])
                   , Prevoius_fault_GF_ = Convert.ToString(PrevoiusRowfault["_fault_GF_"])
                   , Prevoius_fault_PD_ = Convert.ToString(PrevoiusRowfault["_fault_PD_"])
                   , Prevoius_fault_PU_ = Convert.ToString(PrevoiusRowfault["_fault_PU_"])
                   , Prevoius_fault_ZV_ = Convert.ToString(PrevoiusRowfault["_fault_ZV_"])
                   , Prevoius_fault_NV_ = Convert.ToString(PrevoiusRowfault["_fault_NV_"])
                   , Prevoius_n_ = Convert.ToString(PrevoiusRowfault["_n_"])
                   , Prevoius_s1_ = Convert.ToString(PrevoiusRowfault["_s1_"])
                   , Prevoius_s2_ = Convert.ToString(PrevoiusRowfault["_s2_"])
                   , Prevoius_s3_ = Convert.ToString(PrevoiusRowfault["_s3_"])
                   , Prevoius_s4_ = Convert.ToString(PrevoiusRowfault["_s4_"])
                   , Prevoius_boost_ = Convert.ToString(PrevoiusRowfault["_boost_"]);

                TxtStatusHistory.Text += Environment.NewLine + " Preparing SMS……….";

                if (Convert.ToString(rowSlcDevices["_phase_"]) == "1")
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
                if (Convert.ToString(rowSlcDevices["_phase_"]) == "3")
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
                    result += CurrentRowfault["_dt_"] + Environment.NewLine + " Zone:" + rowSlcDevices["_zone_"] + Environment.NewLine + " Area:" + rowSlcDevices["_area_"];
                    result += "-" + rowSlcDevices["_Location_"] + Environment.NewLine + "UnitID:" + rowSlcDevices["_deviceid_"] + Environment.NewLine + rowSlcDevices["_phase_"] + "Phase ";
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
            string DevicePhase = Convert.ToString(rowSlcDevices["_phase_"]);
            string CommonContentmessage = "STREET LIGHT ALERT " + Environment.NewLine, message = string.Empty;

            try
            {
                string SLCFaultDescription, SLCFaultSeverity = null;
                string CurrentEventid_ = Convert.ToString(CurrentRowfault["_id_"]),
                    Current_slcid_ = Convert.ToString(CurrentRowfault["_slcid_"])
                    , Current_deviceid_ = Convert.ToString(CurrentRowfault["_deviceid_"])
                    , Current_dt_ = Convert.ToString(CurrentRowfault["_dt_"])
                    , Current_aux1_ = Convert.ToString(CurrentRowfault["_aux1_"])
                    , Current_aux2_ = Convert.ToString(CurrentRowfault["_aux2_"])
                    , Current_aux3_ = Convert.ToString(CurrentRowfault["_aux3_"])
                    , Current_aux4_ = Convert.ToString(CurrentRowfault["_aux4_"])
                    , Current_aux5_ = Convert.ToString(CurrentRowfault["_aux5_"])
                    , Current_aux6_ = Convert.ToString(CurrentRowfault["_aux6_"])
                    , Current_aux7_ = Convert.ToString(CurrentRowfault["_aux7_"])
                    , Current_aux8_ = Convert.ToString(CurrentRowfault["_aux8_"])
                    , Current_r_failure_ = Convert.ToString(CurrentRowfault["_r_failure_"])
                    , Current_y_failure_ = Convert.ToString(CurrentRowfault["_y_failure_"])
                    , Current_b_failure_ = Convert.ToString(CurrentRowfault["_b_failure_"])
                    , Current_r_ = Convert.ToString(CurrentRowfault["_r_"])
                    , Current_y_ = Convert.ToString(CurrentRowfault["_y_"])
                    , Current_b_ = Convert.ToString(CurrentRowfault["_b_"])
                    , Current_fault_ov_ = Convert.ToString(CurrentRowfault["_fault_ov_"])
                    , Current_fault_uv_ = Convert.ToString(CurrentRowfault["_fault_uv_"])
                    , Current_fault_OL_ = Convert.ToString(CurrentRowfault["_fault_OL_"])
                    , Current_fault_UL_ = Convert.ToString(CurrentRowfault["_fault_UL_"])
                    , Current_fault_OF_ = Convert.ToString(CurrentRowfault["_fault_OF_"])
                    , Current_fault_UF_ = Convert.ToString(CurrentRowfault["_fault_UF_"])
                    , Current_fault_OT_ = Convert.ToString(CurrentRowfault["_fault_OT_"])
                    , Current_fault_GF_ = Convert.ToString(CurrentRowfault["_fault_GF_"])
                    , Current_fault_PD_ = Convert.ToString(CurrentRowfault["_fault_PD_"])
                    , Current_fault_PU_ = Convert.ToString(CurrentRowfault["_fault_PU_"])
                    , Current_fault_ZV_ = Convert.ToString(CurrentRowfault["_fault_ZV_"])
                    , Current_fault_NV_ = Convert.ToString(CurrentRowfault["_fault_NV_"])
                    , Current_n_ = Convert.ToString(CurrentRowfault["_n_"])
                    , Current_s1_ = Convert.ToString(CurrentRowfault["_s1_"])
                    , Current_s2_ = Convert.ToString(CurrentRowfault["_s2_"])
                    , Current_s3_ = Convert.ToString(CurrentRowfault["_s3_"])
                    , Current_s4_ = Convert.ToString(CurrentRowfault["_s4_"])
                    , Current_boost_ = Convert.ToString(CurrentRowfault["_boost_"]);

                string PrevoiusRowEventid_ = Convert.ToString(PrevoiusRowfault["_id_"]),
                   Prevoius_slcid_ = Convert.ToString(PrevoiusRowfault["_slcid_"])
                   , Prevoius_deviceid_ = Convert.ToString(PrevoiusRowfault["_deviceid_"])
                   , Prevoius_dt_ = Convert.ToString(PrevoiusRowfault["_dt_"])
                   , Prevoius_aux1_ = Convert.ToString(PrevoiusRowfault["_aux1_"])
                   , Prevoius_aux2_ = Convert.ToString(PrevoiusRowfault["_aux2_"])
                   , Prevoius_aux3_ = Convert.ToString(PrevoiusRowfault["_aux3_"])
                   , Prevoius_aux4_ = Convert.ToString(PrevoiusRowfault["_aux4_"])
                   , Prevoius_aux5_ = Convert.ToString(PrevoiusRowfault["_aux5_"])
                   , Prevoius_aux6_ = Convert.ToString(PrevoiusRowfault["_aux6_"])
                   , Prevoius_aux7_ = Convert.ToString(PrevoiusRowfault["_aux7_"])
                   , Prevoius_aux8_ = Convert.ToString(PrevoiusRowfault["_aux8_"])
                   , Prevoius_r_failure_ = Convert.ToString(PrevoiusRowfault["_r_failure_"])
                   , Prevoius_y_failure_ = Convert.ToString(PrevoiusRowfault["_y_failure_"])
                   , Prevoius_b_failure_ = Convert.ToString(PrevoiusRowfault["_b_failure_"])
                   , Prevoius_r_ = Convert.ToString(PrevoiusRowfault["_r_"])
                   , Prevoius_y_ = Convert.ToString(PrevoiusRowfault["_y_"])
                   , Prevoius_b_ = Convert.ToString(PrevoiusRowfault["_b_"])
                   , Prevoius_fault_ov_ = Convert.ToString(PrevoiusRowfault["_fault_ov_"])
                   , Prevoius_fault_uv_ = Convert.ToString(PrevoiusRowfault["_fault_uv_"])
                   , Prevoius_fault_OL_ = Convert.ToString(PrevoiusRowfault["_fault_OL_"])
                   , Prevoius_fault_UL_ = Convert.ToString(PrevoiusRowfault["_fault_UL_"])
                   , Prevoius_fault_OF_ = Convert.ToString(PrevoiusRowfault["_fault_OF_"])
                   , Prevoius_fault_UF_ = Convert.ToString(PrevoiusRowfault["_fault_UF_"])
                   , Prevoius_fault_OT_ = Convert.ToString(PrevoiusRowfault["_fault_OT_"])
                   , Prevoius_fault_GF_ = Convert.ToString(PrevoiusRowfault["_fault_GF_"])
                   , Prevoius_fault_PD_ = Convert.ToString(PrevoiusRowfault["_fault_PD_"])
                   , Prevoius_fault_PU_ = Convert.ToString(PrevoiusRowfault["_fault_PU_"])
                   , Prevoius_fault_ZV_ = Convert.ToString(PrevoiusRowfault["_fault_ZV_"])
                   , Prevoius_fault_NV_ = Convert.ToString(PrevoiusRowfault["_fault_NV_"])
                   , Prevoius_n_ = Convert.ToString(PrevoiusRowfault["_n_"])
                   , Prevoius_s1_ = Convert.ToString(PrevoiusRowfault["_s1_"])
                   , Prevoius_s2_ = Convert.ToString(PrevoiusRowfault["_s2_"])
                   , Prevoius_s3_ = Convert.ToString(PrevoiusRowfault["_s3_"])
                   , Prevoius_s4_ = Convert.ToString(PrevoiusRowfault["_s4_"])
                   , Prevoius_boost_ = Convert.ToString(PrevoiusRowfault["_boost_"]);

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
                if (Current_boost_ != Prevoius_boost_)
                {
                    string query = " select sfv.description ,sf.SLCFaultSeverity from SLCFaults  sf inner join SLCFaultsValues sfv on sf.SLCFaultid = sfv.SLCFaultid ";
                    query += "where sf.SLCFaultName = 'boost ' and sfv.value = '" + Current_boost_ + "' ;";
                    DataTable dtSlcFaultInfo = context.Select(query);
                    if (dtSlcFaultInfo != null && dtSlcFaultInfo.Rows.Count > 0)
                    {
                        SLCFaultDescription = Convert.ToString(dtSlcFaultInfo.Rows[0]["description"]);
                        SLCFaultSeverity = Convert.ToString(dtSlcFaultInfo.Rows[0]["SLCFaultSeverity"]);
                        message += "Severity:" + SLCFaultSeverity + Environment.NewLine + SLCFaultDescription + Environment.NewLine;
                    }
                }
                if (Current_n_ != Prevoius_n_)
                {
                    string query = " select sfv.description ,sf.SLCFaultSeverity from SLCFaults  sf inner join SLCFaultsValues sfv on sf.SLCFaultid = sfv.SLCFaultid ";
                    query += "where sf.SLCFaultName = 'n' and sfv.value = '" + Current_n_ + "' ;";
                    DataTable dtSlcFaultInfo = context.Select(query);
                    if (dtSlcFaultInfo != null && dtSlcFaultInfo.Rows.Count > 0)
                    {
                        SLCFaultDescription = Convert.ToString(dtSlcFaultInfo.Rows[0]["description"]);
                        SLCFaultSeverity = Convert.ToString(dtSlcFaultInfo.Rows[0]["SLCFaultSeverity"]);
                        message += "Severity:" + SLCFaultSeverity + Environment.NewLine + SLCFaultDescription + Environment.NewLine;
                    }
                }
                if (Current_s1_ != Prevoius_s1_)
                {
                    string query = " select sfv.description ,sf.SLCFaultSeverity from SLCFaults  sf inner join SLCFaultsValues sfv on sf.SLCFaultid = sfv.SLCFaultid ";
                    query += "where sf.SLCFaultName = 's1' and sfv.value = '" + Current_s1_ + "' ;";
                    DataTable dtSlcFaultInfo = context.Select(query);
                    if (dtSlcFaultInfo != null && dtSlcFaultInfo.Rows.Count > 0)
                    {
                        SLCFaultDescription = Convert.ToString(dtSlcFaultInfo.Rows[0]["description"]);
                        SLCFaultSeverity = Convert.ToString(dtSlcFaultInfo.Rows[0]["SLCFaultSeverity"]);
                        message += "Severity:" + SLCFaultSeverity + Environment.NewLine + SLCFaultDescription + Environment.NewLine;
                    }
                }
                if (Current_s2_ != Prevoius_s2_)
                {
                    string query = " select sfv.description ,sf.SLCFaultSeverity from SLCFaults  sf inner join SLCFaultsValues sfv on sf.SLCFaultid = sfv.SLCFaultid ";
                    query += "where sf.SLCFaultName = 's2' and sfv.value = '" + Current_s2_ + "' ;";
                    DataTable dtSlcFaultInfo = context.Select(query);
                    if (dtSlcFaultInfo != null && dtSlcFaultInfo.Rows.Count > 0)
                    {
                        SLCFaultDescription = Convert.ToString(dtSlcFaultInfo.Rows[0]["description"]);
                        SLCFaultSeverity = Convert.ToString(dtSlcFaultInfo.Rows[0]["SLCFaultSeverity"]);
                        message += "Severity:" + SLCFaultSeverity + Environment.NewLine + SLCFaultDescription + Environment.NewLine;
                    }
                }
                if (Current_s3_ != Prevoius_s3_)
                {
                    string query = " select sfv.description ,sf.SLCFaultSeverity from SLCFaults  sf inner join SLCFaultsValues sfv on sf.SLCFaultid = sfv.SLCFaultid ";
                    query += "where sf.SLCFaultName = 's3' and sfv.value = '" + Current_s3_ + "' ;";
                    DataTable dtSlcFaultInfo = context.Select(query);
                    if (dtSlcFaultInfo != null && dtSlcFaultInfo.Rows.Count > 0)
                    {
                        SLCFaultDescription = Convert.ToString(dtSlcFaultInfo.Rows[0]["description"]);
                        SLCFaultSeverity = Convert.ToString(dtSlcFaultInfo.Rows[0]["SLCFaultSeverity"]);
                        message += "Severity:" + SLCFaultSeverity + Environment.NewLine + SLCFaultDescription + Environment.NewLine;
                    }
                }
                if (Current_s4_ != Prevoius_s4_)
                {
                    string query = " select sfv.description ,sf.SLCFaultSeverity from SLCFaults  sf inner join SLCFaultsValues sfv on sf.SLCFaultid = sfv.SLCFaultid ";
                    query += "where sf.SLCFaultName = 's1' and sfv.value = '" + Current_s1_ + "' ;";
                    DataTable dtSlcFaultInfo = context.Select(query);
                    if (dtSlcFaultInfo != null && dtSlcFaultInfo.Rows.Count > 0)
                    {
                        SLCFaultDescription = Convert.ToString(dtSlcFaultInfo.Rows[0]["description"]);
                        SLCFaultSeverity = Convert.ToString(dtSlcFaultInfo.Rows[0]["SLCFaultSeverity"]);
                        message += "Severity:" + SLCFaultSeverity + Environment.NewLine + SLCFaultDescription + Environment.NewLine;
                    }
                }
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
                    result += CurrentRowfault["_dt_"] + Environment.NewLine + " Zone:" + rowSlcDevices["_zone_"] + Environment.NewLine + " Area:" + rowSlcDevices["_area_"];
                    result += "-" + rowSlcDevices["_Location_"] + Environment.NewLine + "UnitID:" + rowSlcDevices["_deviceid_"] + Environment.NewLine + rowSlcDevices["_phase_"] + "Phase ";
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
            TxtStatusHistory.Text += Environment.NewLine + "Exiting from SMS Engine...";
            context.CloseConnection();
            //Application.Exit();
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
    }
}

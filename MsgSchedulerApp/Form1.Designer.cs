using System.Configuration;

namespace MsgSchedulerApp
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        int timeinterval = int.Parse(ConfigurationManager.AppSettings["Interval"]);
        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.TxtStatusHistory = new System.Windows.Forms.RichTextBox();
            this.BtnStartManual = new System.Windows.Forms.Button();
            this.btnclearLog = new System.Windows.Forms.Button();

            this.timer1 = new System.Windows.Forms.Timer(this.components);
            timer1.Tick += new System.EventHandler(timer1_Tick);
            timer1.Interval = (1000 * 60 * timeinterval);
            timer1.Enabled = true;
            timer1.Start();
            this.SuspendLayout();
            // 
            // TxtStatusHistory
            // 
            this.TxtStatusHistory.Location = new System.Drawing.Point(0, 64);
            this.TxtStatusHistory.MaximumSize = new System.Drawing.Size(1064, 450);
            this.TxtStatusHistory.Name = "TxtStatusHistory";
            this.TxtStatusHistory.ReadOnly = false;
            this.TxtStatusHistory.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.TxtStatusHistory.Size = new System.Drawing.Size(1064, 450);
            this.TxtStatusHistory.TabIndex = 0;
            this.TxtStatusHistory.Text = "SMS Engine Started.";
            this.TxtStatusHistory.TextChanged += new System.EventHandler(this.TxtStatusHistory_TextChanged);
            // 
            // BtnStartManual
            // 
            this.BtnStartManual.Location = new System.Drawing.Point(0, 4);
            this.BtnStartManual.Name = "BtnStartManual";
            this.BtnStartManual.Size = new System.Drawing.Size(485, 54);
            this.BtnStartManual.TabIndex = 1;
            this.BtnStartManual.Text = "Start Manually";
            this.BtnStartManual.UseVisualStyleBackColor = true;
            this.BtnStartManual.Click += new System.EventHandler(this.BtnStartManual_Click);
            // 
            // btnclearLog
            // 
            this.btnclearLog.Location = new System.Drawing.Point(545, 4);
            this.btnclearLog.Name = "btnclearLog";
            this.btnclearLog.Size = new System.Drawing.Size(485, 54);
            this.btnclearLog.TabIndex = 2;
            this.btnclearLog.Text = "Clear Log";
            this.btnclearLog.UseVisualStyleBackColor = true;
            this.btnclearLog.Click += new System.EventHandler(this.btnclearLog_Click);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1067, 521);
            this.Controls.Add(this.btnclearLog);
            this.Controls.Add(this.BtnStartManual);
            this.Controls.Add(this.TxtStatusHistory);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SMSEngineVer1.0";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox TxtStatusHistory;
        private System.Windows.Forms.Button BtnStartManual;
        private System.Windows.Forms.Button btnclearLog;
        private System.Windows.Forms.Timer timer1;
    }
}


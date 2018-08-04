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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // TxtStatusHistory
            // 
            this.TxtStatusHistory.Location = new System.Drawing.Point(15, 85);
            this.TxtStatusHistory.MaximumSize = new System.Drawing.Size(1064, 450);
            this.TxtStatusHistory.Name = "TxtStatusHistory";
            this.TxtStatusHistory.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.TxtStatusHistory.Size = new System.Drawing.Size(509, 192);
            this.TxtStatusHistory.TabIndex = 0;
            this.TxtStatusHistory.Text = "";
            this.TxtStatusHistory.TextChanged += new System.EventHandler(this.TxtStatusHistory_TextChanged);
            // 
            // BtnStartManual
            // 
            this.BtnStartManual.Location = new System.Drawing.Point(15, 51);
            this.BtnStartManual.Name = "BtnStartManual";
            this.BtnStartManual.Size = new System.Drawing.Size(261, 28);
            this.BtnStartManual.TabIndex = 1;
            this.BtnStartManual.Text = "Start Manually";
            this.BtnStartManual.UseVisualStyleBackColor = true;
            this.BtnStartManual.Click += new System.EventHandler(this.BtnStartManual_Click);
            // 
            // btnclearLog
            // 
            this.btnclearLog.Location = new System.Drawing.Point(282, 51);
            this.btnclearLog.Name = "btnclearLog";
            this.btnclearLog.Size = new System.Drawing.Size(242, 28);
            this.btnclearLog.TabIndex = 2;
            this.btnclearLog.Text = "Clear Log";
            this.btnclearLog.UseVisualStyleBackColor = true;
            this.btnclearLog.Click += new System.EventHandler(this.btnclearLog_Click);
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            timer1.Interval = (1000 * 60 * timeinterval);
            timer1.Enabled = true;
            timer1.Start();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(188, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(188, 40);
            this.label1.TabIndex = 3;
            this.label1.Text = "SMS Engine\r\nAaradhya Electronics";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(249, 280);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(275, 17);
            this.label2.TabIndex = 4;
            this.label2.Text = "Powered By Increpe Technologies Pvt. Ltd";
            this.label2.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(536, 306);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
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
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox TxtStatusHistory;
        private System.Windows.Forms.Button BtnStartManual;
        private System.Windows.Forms.Button btnclearLog;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}


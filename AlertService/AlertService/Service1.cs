using System;
using System.Data;
using System.Net;
using System.Net.Mail;
using System.ServiceProcess;
using System.IO;
using System.Threading;
using System.Configuration;
using System.Data.SqlClient;
using Microsoft.Office.Interop.Outlook;

namespace AlertService
{
    public partial class Service1 : ServiceBase
    {

        static System.Timers.Timer t;

        static AurivoAlertClasses.AlertSys alerts;

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            alerts = new AurivoAlertClasses.AlertSys();
            alerts.SetupParam();

            t = new System.Timers.Timer(10000);
            t.Elapsed += new System.Timers.ElapsedEventHandler(T_Elapsed);

            t.AutoReset = true;
            t.Enabled = true;
            t.Start();
        }

        
        private static void T_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            t.Enabled = false;
            t.Stop();

            //WriteLog("Time Lapse");
            
            alerts.CheckForAlerts();
            t.Enabled = true;
            t.Start();
        }

        
        //private void CreateOrderTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        //{
        //    createOrderTimer.Enabled = false;
        //    AurivoAlertClasses.AlertSys alerts = new AurivoAlertClasses.AlertSys();
        //    alerts.CheckForAlerts();
        //    createOrderTimer.Enabled = true;
        //}
        protected override void OnStop()
        {
            //Write to file that the service stopped
            alerts.DisposeParam();
            Dispose();
        }

        private static void WriteLog(string msg)
        {
            System.IO.StreamWriter fd = new System.IO.StreamWriter("\\aurivosupport\\alertlog.txt", true);
            fd.WriteLine(msg + " @ " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
            fd.Close();
        }
    }
}

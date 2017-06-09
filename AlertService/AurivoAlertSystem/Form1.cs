using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;



namespace AurivoAlertSystem
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            testAlerts1();
        }

        private void testAlerts()
        {
            AurivoAlertClasses.AlertSys alerts = new AurivoAlertClasses.AlertSys();
            alerts.CheckForAlerts();
        }


        private void testAlerts1()
        {
            AurivoAlertClasses.AlertSys alerts = new AurivoAlertClasses.AlertSys();
            alerts.CheckForAlerts();
        }
    }
}

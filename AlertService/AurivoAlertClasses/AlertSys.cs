//Authors: Adam Lee & Mark McGoohan. Completion Date: 18/05/2017 @ Aurivo Head Office
using System;
using System.Data;
using System.Net;
using System.Net.Mail;
using System.Configuration;
using System.Data.SqlClient;
using Microsoft.Office.Interop.Outlook;
using Outlook = Microsoft.Office.Interop.Outlook;
using System.Text;
using System.Security;
using EWServices;
using Microsoft.Exchange.WebServices.Data;
using System.Windows.Forms;

namespace AurivoAlertClasses
{
    public class AlertSys
    {
        //Retrieve private credentials for the database
        private class ComParam
        {
            public String EmailAlert { get; set; }
            public String EmailServer { get; set; }
            public Int32 PortNumber { get; set; }
            public Boolean SSL { get; set; }

            public String MailboxUsername { get; set; }
            public String MailboxPassword { get; set; }

            public String SMSUsername { get; set; }
            public String SMSPassword { get; set; }
            public String SMSAPIId { get; set; }

          

            public ComParam GetParameters(SqlConnection conn)
            {
                ComParam comms = new ComParam();

                SqlCommand cmd = new SqlCommand("", conn);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                cmd.CommandText = "Select top 1 * from Control";
                DataTable dt = new DataTable("Param");
                da.SelectCommand = cmd;
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    comms = new ComParam();
                    comms.EmailAlert = dt.Rows[0]["EmailAlert"].ToString();
                    comms.EmailServer = dt.Rows[0]["EmailServer"].ToString();
                    comms.PortNumber = (Int32)dt.Rows[0]["PortNumber"];
                    comms.SSL = Convert.ToBoolean(dt.Rows[0]["SSL"]);

                    comms.MailboxUsername = dt.Rows[0]["MailboxUsername"].ToString();
                    comms.MailboxPassword = dt.Rows[0]["MailboxPassword"].ToString();

                    comms.SMSUsername = dt.Rows[0]["SMSUsername"].ToString();
                    comms.SMSPassword = dt.Rows[0]["SMSPassword"].ToString();

                    comms.SMSAPIId = dt.Rows[0]["SMSAPIId"].ToString();
                    
                    
                }

                cmd.Dispose();
                da.Dispose();
                dt.Dispose();

                return comms;
            }

        }


        private ComParam comms;
        private SqlConnection conn;

        public void SetupParam()
        {
            string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            conn = new SqlConnection(constr);
            conn.Open();

            comms = new ComParam();
            comms = comms.GetParameters(conn);
        }
        public void DisposeParam()
        {
            conn.Close();
            conn.Dispose();
            comms = null;
        }

        public void CheckForAlerts()
        {
            StringBuilder lg = new StringBuilder();
            DataTable dt = new DataTable();

            try
            {

                if (conn == null)
                    SetupParam();
                if (conn.State == ConnectionState.Closed || conn.State == ConnectionState.Broken)
                    conn.Open();

                SqlCommand cmd = new SqlCommand("", conn);
                SqlDataAdapter sda = new SqlDataAdapter(cmd);


                //_Application _app = new Outlook.Application();
                //_NameSpace _ns = _app.GetNamespace("MAPI");

                //MAPIFolder inbox = _ns.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderInbox);
                //_ns.SendAndReceive(true);

                IUserData UserData = clsUserData.GetUserData(comms.MailboxUsername, comms.MailboxPassword);
                ExchangeService service = Service.ConnectToService(UserData, new TraceListener());

                PropertySet itempropertyset = new PropertySet(BasePropertySet.FirstClassProperties);


                ItemView view = new ItemView(1000);
                view.PropertySet = itempropertyset;

                string querystring = "Kind:email";
                // Find the first email message in the Inbox that has attachments. This results in a FindItem operation call to EWS.
                FindItemsResults<Item> results = service.FindItems(WellKnownFolderName.Inbox, querystring, view);


                if (results.TotalCount > 0)
                {

                    foreach (EmailMessage email in results.Items)
                    {
                        String ss = email.Subject.ToString();

                        UserData = null;
                        service = null;

                        // have email now...
                        cmd.CommandText = "select Id from EmailAlerts where Description = '" + email.Subject.ToString().Trim() + "'";
                        var _Id = cmd.ExecuteScalar();

                        if (_Id != null)
                        {
                            String Body = "";

                            WriteLog("Process Mail - have alert - " + ss + " - " + results.Items.Count);
                            // we have an alert active...
                            lg.Append("New Alert - ");
                            lg.Append((email).Subject.ToString());
                            lg.AppendLine(" - Received @ " + DateTime.Now.ToString("dd/MM/yyyy HH:mm") + "<br/>");

                            //Retrieve the view from db...
                            cmd.CommandText = "Select * from EmailAlertDetails where Id = " + _Id.ToString() + " Order by ReplyTo";
                            sda.SelectCommand = cmd;
                            dt = new DataTable("AlertDetails");
                            sda.Fill(dt);

                            if (dt.Rows.Count > 0)
                            {
                                if (Convert.ToInt32(dt.Rows[0]["AlertType"]) == 1)
                                    itempropertyset.RequestedBodyType = BodyType.Text;
                                else
                                    itempropertyset.RequestedBodyType = BodyType.HTML;

                                try
                                {
                                    email.Load(itempropertyset);
                                    Body = email.Body.Text;
                                }
                                catch (System.Exception ee)
                                { // Failed to get mail
                                    WriteLog("System Error " + ee.Message.ToString() + " - INNER:" + (ee.InnerException == null ? "" : ee.InnerException.ToString()));
                                    dt.Rows.Clear();
                                }
                            }


                            foreach (DataRow row in dt.Rows)
                            {


                                // People to Alert...
                                if (Convert.ToInt32(row["AlertType"]) == 0)
                                {   // Email
                                    try
                                    {
                                        lg.Append("Email sent to - " + row["Name"].ToString() + " @ " + DateTime.Now.ToString("dd / MM / yyyy HH: mm"));
                                        SendMail(row["Name"].ToString(), row["Email"].ToString(), row["EmailSubject"].ToString(), (Body == null || Body == "" ? row["DefaultMessage"].ToString() : Body));
                                        lg.AppendLine(" - Successful.<br/>");
                                    }
                                    catch (System.Exception ex)
                                    {
                                        Console.WriteLine(ex);
                                        lg.AppendLine(" - Unsuccessful.<br/>");
                                    }
                                }
                                else
                                {   //SMS
                                    try
                                    {
                                        lg.Append("SMS sent to - " + row["Name"].ToString() + " @ " + DateTime.Now.ToString("dd / MM / yyyy HH: mm"));


                                        SendSMS(row["Name"].ToString(), row["Phone"].ToString(), row["EmailSubject"].ToString(), (Body == null || Body == "" ? row["DefaultMessage"].ToString() : Body));

                                        lg.AppendLine(" - Successful.<br/>");
                                    }
                                    catch (System.Exception ex)
                                    {
                                        WriteLog("Send SMS - " + ex.Message + " - " + (ex.InnerException == null ? "" : ex.InnerException.ToString()));

                                        Console.WriteLine(ex);
                                        lg.AppendLine(" - Unsuccessful.<br/>");
                                    }
                                }
                                if (Convert.ToBoolean(row["ReplyTo"]) == true) // send log to this contact
                                {
                                    SendMail(row["Name"].ToString(), row["Email"].ToString(), "Alert Status", lg.ToString());
                                }
                            }

                        
                            System.Threading.Thread.Sleep(30000);
                        }
                    }
                }

            }
            catch (System.Exception exc)
            {
                WriteLog("System Error " + exc.Message.ToString() + " - INNER:" + (exc.InnerException == null ? "" : exc.InnerException.ToString()));
                SendMail("Mark", "mark.mcgoohan@aurivo.ie", "AlertService Error", exc.Message.ToString() + " - INNER:" + (exc.InnerException == null ? "" : exc.InnerException.ToString()));
            }

            //conn.Close();
        }
            
        //Send Email...
        private void SendMail(string Name, string Email, string Subject, string Body)
        {
            using (MailMessage mm = new MailMessage(comms.EmailAlert, Email))
            {
                mm.Subject = Subject;
                mm.Body = Body;
                mm.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = comms.EmailServer;
                smtp.EnableSsl = comms.SSL;
                NetworkCredential credentials = new NetworkCredential(comms.MailboxUsername, comms.MailboxPassword);
                smtp.Credentials = credentials;
                smtp.Port = comms.PortNumber;
                smtp.Send(mm);
            }

        }
        
        //Send SMS...
        private void SendSMS(string Name, string Phone, string Subject, string Body)
        {
            string vUser = comms.SMSUsername;
            String vPassword = comms.SMSPassword;
            string Api_id = comms.SMSAPIId;

            // Optional + "&SMS-SendAt=" + Format(SendDateTime, "yyyy-mm-dd hh:mm")
            // URL http://bulktext.vodafone.ie/HTTP_API/V1/sendmessage.aspx?user=XXXX&password=XXXXXXXX&api_id=XXXX&to=XXXXXXXXXXXX&text=XXXXXXXXXXXXXX&from=XXXXX

            WebClient client = new WebClient();
            string url = "http://bulktext.vodafone.ie/HTTP_API/V1/sendmessage.aspx?" +
                    "user=" + vUser + "&" +
                    "password=" + vPassword + "&" +
                    "api_id=" + Api_id + "&" +
                    "to=" + Phone + "&" +
                    "text=" + Subject + ": " + Body + "&" +
                    "from=" + vUser;

            WriteLog("Sending SMS - URL - " + url);
            string result = client.DownloadString(url);
            
        }


        private void WriteLog(string msg)
        {
            System.IO.StreamWriter fd = new System.IO.StreamWriter("C:\\alertlog.txt", true);
            fd.WriteLine(msg + " @ " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
            fd.Close();
        }

        //Write to a log file
        /*private void WriteToFile(string text)
        {
            string path = "C:\\ServiceLog.txt";
            using (StreamWriter writer = new StreamWriter(path, true))
            {
                writer.WriteLine(string.Format(text, DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt")));
                writer.Close();
            }
        }*/

    }
}
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using SHLAPI.Database;
using Dapper;
using System.Text;
using SHLAPI.Models;
using SHLAPI;
using System.Net.Mail;
using System.Net;
using System.Threading;
using System.IO;

public class Notifications
{

    public static async Task<Result> CreateNotificationForUsers(IDbConnection db, IDbTransaction trans, List<int> users, string msg, int userId,bool flag=true)
    {
        Result r = new Result();

        if (users.Count > 0)
        {
            List<int> owners = new List<int>();
            // adding eran for debugging users.Add(1);
            foreach (int member in users)
            {
                if ((userId == 0 || userId != member) && !owners.Contains(member) && member != 0)
                {
                    owners.Add(member);
                }
            }
            
            //get all users info
            foreach (int u in owners)
            {
                var user = await UserM.Get(db, trans, u);
                if(user!=null && user.status!=3 && (flag || user.id==1))
                    await SendEmail("تنبيه .. نظام ادارة المهام .. قسم البرمجة", msg, user.name, user.email, /*CC*/"", DateTime.Now);
            }
        }
        return r;
    }

    public static async Task<Result> CreateNotificationForUser(IDbConnection db, IDbTransaction trans, int userId, string msg)
    {
        Result r = new Result();

        var user = await UserM.Get(db, trans, userId);
        if(user!=null && user.status!=3)await SendEmail("تنبيه .. نظام ادارة المهام .. قسم البرمجة", msg, user.name, user.email, /*CC*/"", DateTime.Now);
        return r;
    }

    public static  void SendNotification(ThreadWrapper o)
    {
        SendNotification(o.toEmail, o.subject, o.body, "", null, 3);
    }

    public static async Task<Result> SendEmail(string subject, string msgData, string toName, string toEmail, string CC, DateTime targetDate)
    {
        Result result = new Result();
        ThreadPool.QueueUserWorkItem((r)=>{SendNotification(new ThreadWrapper(){body=msgData,subject=subject,toEmail=toEmail});});
        // Thread t =new Thread(()=>{SendNotification(toEmail, subject, msgData, "");});
        // t.Start();
        // EmailsDatabase _con = new EmailsDatabase();
        // using (var db = _con.Open())
        // {
        //     try
        //     {
        //         if (toEmail != null && toEmail.Trim() != "")
        //         {
        //             StringBuilder messageToSend = new StringBuilder();

        //             string message = "D_CLOCK";
        //             message += "\u0001";
        //             message += "UN";
        //             message += "\u0002";
        //             message += "";//User_name;
        //             message += "\u0001";
        //             message += "TO";
        //             message += "\u0002";
        //             message += toEmail;
        //             message += "\u0001";

        //             message += "CC";
        //             message += "\u0002";
        //             message += CC;
        //             message += "\u0001";

        //             message += "SJT";
        //             message += "\u0002";
        //             message += subject;
        //             message += "\u0001";
        //             message += "MSG";
        //             message += "\u0002";
        //             message += msgData;
        //             message += "\u0001";

        //             if (!toEmail.Contains("@"))
        //             {
        //                 message = msgData;
        //             }

        //             string command = " insert into MailLog(TOName,TOEmail,Data,Sent,Add_Date,Tries,TargetDate,Msgtype)" +
        //                             " Values(@toName,@toEmail,@message," +
        //                             " 'false',@date,0,@targetDate,'TMS')";

        //             await db.ExecuteAsync(command, new { date = DateTime.Now,toName,toEmail,message, targetDate = targetDate }, null);
        //         }
            // }
            // catch (Exception ex)
            // {
            //     throw ex;
            // }
        // }
                return result;
        }

        public static bool SendNotification(string to, string subject, string body, string fileAttachment, List<string> cc, int tries = 3)
        {
            try
            {
                var from = "\"Backup Agent Notification\" <isco.notification@iscosoft.com>";
                var credentials = new NetworkCredential("isco.notification@iscosoft.com", "isc0@s0ft011197");
                bool result = false;
                int triesOrigin = tries;
                while (tries > 0 && !result)
                {
                    tries--;
                    int timeout = (triesOrigin - tries) * 10;//try 1 - 10 sec, try 2 - 20 sec, try 3 - 30 sec ...
                    result = SendNotification(from, to, subject, body, fileAttachment, credentials, cc, timeout);
                }

                return result;
            }
            catch (Exception ex)
            {
                return false;
                // throw ex;
            }
        }

        public static bool SendNotification(string to, string subject, string body, MemoryStream fileAttachment, List<string> cc, int tries = 3)
        {
            try
            {
                var from = "\"Backup Agent Notification\" <isco.notification@iscosoft.com>";
                var credentials = new NetworkCredential("isco.notification@iscosoft.com", "isc0@s0ft011197");
                bool result = false;
                int triesOrigin = tries;
                while (tries > 0 && !result)
                {
                    tries--;
                    int timeout = (triesOrigin - tries) * 10;//try 1 - 10 sec, try 2 - 20 sec, try 3 - 30 sec ...
                    result = SendNotification(from, to, subject, body, fileAttachment, credentials, cc,timeout);
                }

                return result;
            }
            catch
            {
                return false;
                // throw ex;
            }
        }

        private static bool SendNotification(string from, string to, string subject, string body, string fileAttachment, NetworkCredential credentials, List<string> cc, int timeout = 10)
        {
            try
            {
                var client = new SmtpClient();
                client.Port = 587;
                client.Host = "smtp.office365.com";
                client.EnableSsl = true;
                //ServicePointManager.SecurityProtocol = (SecurityProtocolType)48 | (SecurityProtocolType)192 | (SecurityProtocolType)768 | (SecurityProtocolType)3072;
                client.Timeout = (timeout * 1000) + 20 * 1000;//30 - 40 - 50
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = credentials;

                using (var mm = new MailMessage(from, to, subject, body))
                {
                    if(cc!=null)foreach(string _cc in cc)mm.CC.Add(_cc);
                    mm.BodyEncoding = Encoding.UTF8;
                    mm.IsBodyHtml = true;
                    mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

                    if (!string.IsNullOrEmpty(fileAttachment))
                        mm.Attachments.Add(new Attachment(fileAttachment));

                    client.Send(mm);
                }

                return true;
            }
            catch(Exception ex)
            {
                return false;
                //Util.SendException(_config, ex);
                //return false;
                // throw ex;
            }
        }    

        private static bool SendNotification(string from, string to, string subject, string body, MemoryStream fileAttachment, NetworkCredential credentials, List<string> cc , int timeout = 10)
        {
            try
            {
                var client = new SmtpClient();
                client.Port = 587;
                client.Host = "smtp.office365.com";
                client.EnableSsl = true;
                //ServicePointManager.SecurityProtocol = (SecurityProtocolType)48 | (SecurityProtocolType)192 | (SecurityProtocolType)768 | (SecurityProtocolType)3072;
                client.Timeout = (timeout * 1000) + 20 * 1000;//30 - 40 - 50
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = credentials;

                using (var mm = new MailMessage(from, to, subject, body))
                {
                    foreach(string _cc in cc)mm.CC.Add(_cc);
                    mm.BodyEncoding = Encoding.UTF8;
                    mm.IsBodyHtml = true;
                    mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

                    if (fileAttachment!=null)
                    {
                        var attachment = new Attachment(fileAttachment, "xlsxFile.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                        attachment.TransferEncoding = System.Net.Mime.TransferEncoding.Base64;                        
                        mm.Attachments.Add(attachment);
                    }

                    client.Send(mm);
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
                //Util.SendException(_config, ex);
                //return false;
                // throw ex;
            }
        }    

        public class ThreadWrapper
        {
            public string toEmail;
            public string subject;
            public string body;
        }
}
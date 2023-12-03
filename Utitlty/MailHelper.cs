using System.Net.Mail;
using System.Net.Mime;
using System.Net.Security;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Org.BouncyCastle.Utilities;

namespace schoolEnrollment.Utitlty
{
    public class MailHelper
    {
        public static bool SendMail(string SendTo, string CopyTo, string Subject, string Context,byte[] file)
        {
            bool resBool = false;
            try
            {
                string MailServer = "smtp.office365.com";
                //username
                string MailUserName = "autosend2023@outlook.com";
                // password
                string MailPassword = "Test2023";
                // mail name
                string MailName = "Course AutoSend";

                SmtpClient smtpclient = new SmtpClient(MailServer, 587);
                //credentials of sender
                smtpclient.Credentials = new NetworkCredential(MailUserName, MailPassword);
                smtpclient.DeliveryMethod = SmtpDeliveryMethod.Network;
                //SSL connection
                smtpclient.EnableSsl = true;


                //message object
                MailMessage objMailMessage = new MailMessage();
                //set priority
                objMailMessage.Priority = MailPriority.High;
                //sender
                objMailMessage.From = new MailAddress(MailUserName, MailName, Encoding.UTF8);
                //email title
                objMailMessage.Subject = Subject.Trim();
                objMailMessage.To.Add(SendTo);
                if (CopyTo!="")
                {
                    objMailMessage.CC.Add(CopyTo);
                }
                //encoding of email
                objMailMessage.SubjectEncoding = Encoding.UTF8;
                //body
                objMailMessage.Body = Context.Trim();
                objMailMessage.IsBodyHtml = true;
                //内容字符编码
                objMailMessage.BodyEncoding = Encoding.UTF8;

                if (file != null)
                {
                    Stream stream = new MemoryStream(file);
                    Attachment attach = new Attachment(stream, "AcceptanceLetter.pdf");//将文件路径付给Attachment的实例化对象
                    ContentDisposition dispo = attach.ContentDisposition;//获取信息并读写附件
                    dispo.CreationDate = DateTime.Now;
                    dispo.ModificationDate = DateTime.Now;
                    dispo.ReadDate = DateTime.Now;
                    objMailMessage.Attachments.Add(attach);//将附件加入邮件中

                }

                ServicePointManager.ServerCertificateValidationCallback = delegate (Object obj, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors) { return true; };

                //send the email
                smtpclient.Send(objMailMessage);

                resBool = true;

            }
            catch (Exception ex)
            {
                resBool = false;
            }
            return resBool;
        }
    }
}

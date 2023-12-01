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
                //用户名
                string MailUserName = "autosend2023@outlook.com";
                // 密码
                string MailPassword = "Test2023";
                // 名称
                string MailName = "Course AutoSend";

                SmtpClient smtpclient = new SmtpClient(MailServer, 587);
                //构建发件人的身份凭据类
                smtpclient.Credentials = new NetworkCredential(MailUserName, MailPassword);
                smtpclient.DeliveryMethod = SmtpDeliveryMethod.Network;
                //SSL连接
                smtpclient.EnableSsl = true;
                //构建消息类
                MailMessage objMailMessage = new MailMessage();
                //设置优先级
                objMailMessage.Priority = MailPriority.High;

                //消息发送人
                objMailMessage.From = new MailAddress(MailUserName, MailName, Encoding.UTF8);
                //标题
                objMailMessage.Subject = Subject.Trim();
                objMailMessage.To.Add(SendTo);
                if (CopyTo!="")
                {
                    objMailMessage.CC.Add(CopyTo);
                }
                
                //标题字符编码
                objMailMessage.SubjectEncoding = Encoding.UTF8;
                //正文
                //body = body.Replace("\r\n", "<br>");
                //body = body.Replace(@"\r\n", "<br>");
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

                //发送
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

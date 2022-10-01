using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;

namespace EasySplitProject.Security
{
    public class Mail
    {
        /// <summary>
        /// 信箱驗證 GUID
        /// </summary>
        public string Guid { get; set; }

        /// <summary>
        /// 生成信箱驗證連結
        /// </summary>
        /// <param name="urlHost"></param>  
        /// <param name="mailGuid"></param>
        /// <returns></returns>
        public static string SetAuthMailLink(string urlHost, string mailGuid)
        {
            string verifyLink = @"https://" + urlHost + @"/AuthMail/AccountActivation?guid=" + mailGuid;

            return verifyLink;
        }

        /// <summary>
        /// 生成忘記密碼/密碼重設連結
        /// </summary>
        /// <param name="urlHost"></param>
        /// <param name="mailGuid"></param>
        /// <returns></returns>
        public static string SetResetPasswordMailLink(string urlHost, string mailGuid)
        {
            string verifyLink = @"https://" + urlHost + @"/AuthMail/ResetPassword?guid=" + mailGuid;

            return verifyLink;
        }

        /// <summary>
        /// 生成邀請群組連結
        /// </summary>
        /// <param name="urlHost"></param>
        /// <param name="groupGuid"></param>
        /// <returns></returns>
        public static string SendGroupInvitationMailLink(string urlHost, string groupGuid)
        {
            string InvitationLink = @"https://" + urlHost + @"/AuthMail/GroupInvitation?guid=" + groupGuid;

            return InvitationLink;
        }

        /// <summary>
        /// 發送註冊驗證信
        /// </summary>
        /// <param name="toName">收信人名稱</param>
        /// <param name="toAddress">收信地址</param>
        /// <param name="verifyLink">驗證連結</param>
        public static void SendVerifyLinkMail(string toName, string toAddress, string verifyLink)
        {
            string fromAddress = WebConfigurationManager.AppSettings["gmailAccount"];
            string fromName = "EasySplit拆帳趣";
            string title = "會員認證通知";
            string mailAccount = WebConfigurationManager.AppSettings["gmailAccount"];
            string mailPassword = WebConfigurationManager.AppSettings["gmailPassword"];

            //建立建立郵件
            MimeMessage mail = new MimeMessage();
            // 添加寄件者
            mail.From.Add(new MailboxAddress(fromName, fromAddress));
            // 添加收件者
            mail.To.Add(new MailboxAddress(toName, toAddress.Trim()));
            // 設定郵件標題
            mail.Subject = title;
            //使用 BodyBuilder 建立郵件內容
            BodyBuilder bodyBuilder = new BodyBuilder
            {
                HtmlBody =
                "<h1>EasySplit-帳號開通連結</h1>" +
                $"<h3>請點選以下連結進行帳號開通登入，如未開通帳號將無法使用網站進階功能!</h3>" +
                $"<a href='{verifyLink}'>{verifyLink}</a>"
            };
            //設定郵件內容
            mail.Body = bodyBuilder.ToMessageBody(); //轉成郵件內容格式

            using (var client = new SmtpClient())
            {
                //有開防毒時需設定關閉檢查
                client.CheckCertificateRevocation = false;
                //設定連線 gmail ("smtp Server", Port, SSL加密) 
                client.Connect("smtp.gmail.com", 587, false); // localhost 測試使用加密需先關閉 

                // Note: only needed if the SMTP server requires authentication
                client.Authenticate(mailAccount, mailPassword);

                client.Send(mail);
                client.Disconnect(true);
            }
        }

        /// <summary>
        /// 發送重設密碼驗證信
        /// </summary>
        /// <param name="toName">收信人名稱</param>
        /// <param name="toAddress">收信地址</param>
        /// <param name="resetLink">重設連結</param>
        public static void SendResetLinkMail(string toName, string toAddress, string resetLink)
        {
            string fromAddress = WebConfigurationManager.AppSettings["gmailAccount"];
            string fromName = "EasySplit拆帳趣";
            string title = "密碼重設通知";
            string mailAccount = WebConfigurationManager.AppSettings["gmailAccount"];
            string mailPassword = WebConfigurationManager.AppSettings["gmailPassword"];

            //建立建立郵件
            MimeMessage mail = new MimeMessage();
            // 添加寄件者
            mail.From.Add(new MailboxAddress(fromName, fromAddress));
            // 添加收件者
            mail.To.Add(new MailboxAddress(toName, toAddress));
            // 設定郵件標題
            mail.Subject = title;
            //使用 BodyBuilder 建立郵件內容
            BodyBuilder bodyBuilder = new BodyBuilder
            {
                HtmlBody =
                "<h1>EasySplit-密碼重設連結</h1>" +
                $"<h3>請點選以下連結進行密碼重設!</h3>" +
                $"<a href='{resetLink}'>{resetLink}</a>"
            };
            //設定郵件內容
            mail.Body = bodyBuilder.ToMessageBody(); //轉成郵件內容格式

            using (var client = new SmtpClient())
            {
                //有開防毒時需設定關閉檢查
                client.CheckCertificateRevocation = false;
                //設定連線 gmail ("smtp Server", Port, SSL加密) 
                client.Connect("smtp.gmail.com", 587, false); // localhost 測試使用加密需先關閉 
                // Note: only needed if the SMTP server requires authentication
                client.Authenticate(mailAccount, mailPassword);

                client.Send(mail);
                client.Disconnect(true);
            }
        }


        ///// <summary>
        ///// 發送群組邀請信
        ///// </summary>
        ///// <param name="toName">收信人名稱</param>
        ///// <param name="toAddress">收信地址</param>
        ///// <param name="verifyLink">驗證連結</param>
        ///// <param name="groupName">驗證連結</param>
        //public static void SendGroupInviteMail(string toName, string toAddress, string verifyLink,string groupName)
        //{
        //    string fromAddress = WebConfigurationManager.AppSettings["gmailAccount"];
        //    string fromName = "EasySplit拆帳趣";
        //    string title = "群組邀請通知";
        //    string mailAccount = WebConfigurationManager.AppSettings["gmailAccount"];
        //    string mailPassword = WebConfigurationManager.AppSettings["gmailPassword"];

        //    //建立建立郵件
        //    MimeMessage mail = new MimeMessage();
        //    // 添加寄件者
        //    mail.From.Add(new MailboxAddress(fromName, fromAddress));
        //    // 添加收件者
        //    mail.To.Add(new MailboxAddress(toName, toAddress.Trim()));
        //    // 設定郵件標題
        //    mail.Subject = title;
        //    //使用 BodyBuilder 建立郵件內容
        //    BodyBuilder bodyBuilder = new BodyBuilder
        //    {
        //        HtmlBody =
        //        $"<h1>EasySplit-'{groupName}'-群組邀請連結</h1>" +
        //        $"<h3>請點選以下連結加入群組!開始一起拆帳趣~!</h3>" +
        //        $"<a href='{verifyLink}'>{verifyLink}</a>"
        //    };
        //    //設定郵件內容
        //    mail.Body = bodyBuilder.ToMessageBody(); //轉成郵件內容格式

        //    using (var client = new SmtpClient())
        //    {
        //        //有開防毒時需設定關閉檢查
        //        client.CheckCertificateRevocation = false;
        //        //設定連線 gmail ("smtp Server", Port, SSL加密) 
        //        client.Connect("smtp.gmail.com", 587, false); // localhost 測試使用加密需先關閉 

        //        // Note: only needed if the SMTP server requires authentication
        //        client.Authenticate(mailAccount, mailPassword);

        //        client.Send(mail);
        //        client.Disconnect(true);
        //    }
        //}

    }
}
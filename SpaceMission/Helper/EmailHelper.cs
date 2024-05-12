using SpaceMission.WeatherClasses;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Resources;

namespace SpaceMission.Helper
{
    internal class EmailHelper
    {
        ResourceManager rm { get; set; }
        CultureInfo ci { get; set; }
        public EmailHelper(ResourceManager resourceManager, CultureInfo cultureinfo)
        {
            rm = resourceManager;
            ci = cultureinfo;


        }



        public void Email(List<LaunchResult> results)
        {
            Console.WriteLine("Send Email Y/N");
            string answer = Console.ReadLine().ToUpper();

            while (answer == "Y")
            {
                Console.WriteLine(rm.GetString("SenderEmail", ci));
                var senderEmail = Console.ReadLine();
                Console.WriteLine(rm.GetString("SenderPassword", ci));
                var senderPassword = Console.ReadLine();
                Console.WriteLine(rm.GetString("RecipientEmail", ci));
                var recipientEmail = Console.ReadLine();
                Console.WriteLine(rm.GetString("AdditionalMessage", ci));
                var additionalMessage = Console.ReadLine();
                string bestCombination = string.Format(ci, rm.GetString("BestDay", ci), results[0].BestLaunchDay, results[0].Spaceport);

                if (SendEmail(senderEmail, senderPassword, recipientEmail, bestCombination, "LaunchAnalysisReport.csv", additionalMessage))
                {
                    break; // Exit the loop on successful email send
                }
                else
                {
                    Console.WriteLine(rm.GetString("FailSendEmail", ci));
                    answer = Console.ReadLine().ToUpper();
                    if (answer != "Y") break; // Exit loop if user does not want to try again
                }
            }

            if (answer != "Y")
            {
                Console.WriteLine(rm.GetString("EmailCanceled", ci));
            }
        }

        public bool SendEmail(string senderEmail, string senderPassword, string recipientEmail, string emailBody, string attachmentFilename, string additionalMessage)
        {
            using (var client = new SmtpClient("smtp-mail.outlook.com", 587))
            {
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(senderEmail, senderPassword);
                try
                {
                    using (var mailMessage = new MailMessage(senderEmail, recipientEmail))
                    {
                        mailMessage.Subject = rm.GetString("SpaceAnalysisReport", ci);
                        mailMessage.Body = $"{emailBody}\n\n{additionalMessage}";
                        mailMessage.Attachments.Add(new Attachment(attachmentFilename));


                        client.Send(mailMessage);
                        Console.WriteLine(rm.GetString("EmailSendW", ci));
                        return true;

                    }
                }
                catch (FormatException)
                {
                    Console.WriteLine(rm.GetString("EmailFormat",ci));
                    return false;
                }
                catch (Exception ex)
                {

                    return false;
                }
            }
            
        }
    }
}

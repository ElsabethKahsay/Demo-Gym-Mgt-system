using DocumentFormat.OpenXml.Wordprocessing;
using SoulFitness.Abstractions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SoulFitness.Utilities
{
    public class EmailConfiguration
    {
        public string FromEmail { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
    }

    public enum OperationStatus
    {
        Ok = 1,
        NOT_OK,
        SUCCESS,
        ERROR
    }

    public class OperationResult
    {
        public OperationStatus Status { get; set; }
        public string Message { get; set; }
        public string StatusAPI { get; set; }
        public string MessageAPI { get; set; }
        public Exception ex { get; set; }
        public List<string> ErrorList { get; set; }
        public DateTime Date { get { return DateTime.UtcNow; } }

        public OperationResult()
        {
            ErrorList = new List<string>();
        }
    }

    public class EmailSender : IEmailSender
    {
        public bool IsEmailValid(string email)
        {
            string pattern = "^([0-9a-zA-Z]([-\\.\\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\\w]*[0-9a-zA-Z]\\.)+[a-zA-Z]{2,9})$";
            return !string.IsNullOrEmpty(email) && Regex.IsMatch(email, pattern);
        }

        public object SendEmail(string message, List<string> receivers, List<string> emailsInCopy, string subject, string notificationType)
        {
             try
            {
                if (receivers?.Count > 0)
                {
                    EmailSenderConfiguration appConfiguration = new();

                    MailMessage emailMessage = new();
                    emailMessage.IsBodyHtml = true;
                    emailMessage.From = new MailAddress("DHSNotification@ethiopianairlines.com");

                    foreach (var reciever in receivers)
                        emailMessage.To.Add(new MailAddress(reciever.Trim()));

                    if (emailsInCopy != null && emailsInCopy.Count != 0)
                    {
                        foreach (var copy in emailsInCopy)
                            emailMessage.CC.Add(new MailAddress(copy.Trim()));
                    }

                    emailMessage.Body = message;
                    emailMessage.Subject = subject;

                    SmtpClient smtpClient = new()
                    {
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential("DHSNotification@ethiopianairlines.com", "4152@Asdf"),
                        Port = 587,
                        Host = "mail.ethiopianairlines.com",
                        DeliveryMethod = SmtpDeliveryMethod.Network
                        //EnableSsl = true
                    };

                    smtpClient.Send(emailMessage);

                    return new OperationResult
                    {
                        Status = OperationStatus.SUCCESS,
                        Message = Enum.GetName(typeof(OperationStatus), OperationStatus.Ok)
                    };
                }

                return new OperationResult
                {
                    Status = OperationStatus.NOT_OK,
                    Message = "Reciever email address is not given."
                };
            }
            catch (Exception ex)
            {
                return new OperationResult
                {
                    ex = ex,
                    Message = "Exception Occured on SendEmail",
                    Status = OperationStatus.ERROR
                };
            }
        }
    }
}

            //    EmailSenderConfiguration appConfiguration = new();

            //    MailMessage emailMessage = new();
            //    emailMessage.IsBodyHtml = true;
            //    emailMessage.From = new MailAddress("Elsabethtesfay9@gmail.com");

            //    foreach (var reciever in receivers)
            //        emailMessage.To.Add(new MailAddress(reciever.Trim()));

            //    if (emailsInCopy != null && emailsInCopy.Count != 0)
            //    {
            //        foreach (var copy in emailsInCopy)
            //            emailMessage.CC.Add(new MailAddress(copy.Trim()));
            //    }
            //    emailMessage.Body = message;
            //    emailMessage.Subject = subject;

            //    SmtpClient smtpClient = new()
            //    {
            //        UseDefaultCredentials = false,
            //        Credentials = new NetworkCredential("Elsabethtesfay9@gmail.com", "hbgxdhcbovvjvhmx"),
            //        Port = 587,
            //        Host = "smtp.gmail.com",
            //        DeliveryMethod = SmtpDeliveryMethod.Network,
            //        EnableSsl = true
            //    };

            //    smtpClient.Send(emailMessage);

            //    return new OperationResult
            //    {
            //        Status = OperationStatus.SUCCESS,
            //        Message = Enum.GetName(typeof(OperationStatus), OperationStatus.Ok)
            //    };
            //}

            //return new OperationResult
            //{
            //    Status = OperationStatus.NOT_OK,
            //    Message = "Reciever email address is not given."
            //};
        
            //catch (Exception ex)
            //{
            //    return new OperationResult
            //    {
            //        ex = ex,
            //        Message = "Exception Occured on SendEmail",
            //        Status = OperationStatus.ERROR
            //    };
            //}
        
           


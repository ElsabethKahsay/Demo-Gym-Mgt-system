using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SoulFitness.Abstractions
{
    public interface IEmailSender
    {
        object SendEmail(string message, List<string> reciever, List<string> emailsInCopy, string subject, string notificationType);
    }
}

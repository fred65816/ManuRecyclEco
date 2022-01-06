using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ManuRecyEco.Utility
{
    public static class EmailUtil
    {
        public static void SendTokenEmail(string addressTo, string token, SendCompletedEventHandler SendCompletedHandler)
        {
            _ = ThreadPool.QueueUserWorkItem(t =>
              {
                  // Body du courriel
                  string body = "<h2>Jeton aléatoire</h2>";
                  body += "Veuillez taper ou copier/coller ce jeton dans l'application ManuRecyclEco:<br><b>" + token + "</b><br><br>";
                  body += "Merci d'utiliser nos services!";

                  // serveur SMTP
                  SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
                  SmtpServer.Port = 587;
                  SmtpServer.Credentials = new System.Net.NetworkCredential("email@gmail.com", "Password1");
                  SmtpServer.EnableSsl = true;

                  // courriel
                  MailMessage email = new MailMessage("email@gmail.com", addressTo);
                  email.Subject = "Confirmation du courriel";
                  email.IsBodyHtml = true;
                  email.Body = body;

                  SmtpServer.SendCompleted += SendCompletedHandler;

                  // envoie du courriel en async
                  SmtpServer.SendAsync(email, email);
              });
        }

        // génération du token aléatoire de 16 caractères
        public static string GenerateToken()
        {
            string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            char[] charArray = new char[16];
            Random random = new Random();

            for (int i = 0; i < charArray.Length; i++)
            {
                int number = random.Next(characters.Length);
                charArray[i] = characters[number];
            }

            return new String(charArray);
        }
    }
}

using NicheNameJacker.Commands;
using NicheNameJacker.Common;
using NicheNameJacker.Utilities;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;

namespace NicheNameJacker.ViewModels
{
    public class AboutViewModel : ObservableBase
    {
        public string Version { get; } = AssemblyInfo.VersionNumber;

        public ICommand ContactSupportCommand => new RelayCommand(() => Process.Start(Urls.Support));

        public ICommand SaveErrorLogCommand => new RelayCommand(() =>
        {
            var filename = StandardDialogs.RequestTxtSaveFilename("ErrorLog");
            if (filename != null)
            {
                File.Create(filename).Dispose();
                File.AppendAllText(filename, Logger.GetLog());
            }
        });

        //public ICommand SendErrorReportCommand => new RelayCommand(() => SendLogReport());

        //void SendLogReport()
        //{
        //    var smtp = new SmtpClient();
        //    var mail = new MailMessage("bugs@nichenamejacker.com", "bugs@nichenamejacker.com");

        //    mail.Subject = "Log Report";
        //    mail.Body = Logger.GetLog();

        //    if (Debugger.IsAttached)
        //    {
        //        smtp = new SmtpClient()
        //        {
        //            Host = "localhost",
        //            Port = 25,
        //            EnableSsl = false,
        //            DeliveryMethod = SmtpDeliveryMethod.Network,
        //            UseDefaultCredentials = false
        //            //Credentials = new NetworkCredential(mail.From.Address, "w70347w34$")
        //        };
        //    }
        //    else
        //    {
        //        smtp = new SmtpClient()
        //        {
        //            Host = "host.nichejacker.com",
        //            Port = 993,
        //            EnableSsl = true,
        //            DeliveryMethod = SmtpDeliveryMethod.Network,
        //            UseDefaultCredentials = false,
        //            Credentials = new NetworkCredential(mail.From.Address, "w70347w34$")
        //        };
        //    }

        //    try
        //    {
        //        smtp.SendAsync(mail, null);
        //    }
        //    catch (Exception e)
        //    {
        //        Logger.LogError(e.Message);
        //    }
        //}
    }
}

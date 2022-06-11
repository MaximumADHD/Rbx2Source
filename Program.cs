using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;

namespace Rbx2Source
{
    static class Program
    {
        private static bool validateCert(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors errors)
        {
            return (errors == SslPolicyErrors.None);
        }

        [STAThread]
        static void Main()
        {
            var httpsValidator = new RemoteCertificateValidationCallback(validateCert);
            ServicePointManager.ServerCertificateValidationCallback += httpsValidator;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Main());
        }
    }
}

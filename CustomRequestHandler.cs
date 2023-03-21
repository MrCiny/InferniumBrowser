using CefSharp;
using CefSharp.Handler;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using InferniumBrowser;

namespace InferniumBrowser
{
    class CustomRequestHandler : RequestHandler
    {
        //Metode, kas dod iespēju lietotājam izvēlēties vai turpināt apmeklēt lapu, vai neapmeklēt
        protected override bool OnCertificateError(IWebBrowser chromiumWebBrowser, IBrowser browser, CefErrorCode errorCode, string requestUrl, ISslInfo sslInfo, IRequestCallback callback)
        {
            Task.Run(() =>
            {
                //Pārbauda vai "callback" nav iznīcināts
                if (!callback.IsDisposed)
                {
                    using (callback)
                    {
                        string title = "Brīdinājums!";
                        string message = $"Lapai, kuru mēģināt apmeklēt, nav kaut kas kārtībā! SSL Sertifikāta kļūda: {errorCode}; Vai joprojām vēlaties apmeklēt lapu?";
                        MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                        DialogResult result = MessageBox.Show(message, title, buttons);
                        if(result == DialogResult.Yes)
                        {
                            callback.Continue(true);
                        }
                        else if(result == DialogResult.No)
                        {
                            callback.Dispose();
                        }
                    }
                }
            });

            return true;
        }
    }
}

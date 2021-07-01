using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;

namespace RDC.Plugins.ChromeZone.Core
{
    public static class InstallCheck
    {
        public static string GetWebView2Version()
        {
            try
            {
                return CoreWebView2Environment.GetAvailableBrowserVersionString();
            }
            catch (Exception) { return ""; }
        }
    }
}

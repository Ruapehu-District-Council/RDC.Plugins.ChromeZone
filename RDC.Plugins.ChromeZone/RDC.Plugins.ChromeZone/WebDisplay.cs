using CefSharp;
using CefSharp.WinForms;
using CefSharp.WinForms.Internals;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RDC.Plugins.ChromeZone
{
    public partial class WebDisplay : DisplayComponents
    {
        private ChromiumWebBrowser cBrowser;

        static string lib, browserExe, locales, res;

        private string AddressURL = "";

        public WebDisplay()
        {
            InitializeComponent();
            SetupBrowser();

            forwardButton.Click += forwardButton_Click;
            backButton.Click += backButton_Click;
            refreshButton.Click += toolStripButton1_Click;
            homeButton.Click += HomeButton_Click;
        }

        private void HomeButton_Click(object sender, EventArgs e)
        {
            cBrowser.Load(BuildURL());
        }

        //Ozone Initialize function
        public override void Initialize(string InitData)
        {
            AddressURL = InitData;
            cBrowser.Load(BuildURL());
        }

        public override void RecordRefresh(string InitData = "")
        {
            cBrowser.Load(BuildURL());
        }

        //Setup the browser and it's dll directories 
        private void SetupBrowser()
        {
            lib = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"cefsharp\libcef.dll");
            browserExe = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"cefsharp\CefSharp.BrowserSubprocess.exe");
            locales = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"cefsharp\locales\");
            res = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"cefsharp\");

            var libraryLoader = new CefLibraryHandle(lib);

            var settings = new CefSettings();
            settings.BrowserSubprocessPath = browserExe;
            settings.LocalesDirPath = locales;
            settings.ResourcesDirPath = res;

            Cef.Initialize(settings);

            cBrowser = new ChromiumWebBrowser("about:blank")
            {
                Dock = DockStyle.Fill,
            };
            toolStripContainer.ContentPanel.Controls.Add(cBrowser);

            cBrowser.AddressChanged += OnBrowserAddressChanged;
        }

        private void OnBrowserAddressChanged(object sender, AddressChangedEventArgs args)
        {
            this.InvokeOnUiThreadIfRequired(() => urlTextBox.Text = args.Address);
        }

        private void backButton_Click(object sender, EventArgs e)
        {
            cBrowser.Back();
        }

        private void forwardButton_Click(object sender, EventArgs e)
        {
            cBrowser.Forward();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            cBrowser.Reload();
        }

        public string BuildURL()
        {
            string URL = AddressURL;

            URL = URL.Replace("@@BusinessObjectID@@", BusinessObjectID);
            URL = URL.Replace("@@RecordID@@", RecordID);
            URL = URL.Replace("@@ScreenID@@", ScreenID);

            return URL;
        }

        private string BusinessObjectID
        {
            get
            {
                if (this.CommonBlock != null && this.CommonBlock.CurrentRecord != null)
                {
                    return this.CommonBlock.CurrentRecord.FileId;
                }
                return "";
            }
        }

        private string RecordID
        {
            get
            {
                if (this.CommonBlock != null && this.CommonBlock.CurrentRecord != null)
                {
                    return this.CommonBlock.CurrentRecord.RecordId;
                }
                return "";
            }
        }

        private string ScreenID
        {
            get
            {
                if (this.CommonBlock != null && this.CommonBlock.CurrentRecord != null)
                {
                    return this.CommonBlock.Function;
                }
                return "";
            }
        }
    }
}

using CefSharp;
using CefSharp.Handler;
using CefSharp.WinForms;
using CefSharp.WinForms.Internals;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace RDC.Plugins.ChromeZone
{
    public partial class WebDisplay : DisplayComponents
    {
        private ChromiumWebBrowser cBrowser;

        static string lib, browserExe, locales, res;

        private bool RefreshOnScreenChange = false;
        private bool RefreshOnRecordLoad = true;
        private bool BlockRefreshAfterNavigateAway = true;

        private string CefsharpFolderLocation = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"cefsharp\");

        private bool HasNavigatedAway = false;

        private string AddressURL = string.Empty;

        private string TabName = string.Empty;

        private string ScreenID;
        private string _Subject;

        public WebDisplay()
        {
            TryLoadSettings();
            var libraryLoader = new CefLibraryHandle(lib);

            InitializeComponent();

            forwardButton.Click += forwardButton_Click;
            backButton.Click += backButton_Click;
            refreshButton.Click += refreshButton_Click;
            homeButton.Click += HomeButton_Click;

            //libraryLoader.Dispose();
        }

        private void TryLoadSettings()
        {
            string value = System.Configuration.ConfigurationManager.AppSettings["ConfigFolderPath"];
            string SettingsFile = Path.Combine(value, @"ChromeZoneSettings.xml");

            if (!File.Exists(SettingsFile))
            {
                return;
            }

            try
            {
                XmlDocument RootDocument = new XmlDocument();
                RootDocument.LoadXml(File.ReadAllText(SettingsFile));

                XmlNode CefsharpFolderLocationNode = RootDocument.SelectSingleNode("//Settings/CefsharpFolderLocation");
                if (CefsharpFolderLocationNode != null)
                {
                    CefsharpFolderLocation = CefsharpFolderLocationNode.InnerText;
                }
            }
            catch
            {

            }

            lib = Path.Combine(CefsharpFolderLocation, @"libcef.dll");
            browserExe = Path.Combine(CefsharpFolderLocation, @"CefSharp.BrowserSubprocess.exe");
            locales = Path.Combine(CefsharpFolderLocation, @"locales\");
            res = CefsharpFolderLocation;
            
        }

        private void LoadTabData()
        {
            string value = System.Configuration.ConfigurationManager.AppSettings["ConfigFolderPath"];
            string SettingsFile = Path.Combine(value, @"ChromeZoneSettings.xml");

            if (!File.Exists(SettingsFile))
            {
                return;
            }

            try
            {
                XmlDocument RootDocument = new XmlDocument();
                RootDocument.LoadXml(File.ReadAllText(SettingsFile));

                XmlNode Document = RootDocument.SelectSingleNode("//Settings/Tabs/Tab[@Name='" + TabName + "']");

                XmlNode RefreshOnScreenChangeNode = Document.SelectSingleNode("RefreshOnScreenChange");
                if (RefreshOnScreenChangeNode != null)
                {
                    RefreshOnScreenChange = bool.Parse(RefreshOnScreenChangeNode.InnerText);
                }

                XmlNode RefreshOnRecordLoadNode = Document.SelectSingleNode("RefreshOnRecordLoad");
                if (RefreshOnRecordLoadNode != null)
                {
                    RefreshOnRecordLoad = bool.Parse(RefreshOnRecordLoadNode.InnerText);
                }

                XmlNode BlockRefreshAfterNavigateAwayNode = Document.SelectSingleNode("BlockRefreshAfterNavigateAway");
                if (BlockRefreshAfterNavigateAwayNode != null)
                {
                    BlockRefreshAfterNavigateAway = bool.Parse(BlockRefreshAfterNavigateAwayNode.InnerText);
                }

                XmlNode URLNode = Document.SelectSingleNode("URL");
                if (URLNode != null)
                {
                    AddressURL = URLNode.InnerText;
                }
            }
            catch
            {

            }

        }

        //Setup the browser and it's dll directories 
        private void SetupBrowser()
        {
            CefSettings settings = new CefSettings
            {
                BrowserSubprocessPath = browserExe,
                LocalesDirPath = locales,
                ResourcesDirPath = res
            };

            if (Cef.IsInitialized == false)
            {
                Cef.Initialize(settings);
            }

            cBrowser = new ChromiumWebBrowser("about:blank")
            {
                Dock = DockStyle.Fill,
            };
            toolStripContainer.ContentPanel.Controls.Add(cBrowser);

            cBrowser.RequestHandler = new CustomRequestHandler();
            cBrowser.AddressChanged += OnBrowserAddressChanged;
            cBrowser.LoadingStateChanged += CBrowser_LoadingStateChanged;
            cBrowser.LoadError += CBrowser_LoadError;
            
        }

        private void CBrowser_LoadError(object sender, LoadErrorEventArgs e)
        {
            //System.Windows.Forms.MessageBox.Show(e.FailedUrl + " " + e.ErrorText + " "  + e.ErrorCode);
        }

        public class CustomResourceRequestHandler : ResourceRequestHandler
        {

            protected override bool OnProtocolExecution(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request)
            {
                if (request.Url.Contains("Http") || request.Url.Contains("Https"))
                {
                    return false;
                }

                if (browser.IsPopup)
                {
                    browser.CloseBrowser(false);
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("Frame URL: " + frame.Url + " Request URL: " + request.Url);
                    frame.LoadUrl(frame.Url);
                }
                return true;
            }
        }

        public class CustomRequestHandler : RequestHandler
        {
            protected override IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
            {
                return new CustomResourceRequestHandler();
            }
        }

        //Ozone Initialize function
        public override void Initialize(string InitData)
        {
            base.Initialize(InitData);

            TabName = InitData;

            if (AddressURL == string.Empty)
            {
                AddressURL = InitData;
            }

            LoadTabData();

            SetupBrowser();

            LoadCustomPage();
        }

        public override void RecordRefresh(string InitData = "")
        {
            if (RefreshOnRecordLoad)
            {
                LoadCustomPage();
            }
        }

        public override void FunctionChanged(string FunctionID, string SubjectID)
        {
            ScreenID = FunctionID;
            _Subject = SubjectID;

            if (RefreshOnScreenChange)
            {
                LoadCustomPage();
            }
        }


        private void CBrowser_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            InvokeOnUiThreadIfRequired(() =>
            {
                if (e.IsLoading)
                {
                    Waiting = true;
                    ProgressIndicatorVisible = true;

                }
                else if (e.IsLoading == false)
                {
                    Waiting = false;
                    ProgressIndicatorVisible = false;
                }
            });
        }

        private void OnBrowserAddressChanged(object sender, AddressChangedEventArgs args)
        {
            InvokeOnUiThreadIfRequired(() => urlTextBox.Text = args.Address);

            if (args.Address.ToLower() != CustomURL.ToLower())
            {
                HasNavigatedAway = true;
            }
            else
            {
                HasNavigatedAway = false;
            }
        }

        private void HomeButton_Click(object sender, EventArgs e)
        {
            cBrowser.Load(CustomURL);
            HasNavigatedAway = false;
        }

        private void backButton_Click(object sender, EventArgs e)
        {
            cBrowser.Back();
        }

        private void forwardButton_Click(object sender, EventArgs e)
        {
            cBrowser.Forward();
        }

        private void refreshButton_Click(object sender, EventArgs e)
        {
            cBrowser.Reload();
        }

        private void LoadCustomPage()
        {
            if (BlockRefreshAfterNavigateAway && HasNavigatedAway)
            {
                return;
            }

            cBrowser.Load(CustomURL);
        }

        private string CustomURL
        {
            get
            {
                string URL = AddressURL;

                URL = URL.Replace("@@BusinessObjectID@@", BusinessObjectID);
                URL = URL.Replace("@@RecordID@@", RecordID);
                URL = URL.Replace("@@ScreenID@@", ScreenID);
                URL = URL.Replace("@@Subject@@", _Subject);

                return URL;
            }
        }

        private string BusinessObjectID
        {
            get
            {
                if (this.CommonBlock != null && this.CommonBlock.CurrentRecord != null)
                {
                    string id = this.CommonBlock.CurrentRecord.FileId;

                    id = id.Replace(".", "_");

                    return id;
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

        public void InvokeOnUiThreadIfRequired(Action action)
        {
            if (InvokeRequired)
            {
                BeginInvoke(action);
            }
            else
            {
                action.Invoke();
            }
        }
    }
}

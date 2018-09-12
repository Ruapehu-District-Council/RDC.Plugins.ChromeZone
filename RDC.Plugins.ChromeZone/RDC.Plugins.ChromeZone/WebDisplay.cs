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

        private string ScreenID;
        private string _Subject;

        public WebDisplay()
        {
            InitializeComponent();

            forwardButton.Click += forwardButton_Click;
            backButton.Click += backButton_Click;
            refreshButton.Click += toolStripButton1_Click;
            homeButton.Click += HomeButton_Click;

            TryLoadSettings();

            SetupBrowser();
        }

        private void TryLoadSettings()
        {
            string SettingsFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"ChromeZoneSettings.xml");

            if (!File.Exists(SettingsFile))
            {
                return;
            }

            try
            {
                XmlDocument Document = new XmlDocument();
                Document.LoadXml(File.ReadAllText(SettingsFile));

                XmlNode RefreshOnScreenChangeNode = Document.SelectSingleNode("//Settings/RefreshOnScreenChange");
                if (RefreshOnScreenChangeNode != null)
                {
                    RefreshOnScreenChange = bool.Parse(RefreshOnScreenChangeNode.InnerText);
                }

                XmlNode RefreshOnRecordLoadNode = Document.SelectSingleNode("//Settings/RefreshOnRecordLoad");
                if (RefreshOnRecordLoadNode != null)
                {
                    RefreshOnRecordLoad = bool.Parse(RefreshOnRecordLoadNode.InnerText);
                }

                XmlNode BlockRefreshAfterNavigateAwayNode = Document.SelectSingleNode("//Settings/BlockRefreshAfterNavigateAway");
                if (BlockRefreshAfterNavigateAwayNode != null)
                {
                    BlockRefreshAfterNavigateAway = bool.Parse(BlockRefreshAfterNavigateAwayNode.InnerText);
                }

                XmlNode CefsharpFolderLocationNode = Document.SelectSingleNode("//Settings/CefsharpFolderLocation");
                if (CefsharpFolderLocationNode != null)
                {
                    CefsharpFolderLocation = CefsharpFolderLocationNode.InnerText;
                }

                XmlNode URLNode = Document.SelectSingleNode("//Settings/URL");
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
            lib = Path.Combine(CefsharpFolderLocation, @"libcef.dll");
            browserExe = Path.Combine(CefsharpFolderLocation, @"CefSharp.BrowserSubprocess.exe");
            locales = Path.Combine(CefsharpFolderLocation, @"locales\");
            res = CefsharpFolderLocation;

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
            cBrowser.LoadingStateChanged += CBrowser_LoadingStateChanged;
        }

        //Ozone Initialize function
        public override void Initialize(string InitData)
        {
            if (AddressURL == string.Empty)
            {
                AddressURL = InitData;
            }

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
            this.InvokeOnUiThreadIfRequired(() =>
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
            this.InvokeOnUiThreadIfRequired(() => urlTextBox.Text = args.Address);

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

        private void toolStripButton1_Click(object sender, EventArgs e)
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
    }
}

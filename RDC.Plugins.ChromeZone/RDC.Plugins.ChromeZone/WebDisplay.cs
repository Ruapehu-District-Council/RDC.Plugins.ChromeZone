using CefSharp;
using CefSharp.Handler;
using CefSharp.WinForms;
using CefSharp.WinForms.Internals;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Xml;

namespace RDC.Plugins.ChromeZone
{
    public partial class WebDisplay : DisplayComponents
    {
        private ChromiumWebBrowser cBrowser;

        static string lib, browserExe, locales, res;

        private string CefsharpFolderLocation = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"cefsharp\");

        private bool HasNavigatedAway = false;

        private string AddressURL = string.Empty;

        private string TabName = string.Empty;

        private string ScreenID;
        private string _Subject;

        public Core.Objects.WebTab WebTab
        {
            get
            {
                var CurrentTab = Core.Settings.SettingsWrapper.WebTabs.Find(t => t.Name == TabName);

                return CurrentTab;
            }
        }

        public WebDisplay()
        {
            TryLoadSettings();
            var libraryLoader = new CefLibraryHandle(lib);

            InitializeComponent();

            forwardButton.Click += forwardButton_Click;
            backButton.Click += backButton_Click;
            refreshButton.Click += refreshButton_Click;
            homeButton.Click += HomeButton_Click;
        }

        private void TryLoadSettings()
        {
            if (!Core.Settings.LoadSettings())
            {
                return;
            }

            if (Core.Settings.SettingsWrapper.CefsharpFolderLocation != string.Empty)
            {
                CefsharpFolderLocation = Core.Settings.SettingsWrapper.CefsharpFolderLocation;
            }

            lib = Path.Combine(CefsharpFolderLocation, @"libcef.dll");
            browserExe = Path.Combine(CefsharpFolderLocation, @"CefSharp.BrowserSubprocess.exe");
            locales = Path.Combine(CefsharpFolderLocation, @"locales\");
            res = CefsharpFolderLocation;
            
        }

        //Setup the browser and it's dll directories 
        private void SetupBrowser()
        {
            var settings = new CefSettings
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

            System.Diagnostics.Debugger.Launch();

            TabName = InitData;

            AddressURL = WebTab.DefaultURL;

            if (AddressURL == string.Empty)
            {
                AddressURL = InitData;
            }

            if (WebTab == null)
            {
                return;
            }

            SetupBrowser();

            LoadCustomPage();
        }

        public override void RecordRefresh(string InitData = "")
        {
            if (WebTab.RefreshOnRecordLoad)
            {
                LoadCustomPage();
            }
        }

        public override void FunctionChanged(string FunctionID, string SubjectID)
        {
            ScreenID = FunctionID;
            _Subject = SubjectID;

            if (WebTab.RefreshOnScreenChange)
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

            HasNavigatedAway = args.Address.ToLower() != BuildCustomURL(AddressURL).ToLower();
        }

        private void HomeButton_Click(object sender, EventArgs e)
        {
            cBrowser.Load(BuildCustomURL(AddressURL));
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

        private string GetRecordAttribute(int attributeNumber)
        {
            var result = string.Empty;

            if (CommonBlock == null)
            {
                return result;
            }

            var att = CommonBlock.CurrentRecord.GetAttribute(attributeNumber);

            if (att != null && att.Count == 1 && att.Contains(1))
            {
                var value = att[1].ToString();

                value = value.Replace("*", "-");
                return value;
            }

            return result;
        }

        private void LoadCustomPage()
        {
            if (WebTab.BlockRefreshAfterNavigateAway && HasNavigatedAway)
            {
                return;
            }

            cBrowser.Load(BuildCustomURL(AddressURL));
        }

        private string BuildCustomURL(string BaseURL)
        {
            var CustomURL = GetCustomUrl();
            if (CustomURL != string.Empty)
            {
                BaseURL = CustomURL;
            }

            BaseURL = BaseURL.Replace("@@BusinessObjectID@@", BusinessObjectID);
            BaseURL = BaseURL.Replace("@@RecordID@@", RecordID);
            BaseURL = BaseURL.Replace("@@ScreenID@@", ScreenID);
            BaseURL = BaseURL.Replace("@@Subject@@", _Subject);

            if (BaseURL.Contains("**"))
            {
                try
                {
                    var RecordIDParts = BaseURL.Split(new[] {"**"}, StringSplitOptions.None);

                    for (int i = 0; RecordIDParts.Length > i; i++)
                    {
                        i++;
                        var attrID = RecordIDParts[i];
                        if (int.TryParse(attrID, out int result))
                        {
                            var value = GetRecordAttribute(result);
                            BaseURL = BaseURL.Replace($"**{attrID}**", value);
                        }

                        i++;
                    }
                }
                catch (Exception e)
                {
                    e.ToString();
                }
            }

            return BaseURL;
        }

        private string GetCustomUrl()
        {
            foreach (var rule in WebTab.URLRules)
            {
                switch (rule.Field)
                {
                    case "BusinessObjectID":
                        if (BusinessObjectID == rule.Match)
                        {
                            return rule.URL;
                        }
                        break;

                    case "ScreenID":
                        break;
                        
                    case "Subject":
                        break;
                }
            }

            return string.Empty;
        }

        private string BusinessObjectID
        {
            get
            {
                if (CommonBlock?.CurrentRecord != null)
                {
                    var id = this.CommonBlock.CurrentRecord.FileId;

                    //id = id.Replace(".", "_");

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
                    var record = this.CommonBlock.CurrentRecord.RecordId;

                    record = record.Replace("*", "-");

                    return record;
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

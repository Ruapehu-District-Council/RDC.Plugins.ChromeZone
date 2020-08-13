using CefSharp;
using CefSharp.Handler;
using CefSharp.WinForms;
using CefSharp.WinForms.Internals;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Xml;
using RDC.Plugins.ChromeZone.Core.Objects;

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
            new CefLibraryHandle(lib);

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
            if (e.ErrorCode == CefErrorCode.FileNotFound && e.FailedUrl != WebTab.DefaultURL)
            {
                cBrowser.Load(BuildCustomUrl(WebTab.DefaultURL));
            }
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

            //System.Diagnostics.Debugger.Launch();

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

            HasNavigatedAway = args.Address.ToLower() != BuildCustomUrl(AddressURL).ToLower();
        }

        private void HomeButton_Click(object sender, EventArgs e)
        {
            cBrowser.Load(BuildCustomUrl(AddressURL));
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

        public string BuildCustomUrl(string baseUrl)
        {
            var customUrl = GetCustomUrl();

            var adjustedValues = new Dictionary<string, string>();

            if (customUrl != null)
            {
                baseUrl = customUrl.URL;

                try
                {
                    HandleFieldConversions(adjustedValues, customUrl.FieldConversions);

                    HandleFieldRules(adjustedValues, customUrl.FieldRules);
                }
                catch
                {

                }
            }

            baseUrl = adjustedValues.Aggregate(baseUrl, (current, keyValuePair) => current.Replace(keyValuePair.Key, keyValuePair.Value));

            baseUrl = baseUrl.Replace("@@BusinessObjectID@@", BusinessObjectID);
            baseUrl = baseUrl.Replace("@@RecordID@@", RecordID);
            baseUrl = baseUrl.Replace("@@ScreenID@@", ScreenID);
            baseUrl = baseUrl.Replace("@@Subject@@", _Subject);

            if (baseUrl.Contains("**"))
            {
                var attributeIDs = Core.Logic.GetAttributeIDs(baseUrl);

                attributeIDs.ForEach(i =>
                {
                    var replaceName = $"**{i}**";
                    baseUrl = baseUrl.Replace(replaceName, GetRecordAttribute(i));
                });
            }

            return baseUrl;
        }

        private void HandleFieldConversions(IDictionary<string, string> adjustedValues, List<FieldConversion> fieldConversions)
        {
            fieldConversions.ForEach(conversion =>
            {
                if (conversion.FieldName.Contains("@@"))
                {
                    switch (conversion.FieldName)
                    {
                        case "@@BusinessObjectID@@":
                            adjustedValues.Add(conversion.FieldName, Core.Logic.HandleFieldConversion(BusinessObjectID, conversion));
                            break;
                        case "@@RecordID@@":
                            adjustedValues.Add(conversion.FieldName, Core.Logic.HandleFieldConversion(RecordID, conversion));
                            break;
                        case "@@ScreenID@@":
                            adjustedValues.Add(conversion.FieldName, Core.Logic.HandleFieldConversion(ScreenID, conversion));
                            break;
                        case "@@Subject@@":
                            adjustedValues.Add(conversion.FieldName, Core.Logic.HandleFieldConversion(_Subject, conversion));
                            break;
                    }
                }
                else if (conversion.FieldName.Contains("**"))
                {
                    var fieldId = Core.Logic.GetAttributeID(conversion.FieldName);
                    if (fieldId != -1)
                    {
                        var fieldValue = GetRecordAttribute(fieldId);
                        adjustedValues.Add(conversion.FieldName, Core.Logic.HandleFieldConversion(fieldValue, conversion));
                    }
                }
            });
        }

        private void HandleFieldRules(IDictionary<string, string> adjustedValues, List<FieldRule> fieldRules)
        {
            fieldRules.ForEach(rule =>
            {
                if (rule.FieldName.Contains("@@"))
                {
                    var value = "";
                    switch (rule.FieldName)
                    {
                        case "@@BusinessObjectID@@":
                            value = BusinessObjectID;
                            break;
                        case "@@RecordID@@":
                            value = RecordID;
                            break;
                        case "@@ScreenID@@":
                            value = ScreenID;
                            break;
                        case "@@Subject@@":
                            value = Subject;
                            break;
                    }

                    if (adjustedValues.ContainsKey(rule.FieldName))
                    {
                        var hasMatch = Core.Logic.HandleMatchFieldRules(adjustedValues[rule.FieldName], rule.Operator, rule.Value);
                        if (hasMatch)
                        {
                            adjustedValues[rule.FieldName] = rule.Result;
                        }
                    }
                    else
                    {
                        var hasMatch = Core.Logic.HandleMatchFieldRules(value, rule.Operator, rule.Value);
                        if (hasMatch)
                        {
                            adjustedValues.Add(rule.FieldName, rule.Result);
                        }
                    }

                }
                else if (rule.FieldName.Contains("**"))
                {
                    var fieldId = Core.Logic.GetAttributeID(rule.FieldName);
                    if (fieldId != -1)
                    {
                        if (adjustedValues.ContainsKey(rule.FieldName))
                        {
                            var hasMatch = Core.Logic.HandleMatchFieldRules(adjustedValues[rule.FieldName], rule.Operator, rule.Value);
                            if (hasMatch)
                            {
                                adjustedValues[rule.FieldName] = rule.Result;
                            }
                        }
                        else
                        {
                            var fieldValue = GetRecordAttribute(fieldId);
                            var hasMatch = Core.Logic.HandleMatchFieldRules(fieldValue, rule.Operator, rule.Value);
                            if (hasMatch)
                            {
                                adjustedValues.Add(rule.FieldName, rule.Result);
                            }
                        }
                    }
                }
            });
        }

        private URLRule GetCustomUrl()
        {
            foreach (var rule in WebTab.URLRules)
            {
                switch (rule.Field)
                {
                    case "BusinessObjectID":
                        if (BusinessObjectID == rule.Match)
                        {
                            return rule;
                        }
                        break;

                    case "ScreenID":
                        if (ScreenID == rule.Match)
                        {
                            return rule;
                        }
                        break;

                    case "Subject":
                        if (Subject == rule.Match)
                        {
                            return rule;
                        }
                        break;
                }
            }

            return null;
        }

        private string GetRecordAttribute(int attributeNumber)
        {
            var result = string.Empty;

            try
            {
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
            }
            catch
            {

            }

            return result;
        }

        private void LoadCustomPage()
        {
            if (WebTab.BlockRefreshAfterNavigateAway && HasNavigatedAway)
            {
                return;
            }

            cBrowser.Load(BuildCustomUrl(AddressURL));
        }

        private string BusinessObjectID
        {
            get
            {
                if (CommonBlock?.CurrentRecord != null)
                {
                    var id = this.CommonBlock.CurrentRecord.FileId;

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

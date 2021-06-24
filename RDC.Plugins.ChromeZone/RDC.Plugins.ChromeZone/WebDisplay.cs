using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Xml;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using RDC.Plugins.ChromeZone.Core.Objects;

namespace RDC.Plugins.ChromeZone
{
    public partial class WebDisplay : DisplayComponents
    {
        private WebView2 webView2;

        private bool HasNavigatedAway = false;

        private string AddressURL = string.Empty;

        private string TabName = string.Empty;

        private string ScreenID;
        private string _Subject;

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetDllDirectory(string lpPathName);

        public WebTab WebTab
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

            SetDllDirectory(Core.Settings.SettingsWrapper.WebView2FolderLocation);


            InitializeComponent();

            forwardButton.Click += forwardButton_Click;
            backButton.Click += backButton_Click;
            refreshButton.Click += refreshButton_Click;
            homeButton.Click += HomeButton_Click;

            SetupBrowser();
        }

        private void TryLoadSettings()
        {
            if (!Core.Settings.LoadSettings())
            {
                return;
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

            toolStripContainer.ContentPanel.Controls.Add(webView2);

            LoadCustomPage();
        }

        //Setup the browser and it's dll directories 
        private async void SetupBrowser()
        {
            webView2 = new WebView2();

            string DefaultLogFilePath = Path.Combine(Path.GetTempPath(), "OzoneWebVTemp");

            if (Directory.Exists(DefaultLogFilePath) == false)
            {
                Directory.CreateDirectory(DefaultLogFilePath);
            }

            var env = await CoreWebView2Environment.CreateAsync(null, DefaultLogFilePath);

           await webView2.EnsureCoreWebView2Async(env);

            webView2.NavigationStarting += WebView2_NavigationStarting;
            webView2.NavigationCompleted += WebView2OnNavigationCompleted;

            webView2.Width = Int32.MaxValue;
            webView2.Height = Int32.MaxValue;
            webView2.Dock = DockStyle.Fill;
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

        private void WebView2OnNavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            Waiting = false;
            ProgressIndicatorVisible = false;
        }

        private void WebView2_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            InvokeOnUiThreadIfRequired(() => urlTextBox.Text = e.Uri);

            HasNavigatedAway = e.Uri.ToLower() != BuildCustomUrl(AddressURL).ToLower();

            Waiting = true;
            ProgressIndicatorVisible = true;
        }

        private void HomeButton_Click(object sender, EventArgs e)
        {
            webView2.CoreWebView2.Navigate(BuildCustomUrl(AddressURL));
            HasNavigatedAway = false;
        }

        private void backButton_Click(object sender, EventArgs e)
        {
            webView2.GoBack();
        }

        private void forwardButton_Click(object sender, EventArgs e)
        {
            webView2.GoForward();
        }

        private void refreshButton_Click(object sender, EventArgs e)
        {
            webView2.Refresh();
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

                var rem = GetRecordAttribute("CE.CEM.NAME");

                rem.ToString();
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

        private string GetRecordAttribute(string dictionary)
        {
            var result = string.Empty;

            try
            {
                if (CommonBlock == null)
                {
                    return result;
                }

                HybridDictionary att = CommonBlock.CurrentRecordDisplay.GetAttribute(dictionary);

                if (att == null)
                {
                    foreach (DictionaryEntry o in CommonBlock.CurrentRecordDisplay.Data)
                    {
                        if (o.Key.ToString().Contains(dictionary))
                        {
                            att = o.Value as HybridDictionary;
                            break;
                        }
                    }
                }

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

            webView2.CoreWebView2?.Navigate(BuildCustomUrl(AddressURL));
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

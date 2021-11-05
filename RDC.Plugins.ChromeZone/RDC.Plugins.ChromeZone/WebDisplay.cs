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
using Newtonsoft.Json;
using Origen.Ozone;
using Origen.Ozone.Classes;
using Origen.Ozone.Plugins;
using RDC.Plugins.ChromeZone.Core.Interfaces;
using RDC.Plugins.ChromeZone.Core.Objects;
using ContentAlignment = System.Drawing.ContentAlignment;

namespace RDC.Plugins.ChromeZone
{
    public partial class WebDisplay : DisplayComponents
    {
        private WebView2 webView2;

        public static Core.Objects.SettingsWrapper SettingsWrapper { get; set; }

        private bool HasNavigatedAway = false;

        private string AddressURL = string.Empty;

        private string TabName = "Unloaded";

        private string ScreenID;
        private string _Subject;

        private bool Displaying = false;
        private bool Debug = false;
        private DebugWindow DebugWindow;
        private List<string> DebugLogs = new List<string>();

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetDllDirectory(string lpPathName);

        public WebTab WebTab
        {
            get
            {
                var CurrentTab = SettingsWrapper.WebTabs.Find(t => t.Name == TabName);

                return (WebTab) CurrentTab;
            }
        }

        public WebDisplay()
        {
            TryLoadSettings();

            LogDebugMessage($"WebView2Folder location: {SettingsWrapper.WebView2FolderLocation}");

            if (Directory.Exists(SettingsWrapper.WebView2FolderLocation))
            {
                if (File.Exists(Path.Combine(SettingsWrapper.WebView2FolderLocation, "WebView2Loader.dll")) == false)
                {
                    LogDebugMessage($"Can't find WebView2Loader.dll in folder: {SettingsWrapper.WebView2FolderLocation}");
                    return;
                }
                else
                {
                    SetDllDirectory(SettingsWrapper.WebView2FolderLocation);
                }
            }
            else
            {
                LogDebugMessage($"WebView2Folder location not found!");
                return;
            }

            InitializeComponent();

            forwardButton.Click += forwardButton_Click;
            backButton.Click += backButton_Click;
            refreshButton.Click += refreshButton_Click;
            homeButton.Click += HomeButton_Click;

            SetupBrowser();
            
        }

        private void TryLoadSettings()
        {
            if (LoadSettings())
            {
                return;
            }
        }

        //Ozone Initialize function
        public override void Initialize(string InitData)
        {
            base.Initialize(InitData);

            if (InitData.Contains("|"))
            {
                var parts = InitData.Split('|');
                TabName = parts[0];

                if (parts[1].ToLower() == "debug")
                {
                    Debug = true;
                }
            }
            else
            {
                TabName = InitData;
            }

            Displaying = true;

            if (Debug)
            {
                DebugWindow = new DebugWindow(DebugLogs);
                DebugWindow.Show();
                DebugWindow.SetTabName(TabName);
            }

            if (webView2 == null)
            {
                LogDebugMessage($"WebView2 not loaded due to error!!");
                var label = new Label
                {
                    Dock = DockStyle.Fill,
                    Width = Int32.MaxValue,
                    Height = Int32.MaxValue,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Text = "Issue Loading webview.",
                    Font = new Font(FontFamily.GenericSansSerif, 25, FontStyle.Bold),
                    
                };
                toolStripContainer.ContentPanel.Controls.Add(label);
                return;
            }

            //System.Diagnostics.Debugger.Launch();

            if (WebTab == null)
            {
                LogDebugMessage($"Tab name {TabName} Could not be found in setting file!");
                return;
            }

            AddressURL = WebTab.DefaultURL;

            if (AddressURL == string.Empty)
            {
                AddressURL = InitData;
            }

            LogDebugMessage($"Default address: {AddressURL}");

            toolStripContainer.ContentPanel.Controls.Add(webView2);
            
            LoadCustomPage();
        }

        //Setup the browser and it's dll directories 
        private async void SetupBrowser()
        {
            var version = Core.InstallCheck.GetWebView2Version();
            if (version == String.Empty)
            {
                LogDebugMessage("Couldn't find installed WebView on system!");
                return;
            }

            LogDebugMessage($"found installed Web2 version {version}");

            webView2 = new WebView2();

            string DefaultLogFilePath = Path.Combine(Path.GetTempPath(), "OzoneWebVTemp");
            LogDebugMessage($"Temp Directory for web files: {DefaultLogFilePath}");

            if (Directory.Exists(DefaultLogFilePath) == false)
            {
                Directory.CreateDirectory(DefaultLogFilePath);
                LogDebugMessage($"Created Temp Directory for web files in: {DefaultLogFilePath}");
            }

            try
            {
                var env = await CoreWebView2Environment.CreateAsync(null, DefaultLogFilePath);

                await webView2.EnsureCoreWebView2Async(env);
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.ToString());
                LogDebugMessage($"Error creating WebView2 Environment: {e}");
                throw;
            }

            webView2.NavigationStarting += WebView2_NavigationStarting;
            webView2.NavigationCompleted += WebView2OnNavigationCompleted;

            webView2.Width = Int32.MaxValue;
            webView2.Height = Int32.MaxValue;
            webView2.Dock = DockStyle.Fill;
        }

        public override void RecordRefresh(string InitData = "")
        {
            if (WebTab == null)
            {
                return;
            }

            if (WebTab.RefreshOnRecordLoad)
            {
                LoadCustomPage();
            }
        }

        public override void FunctionChanged(string FunctionID, string SubjectID)
        {
            ScreenID = FunctionID;
            _Subject = SubjectID;

            if (WebTab == null)
            {
                return;
            }

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
            if (webView2 == null)
            {
                return;
            }

            var url = BuildCustomUrl(AddressURL);
            LogDebugMessage($"Custom URL has been created. {url}");

            webView2.CoreWebView2.Navigate(url);
            HasNavigatedAway = false;
        }

        private void backButton_Click(object sender, EventArgs e)
        {
            webView2?.GoBack();
        }

        private void forwardButton_Click(object sender, EventArgs e)
        {
            webView2?.GoForward();
        }

        private void refreshButton_Click(object sender, EventArgs e)
        {
            webView2?.Refresh();
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
                var attributeFields = Core.Logic.GetAttributeFields(baseUrl);

                attributeFields.ForEach(i =>
                {
                    var replaceName = $"**{i}**";
                    var RecordAttribute = GetRecordAttribute(i);

                    LogDebugMessage($"Record Value lookup found in URL: {i}, will be replaced with {RecordAttribute}");

                    baseUrl = baseUrl.Replace(replaceName, RecordAttribute);
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
                    var fieldId = Core.Logic.GetAttributeField(conversion.FieldName);
                    if (fieldId != String.Empty)
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
                    var fieldId = Core.Logic.GetAttributeField(rule.FieldName);
                    if (fieldId != string.Empty)
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
                bool RuleMatch = true;
                foreach (var ruleRule in rule.Rules)
                {
                    string Value = "";
                    switch (ruleRule.Field)
                    {
                        case "BusinessObjectID":
                            Value = BusinessObjectID;
                            break;

                        case "ScreenID":
                            Value = ScreenID;
                            break;

                        case "Subject":
                            Value = Subject;
                            break;
                    }

                    if (ruleRule.Field.StartsWith("**") && ruleRule.Field.EndsWith("**"))
                    {
                        var fieldName = Core.Logic.GetAttributeField(ruleRule.Field);
                        Value = GetRecordAttribute(fieldName);
                    }


                    if (Core.Logic.HandleMatchFieldRules(Value, ruleRule.Operator, ruleRule.Match) == false)
                    {
                        RuleMatch = false;
                    }
                }

                if (RuleMatch)
                {
                    LogDebugMessage($"returning URL: {rule.URL}");
                    return rule;
                }
            }

            return null;
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

                var lap = CommonBlock.CurrentRecord.LazyLoad(dictionary);
                if (lap.Count > 0)
                {
                    var value = lap[1].ToString();
                    if (value != string.Empty)
                    {
                        result = value;
                    }
                }

                if (result == string.Empty)
                {
                    var lad = CommonBlock.CurrentRecordDisplay.LazyLoad(dictionary);
                    var value = lad[1].ToString();
                    if (value != string.Empty)
                    {
                        result = value;
                    }
                }
            }
            catch
            {

            }

            return result;
        }

        private void LoadCustomPage()
        {
            if (webView2 == null)
            {
                return;
            }

            if (WebTab.BlockRefreshAfterNavigateAway && HasNavigatedAway)
            {
                LogDebugMessage($"BlockRefreshAfterNavigateAway is True, and page Has Navigated Away so will not change.");
                return;
            }

            var url = BuildCustomUrl(AddressURL);
            LogDebugMessage($"Custom URL has been created. {url}");

            webView2.CoreWebView2?.Navigate(url);
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

        public bool LoadSettings()
        {
            var value = System.Configuration.ConfigurationManager.AppSettings["ConfigFolderPath"];
            var SettingsFile = Path.Combine(value, @"ChromeZoneSettings.json");

            if (!File.Exists(SettingsFile))
            {
                LogDebugMessage($"Can't find settings file in: {SettingsFile}");
                return false;
            }

            LogDebugMessage($"Loading settings file from: {SettingsFile}");

            try
            {
                SettingsWrapper = JsonConvert.DeserializeObject<Core.Objects.SettingsWrapper>(File.ReadAllText(SettingsFile));
            }
            catch (Exception e)
            {
                LogDebugMessage($"Can't load settings files, error parsing file: {e}");
                return false;
            }

            return true;
        }

        public void LogDebugMessage(string Message)
        {
            var builtMessage = $"({DateTime.Now}) Tab Name: {TabName}, Message: {Message}";

            if (Displaying && DebugWindow != null)
            {
                try
                {
                    DebugWindow.AddDebugItem(builtMessage);
                }
                catch
                {

                }
            }
            else
            {
                DebugLogs.Add(builtMessage);
            }
            
        }
    }
}

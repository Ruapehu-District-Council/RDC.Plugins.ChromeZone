using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using AutoFormGenerator;
using AutoFormGenerator.Object;
using Newtonsoft.Json;
using Path = System.IO.Path;
using WinForms = System.Windows.Forms;

namespace ConfigApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string ConfigFolder = string.Empty;

        public static Objects.SettingsWrapper SettingsWrapper { get; set; }


        public MainWindow()
        {
            InitializeComponent();
        }

        private void BrowseFolder_Click(object sender, RoutedEventArgs e)
        {
            var FB = new WinForms.FolderBrowserDialog();
            if (FB.ShowDialog() != WinForms.DialogResult.OK)
            {
                return;
            }

            ConfigFolder = FB.SelectedPath;

            HandleFolder(ConfigFolder);
        }

        private void HandleFolder(string ConfigFolder)
        {
            EnvironmentDropDown.Items.Clear();

            var environments = Directory.GetFiles(ConfigFolder, "*.environment.config");

            foreach (var environment in environments)
            {
                var fileName = Path.GetFileName(environment);
                var CBI = new ComboBoxItem
                {
                    Content = fileName.Replace(".environment.config", ""),
                    Tag = environment
                };
                EnvironmentDropDown.Items.Add(CBI);
            }

            if (File.Exists(Path.Combine(ConfigFolder, "ChromeZoneSettings.json")))
            {
                try
                {
                    SettingsWrapper = JsonConvert.DeserializeObject<Objects.SettingsWrapper>(File.ReadAllText(Path.Combine(ConfigFolder, "ChromeZoneSettings.json")));

                    AutoFormGenerator.Logic afg = new Logic();
                    var afgControl = afg.BuildFormControl(SettingsWrapper);

                    var items = new List<AutoFormGenerator.Object.FormDropdownItem>
                    {
                        new FormDropdownItem()
                        {
                            Value = "BusinessObjectID",
                            DisplayValue = "BusinessObjectID"
                        },
                        new FormDropdownItem()
                        {
                            Value = "ScreenID",
                            DisplayValue = "ScreenID"
                        },
                        new FormDropdownItem()
                        {
                            Value = "Subject",
                            DisplayValue = "Subject"
                        }
                    };

                    afg.PopulateSpecialDropdown<Objects.URLRule>("Field", items);
                    afg.PopulateSpecialDropdown<Objects.FieldRule>("Operator", GetOperators());
                    ContentScrollViewer.Content = afgControl;
                }
                catch (Exception e)
                {
                    e.ToString();
                }
            }

            EnvironmentDropDown.SelectedIndex = 1;
        }

        private void EnvironmentDropDown_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EnvironmentDropDown.SelectedValue == null)
            {
                return;
            }

            var selectedItem = (ComboBoxItem) EnvironmentDropDown.SelectedValue;
            var FilePath = selectedItem.Tag.ToString();

            var xreader = new XmlDocument();
            xreader.Load(FilePath);

            var StartupItems = xreader.SelectNodes("//configuration/startup/item[@type='plugin']");

            foreach (XmlNode startupItem in StartupItems)
            {
                var command = startupItem.Attributes["command"].Value;

                if (command.Contains("RDC.Plugins.ChromeZone.dll"))
                {
                    command.ToString();
                }
            }
        }

        private List<FormDropdownItem> GetOperators()
        {
            var items = new List<AutoFormGenerator.Object.FormDropdownItem>
            {
                new FormDropdownItem()
                {
                    Value = "=",
                    DisplayValue = "="
                },
                new FormDropdownItem()
                {
                    Value = "!=",
                    DisplayValue = "!="
                },
                new FormDropdownItem()
                {
                    Value = ">",
                    DisplayValue = ">"
                },
                new FormDropdownItem()
                {
                    Value = ">=",
                    DisplayValue = ">="
                },
                new FormDropdownItem()
                {
                    Value = "<",
                    DisplayValue = "<"
                },
                new FormDropdownItem()
                {
                    Value = "<=",
                    DisplayValue = "<="
                }
            };

            return items;
        }
    }
}

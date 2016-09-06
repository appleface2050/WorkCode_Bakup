﻿using CLRBrowserSourcePlugin.Shared;
using CLROBS;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CLRBrowserSourcePlugin
{
    public class VolumeProgressBarValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double progress = (double)value;
            Brush foreground = Brushes.Green;

            if (progress >= 90d)
            {
                foreground = Brushes.Red;
            }
            else if (progress >= 60d)
            {
                foreground = Brushes.Yellow;
            }

            return foreground;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Interaction logic for SampleConfiguration.xaml
    /// </summary>
    public partial class ConfigDialog : Window
    {
        private XElement dataElement;
        private BrowserConfig config;

        private TextEditor cssEditor;
        private TextEditor templateEditor;

        public ConfigDialog(XElement dataElement)
        {
            InitializeComponent();
            this.dataElement = dataElement;

            config = new BrowserConfig();
            config.Reload(dataElement);

            cssEditor = new TextEditor
            {
                FontFamily = new FontFamily("Consolas"),
                SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("CSS"),
                ShowLineNumbers = true,
                Options =
                {
                    ConvertTabsToSpaces = true,
                    IndentationSize = 2
                }
            };

            templateEditor = new TextEditor
            {
                FontFamily = new FontFamily("Consolas"),
                SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("HTML"),
                ShowLineNumbers = true,
                Options =
                {
                    ConvertTabsToSpaces = true,
                    IndentationSize = 2
                }
            };

            advancedPropertiesCheckBox.IsChecked = config.BrowserSourceSettings.IsShowingAdvancedProperties;
            SetTabVisibility(advancedTab, config.BrowserSourceSettings.IsShowingAdvancedProperties);

            SetTabVisibility(templateTab, config.BrowserSourceSettings.IsApplyingTemplate);

            url.Text = config.BrowserSourceSettings.Url;
            cssEditor.Text = config.BrowserSourceSettings.CSS;
            templateEditor.Text = config.BrowserSourceSettings.Template;
            widthText.Text = config.BrowserSourceSettings.Width.ToString();
            heightText.Text = config.BrowserSourceSettings.Height.ToString();
            opacitySlider.Value = config.BrowserSourceSettings.Opacity;
            fpsTextBox.Text = config.BrowserSourceSettings.Fps.ToString();

            applyTemplateCheckBox.IsChecked = config.BrowserSourceSettings.IsApplyingTemplate;

            instanceSettings.SelectedObject = config.BrowserInstanceSettings;

            cssGrid.Children.Add(cssEditor);
            templateGrid.Children.Add(templateEditor);
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();

            int width;
            int height;
            int fps;

            if (!int.TryParse(widthText.Text, out width))
            {
                return;
            }

            if (!int.TryParse(heightText.Text, out height))
            {
                return;
            }

            if (!int.TryParse(fpsTextBox.Text, out fps))
            {
                return;
            }

            config.BrowserSourceSettings.Url = url.Text;
            config.BrowserSourceSettings.Width = width;
            config.BrowserSourceSettings.Height = height;
            config.BrowserSourceSettings.CSS = cssEditor.Text;
            config.BrowserSourceSettings.Template = templateEditor.Text;
            config.BrowserSourceSettings.IsShowingAdvancedProperties =
                advancedPropertiesCheckBox.IsChecked.GetValueOrDefault();
            config.BrowserSourceSettings.IsApplyingTemplate =
                applyTemplateCheckBox.IsChecked.GetValueOrDefault();
            config.BrowserSourceSettings.Opacity = opacitySlider.Value;
            config.BrowserSourceSettings.Fps = fps;
            //config.BrowserSourceSettings.IsMuted =
            //    !mutedCheckbox.IsChecked.GetValueOrDefault(false);
            //config.BrowserSourceSettings.Volume = (float)volumeSlider.Value;

            DialogResult = config.Save(dataElement);

            Close();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void browseButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void SetTabVisibility(TabItem tabItem, bool isVisible)
        {
            //Style style = new Style();
            //style.Setters.Add(new Setter(UIElement.VisibilityProperty, isVisible ? Visibility.Visible : Visibility.Collapsed));
            //tabItem.Style = style;
            tabItem.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        private void advancedPropertiesCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            SetTabVisibility(advancedTab, cb.IsChecked.GetValueOrDefault());
        }

        private void applyTemplateCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            SetTabVisibility(templateTab, cb.IsChecked.GetValueOrDefault());
        }

        private void browseButton_Click_1(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog
            OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.DefaultExt = "*.*";
            dlg.Filter = "All Files (*.*)|*.*";

            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox
            if (result == true)
            {
                url.Text = "http://absolute/" + dlg.FileName;
            }
        }

        //private void mutedCheckbox_Checked(object sender, RoutedEventArgs e)
        //{
        //    mutedTextBlock.Visibility = Visibility.Hidden;
        //}

        //private void mutedCheckbox_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    mutedTextBlock.Visibility = Visibility.Visible;
        //}
    }
}
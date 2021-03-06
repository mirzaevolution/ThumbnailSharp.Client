﻿using System.Windows;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using System.IO;
using System;

namespace ThumbnailSharp.Gui
{
    public partial class MainWindow : MetroWindow
    {
         public MainWindow()
        {
            HandleLocalization();
            InitializeComponent();
        }

        private async void ThumbViewModel_Error(object sender, string e)
        {
            await this.ShowMessageAsync("Error", e);
            
        }

        private async void ThumbViewModel_Completed(object sender, string e)
        {
            await this.ShowMessageAsync("Message", e);
        }

        private void ButtonBrowseHandler(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog
            {
                CheckFileExists = true,
                CheckPathExists = true,
                FileName = "",
                Filter = "Image Files |*.jpg;*.jpeg;*.bmp;*.png;*.giff;*.tiff"
            };
            if(fileDialog.ShowDialog().Value)
            {
                textboxLocation.Text = fileDialog.FileName;
            }
        }

        private void ButtonBrowseTargetHandler(object sender, RoutedEventArgs e)
        {
            SaveFileDialog fileDialog = new SaveFileDialog
            {
                FileName = "",
                Filter = "Jpg File (*.jpg)|*.jpg|Bmp File (*.bmp)|*.bmp|Png File (*.png)|*.png|Gif File (*.gif)|*.gif|Tiff File (*.tiff)|*.tiff"
            };
            if(fileDialog.ShowDialog().Value)
            {
                textboxTargetLocation.Text = fileDialog.FileName;
            }
        }

        private void UrlHandler(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.ToString());
        }
        
        private void HandleLocalization()
        {
            try
            {
                if (File.Exists(LanguageDataLocation.Location))
                {
                    string shortName = "";
                    using (StreamReader reader = new StreamReader(LanguageDataLocation.Location))
                    {
                        shortName = reader.ReadToEnd();
                    }
                    if (!String.IsNullOrEmpty(shortName))
                        LanguageResource.Instance.CurrentCulture = new System.Globalization.CultureInfo(shortName);
                    else
                        LanguageResource.Instance.CurrentCulture = new System.Globalization.CultureInfo("en-US");
                }
                else
                {
                    LanguageResource.Instance.CurrentCulture = new System.Globalization.CultureInfo("en-US");
                }
            }
            catch { LanguageResource.Instance.CurrentCulture = new System.Globalization.CultureInfo("en-US"); }
        }

    }
}

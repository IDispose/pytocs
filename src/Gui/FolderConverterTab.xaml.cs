﻿#region License
//  Copyright 2015-2018 John Källén
// 
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
#endregion

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

namespace Pytocs.Gui
{
    public class FolderConverterTab : UserControl
    {
        private readonly object _syncRoot = new object();

        private bool _isConversionInProgress;

        private TextBox TargetFolderBox { get; }

        private TextBox SourceFolderBox { get; }

        private TextBox ConversionLogBox { get; }

        private Func<string, Task> AppendLog { get; }

        public FolderConverterTab()
        {
            this.InitializeComponent();
            SourceFolderBox = this.FindControl<TextBox>(nameof(SourceFolderBox));
            TargetFolderBox = this.FindControl<TextBox>(nameof(TargetFolderBox));
            ConversionLogBox = this.FindControl<TextBox>(nameof(ConversionLogBox));

            AppendLog = x => Dispatcher.UIThread.InvokeAsync(() => ConversionLogBox.Text += x);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private async void BrowseSource_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog();
            var result = await dialog.ShowAsync();

            if (!string.IsNullOrWhiteSpace(result))
            {
                SourceFolderBox.Text = result;
            }
        }

        private async void BrowseTarget_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog();
            var result = await dialog.ShowAsync();

            if (!string.IsNullOrWhiteSpace(result))
            {
                TargetFolderBox.Text = result;
            }
        }

        private async void Convert_Click(object sender, RoutedEventArgs e)
        {
            var (sourceFolder, targetFolder) = GetValidConversionFolders();

            if (sourceFolder == null)
            {
                return;
            }

            lock (_syncRoot)
            {
                if (_isConversionInProgress)
                {
                    return;
                }

                _isConversionInProgress = true;
            }

            ConversionLogBox.Text = string.Empty;

            await ConversionUtils.ConvertFolderAsync(sourceFolder, targetFolder, new DelegateLogger(AppendLog));

            lock (_syncRoot)
            {
                _isConversionInProgress = false;
            }
        }

        private (string sourceFolder, string targetFolder) GetValidConversionFolders()
        {
            if (string.IsNullOrWhiteSpace(SourceFolderBox.Text))
            {
                //$TODO: use a configuration file for UI message
                ConversionLogBox.Text = "Source directory path is empty.";
                return (null, null);
            }

            if (string.IsNullOrWhiteSpace(TargetFolderBox.Text))
            {
                //$TODO: use a configuration file for UI message
                ConversionLogBox.Text = "Target directory path is empty.";
                return (null, null);
            }


            var sourceFolder = Path.GetFullPath(SourceFolderBox.Text);
            var targetFolder = Path.GetFullPath(TargetFolderBox.Text);

            SourceFolderBox.Text = sourceFolder;
            TargetFolderBox.Text = targetFolder;

            if (!Directory.Exists(sourceFolder))
            {
                //$TODO: use a configuration file for UI message
                ConversionLogBox.Text = "Invalid source directory path.";
                return (null, null);
            }

            if (!Directory.Exists(targetFolder))
            {
                try
                {
                    Directory.CreateDirectory(targetFolder);
                }
                catch (Exception ex)
                {
                    //$TODO: use a configuration file for UI message
                    ConversionLogBox.Text = "Couldn't create target directory:\n" + ex.Message;
                    return (null, null);
                }
            }

            return (sourceFolder, targetFolder);
        }
    }
}

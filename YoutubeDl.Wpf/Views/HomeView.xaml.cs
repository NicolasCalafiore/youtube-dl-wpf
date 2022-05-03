﻿using MaterialDesignThemes.Wpf;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Input;
using System.Windows.Shell;
using YoutubeDl.Wpf.Utils;

namespace YoutubeDl.Wpf.Views
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class HomeView
    {
        public HomeView()
        {
            InitializeComponent();

            this.WhenActivated(disposables =>
            {
                // Link and Start
                this.Bind(ViewModel,
                    viewModel => viewModel.Link,
                    view => view.linkTextBox.Text)
                    .DisposeWith(disposables);

                linkTextBox.Events().KeyDown
                           .Where(x => x.Key == Key.Enter)
                           .Select(x => Unit.Default)
                           .InvokeCommand(ViewModel!.StartDownloadCommand) // Null forgiving reason: upstream limitation.
                           .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.BackendService.GlobalDownloadProgressPercentage,
                    view => view.downloadButton.Content,
                    percentage => percentage > 0.0 ? percentage.ToString("P1") : "_Download")
                    .DisposeWith(disposables);

                // ButtonProgressAssist bindings
                ViewModel.WhenAnyValue(x => x.BackendInstance.IsRunning)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(x =>
                    {
                        ButtonProgressAssist.SetIsIndicatorVisible(downloadButton, x);
                        ButtonProgressAssist.SetIsIndicatorVisible(listFormatsButton, x);
                    })
                    .DisposeWith(disposables);

                ViewModel.WhenAnyValue(x => x.BackendService.ProgressState)
                    .Select(x => x == TaskbarItemProgressState.Indeterminate)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(x => ButtonProgressAssist.SetIsIndeterminate(downloadButton, x))
                    .DisposeWith(disposables);

                ViewModel.WhenAnyValue(x => x.BackendService.GlobalDownloadProgressPercentage)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(x => ButtonProgressAssist.SetValue(downloadButton, x * 100))
                    .DisposeWith(disposables);

                ViewModel.WhenAnyValue(x => x.BackendService.ProgressState)
                    .Select(x => x == TaskbarItemProgressState.Indeterminate)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(x => ButtonProgressAssist.SetIsIndeterminate(listFormatsButton, x))
                    .DisposeWith(disposables);

                // presetComboBox
                this.OneWayBind(ViewModel,
                    viewModel => viewModel.Presets,
                    view => view.presetComboBox.ItemsSource)
                    .DisposeWith(disposables);

                this.Bind(ViewModel,
                    viewModel => viewModel.SharedSettings.SelectedPreset,
                    view => view.presetComboBox.SelectedItem)
                    .DisposeWith(disposables);

                this.Bind(ViewModel,
                    viewModel => viewModel.SharedSettings.SelectedPresetText,
                    view => view.presetComboBox.Text)
                    .DisposeWith(disposables);

                // Subtitles
                this.Bind(ViewModel,
                    viewModel => viewModel.SharedSettings.DownloadSubtitles,
                    view => view.subtitlesDefaultCheckBox.IsChecked)
                    .DisposeWith(disposables);

                this.Bind(ViewModel,
                    viewModel => viewModel.SharedSettings.DownloadSubtitlesAllLanguages,
                    view => view.subtitlesAllLanguagesCheckBox.IsChecked)
                    .DisposeWith(disposables);

                this.Bind(ViewModel,
                    viewModel => viewModel.SharedSettings.DownloadAutoGeneratedSubtitles,
                    view => view.subtitlesAutoGeneratedCheckBox.IsChecked)
                    .DisposeWith(disposables);

                // Options row 1
                this.Bind(ViewModel,
                    viewModel => viewModel.SharedSettings.AddMetadata,
                    view => view.metadataToggle.IsChecked)
                    .DisposeWith(disposables);

                this.Bind(ViewModel,
                    viewModel => viewModel.SharedSettings.DownloadThumbnail,
                    view => view.thumbnailToggle.IsChecked)
                    .DisposeWith(disposables);

                this.Bind(ViewModel,
                    viewModel => viewModel.SharedSettings.DownloadPlaylist,
                    view => view.playlistToggle.IsChecked)
                    .DisposeWith(disposables);

                // Options row 2
                this.Bind(ViewModel,
                    viewModel => viewModel.SharedSettings.UseCustomOutputTemplate,
                    view => view.filenameTemplateToggle.IsChecked)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.SharedSettings.UseCustomOutputTemplate,
                    view => view.filenameTemplateTextBox.IsEnabled)
                    .DisposeWith(disposables);

                this.Bind(ViewModel,
                    viewModel => viewModel.SharedSettings.CustomOutputTemplate,
                    view => view.filenameTemplateTextBox.Text)
                    .DisposeWith(disposables);

                // Options row 3
                this.Bind(ViewModel,
                    viewModel => viewModel.SharedSettings.UseCustomPath,
                    view => view.pathToggle.IsChecked)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.SharedSettings.UseCustomPath,
                    view => view.pathComboBox.IsEnabled)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.DownloadPathHistory,
                    view => view.pathComboBox.ItemsSource)
                    .DisposeWith(disposables);

                this.Bind(ViewModel,
                    viewModel => viewModel.SharedSettings.DownloadPath,
                    view => view.pathComboBox.Text)
                    .DisposeWith(disposables);

                // Arguments
                this.OneWayBind(ViewModel,
                    viewModel => viewModel.DownloadArguments,
                    view => view.argumentsItemsControl.ItemsSource)
                    .DisposeWith(disposables);

                // Output
                this.Bind(ViewModel,
                    viewModel => viewModel.QueuedTextBoxSink.Content,
                    view => view.resultTextBox.Text)
                    .DisposeWith(disposables);

                resultTextBox.Events().TextChanged
                             .Where(_ => WpfHelper.IsScrolledToEnd(resultTextBox))
                             .Subscribe(_ => resultTextBox.ScrollToEnd())
                             .DisposeWith(disposables);

                // Download, list, abort button
                this.BindCommand(ViewModel,
                    viewModel => viewModel.StartDownloadCommand,
                    view => view.downloadButton)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel,
                    viewModel => viewModel.ListFormatsCommand,
                    view => view.listFormatsButton)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel,
                    viewModel => viewModel.AbortDlCommand,
                    view => view.abortButton)
                    .DisposeWith(disposables);

                // Browse and open folder button
                this.BindCommand(ViewModel,
                    viewModel => viewModel.BrowseDownloadFolderCommand,
                    view => view.browseButton)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel,
                    viewModel => viewModel.OpenDownloadFolderCommand,
                    view => view.openFolderButton)
                    .DisposeWith(disposables);

                // Reset custom filename template button
                this.BindCommand(ViewModel,
                    viewModel => viewModel.ResetCustomFilenameTemplateCommand,
                    view => view.resetFilenameTemplateButton)
                    .DisposeWith(disposables);

                // Custom preset buttons
                this.BindCommand(ViewModel,
                    viewModel => viewModel.OpenAddCustomPresetDialogCommand,
                    view => view.addPresetButton)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel,
                    viewModel => viewModel.OpenEditCustomPresetDialogCommand,
                    view => view.editPresetButton)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel,
                    viewModel => viewModel.DuplicatePresetCommand,
                    view => view.duplicatePresetButton)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel,
                    viewModel => viewModel.DeleteCustomPresetCommand,
                    view => view.deletePresetButton)
                    .DisposeWith(disposables);
            });
        }
    }
}

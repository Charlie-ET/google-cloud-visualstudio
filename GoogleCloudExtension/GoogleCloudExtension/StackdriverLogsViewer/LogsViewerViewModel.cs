﻿// Copyright 2016 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Google.Apis.Logging.v2.Data;
using GoogleCloudExtension.Accounts;
using GoogleCloudExtension.DataSources;
using GoogleCloudExtension.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;

namespace GoogleCloudExtension.StackdriverLogsViewer
{

    /// <summary>
    /// The view model for LogsViewerToolWindow.
    /// </summary>
    public class LogsViewerViewModel : ViewModelBase
    {
        private const int DefaultPageSize = 100;

        private bool _isLoading = false;
        private Lazy<LoggingDataSource> _dataSource;
        private string _nextPageToken;

        private string _firstRowDate;
        private bool _toggleExpandAllExpanded = false;
        private bool _isControlEnabled = true;

        private ObservableCollection<LogItem> _logs = new ObservableCollection<LogItem>();

        private string _requestStatusText;
        private string _requestErrorMessage;
        private bool _showRequestErrorMessage = false;
        private bool _showRequestStatus = false;
        private bool _showCancelRequestButton = false;
        private CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// Gets the refresh button command.
        /// </summary>
        public ProtectedCommand RefreshCommand { get; }

        /// <summary>
        /// Gets the account name.
        /// </summary>
        public string Account => CredentialsStore.Default.CurrentAccount?.AccountName ?? "";

        /// <summary>
        /// Gets the project id.
        /// </summary>
        public string Project => CredentialsStore.Default.CurrentProjectId ?? "";

        /// <summary>
        /// Route the expander IsExpanded state to control expand all or collapse all.
        /// </summary>
        public bool ToggleExapandAllExpanded
        {
            get { return _toggleExpandAllExpanded; }
            set
            {
                SetValueAndRaise(ref _toggleExpandAllExpanded, value);
                RaisePropertyChanged(nameof(ToggleExapandAllToolTip));
            }
        }

        /// <summary>
        /// Gets the tool tip for Toggle Expand All button.
        /// </summary>
        public string ToggleExapandAllToolTip => _toggleExpandAllExpanded ?
            Resources.LogViewerCollapseAllTip : Resources.LogViewerExpandAllTip;

        /// <summary>
        /// Gets the visible view top row date in string.
        /// When data grid vertical scroll moves, the displaying rows move. 
        /// This is to return the top row date
        /// </summary>
        public string FirstRowDate
        {
            get { return _firstRowDate; }
            private set { SetValueAndRaise(ref _firstRowDate, value); }
        }

        /// <summary>
        /// Gets the LogItem collection
        /// </summary>
        public ListCollectionView LogItemCollection { get; }

        /// <summary>
        /// Gets the cancel request button ICommand interface.
        /// </summary>
        public ProtectedCommand CancelRequestCommand { get; }

        /// <summary>
        /// Gets the cancel request button visibility
        /// </summary>
        public bool ShowCancelRequestButton
        {
            get { return _showCancelRequestButton; }
            private set { SetValueAndRaise(ref _showCancelRequestButton, value); }
        }

        /// <summary>
        /// Gets the request error message visibility.
        /// </summary>
        public bool ShowRequestErrorMessage
        {
            get { return _showRequestErrorMessage; }
            private set { SetValueAndRaise(ref _showRequestErrorMessage, value); }
        }

        /// <summary>
        /// Gets the request error message.
        /// </summary>
        public string RequestErrorMessage
        {
            get { return _requestErrorMessage; }
            private set { SetValueAndRaise(ref _requestErrorMessage, value); }
        }

        /// <summary>
        /// Gets the request status text message.
        /// </summary>
        public string RequestStatusText
        {
            get { return _requestStatusText; }
            private set { SetValueAndRaise(ref _requestStatusText, value); }
        }

        /// <summary>
        /// Gets the request status block visibility. 
        /// It includes the request status text block and cancel request button.
        /// </summary>
        public bool ShowRequestStatus
        {
            get { return _showRequestStatus; }
            private set { SetValueAndRaise(ref _showRequestStatus, value); }
        }

        /// <summary>
        /// Gets if it is making request to remote servers or not.
        /// </summary>
        public bool IsControlEnabled
        {
            get { return _isControlEnabled; }
            private set { SetValueAndRaise(ref _isControlEnabled, value); }
        }

        /// <summary>
        /// Initializes an instance of <seealso cref="LogsViewerViewModel"/> class.
        /// </summary>
        public LogsViewerViewModel()
        {
            _dataSource = new Lazy<LoggingDataSource>(CreateDataSource);
            RefreshCommand = new ProtectedCommand(() => Reload());
            LogItemCollection = new ListCollectionView(_logs);
            LogItemCollection.GroupDescriptions.Add(new PropertyGroupDescription(nameof(LogItem.Date)));
            CancelRequestCommand = new ProtectedCommand(CancelRequest);
        }

        /// <summary>
        /// When a new view model is created and attached to Window, invalidate controls and re-load first page
        /// of log entries.
        /// </summary>
        public void InvalidateAllProperties()
        {
            if (String.IsNullOrWhiteSpace(CredentialsStore.Default.CurrentAccount?.AccountName) ||
                String.IsNullOrWhiteSpace(CredentialsStore.Default.CurrentProjectId))
            {
                return;
            }

            Reload();
        }

        /// <summary>
        /// Append a set of log entries.
        /// </summary>
        public void AddLogs(IList<LogEntry> logEntries)
        {
            if (logEntries == null)
            {
                return;
            }

            foreach (var log in logEntries)
            {
                _logs.Add(new LogItem(log));
            }
        }

        /// <summary>
        /// Send request to get logs following prior requests.
        /// </summary>
        public void LoadNextPage()
        {
            if (String.IsNullOrWhiteSpace(_nextPageToken) || String.IsNullOrWhiteSpace(Project))
            {
                return;
            }

            LogLoaddingWrapperAsync(async (cancelToken) => await LoadLogsAsync(cancelToken));
        }

        /// <summary>
        /// Cancel request button command.
        /// </summary>
        private void CancelRequest()
        {
            Debug.WriteLine("Cancel command is called");
            RequestStatusText = Resources.LogViewerRequestCancellingMessage;
            ShowCancelRequestButton = false;
            _cancellationTokenSource?.Cancel();
        }

        /// <summary>
        /// Show request status bar.
        /// </summary>
        private void InitAndShowRequestStatus()
        {
            ShowRequestErrorMessage = false;
            RequestStatusText = Resources.LogViewerRequestProgressMessage;
            ShowRequestStatus = true;
            ShowCancelRequestButton = true;
        }

        /// <summary>
        /// A wrapper to LoadLogs.
        /// This is to make the try/catch statement conscise and easy to read.
        /// </summary>
        /// <param name="callback">A function to execute.</param>
        private async Task LogLoaddingWrapperAsync(Func<CancellationToken, Task> callback)
        {
            if (_isLoading)
            {
                Debug.WriteLine($"_isLoading is true. Skip.");
                return;
            }

            Debug.WriteLine("Setting _isLoading to true");
            _isLoading = true;

            _cancellationTokenSource = new CancellationTokenSource();
            IsControlEnabled = false;
            InitAndShowRequestStatus();
            try
            {
                await callback(_cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                _nextPageToken = null;

                if (ex is TaskCanceledException && _cancellationTokenSource.IsCancellationRequested)
                {
                    // Expected cancellation. Log and continue.
                    Debug.WriteLine("Request was cancelled");
                }
                else if (ex is DataSourceException)
                {
                    ShowRequestErrorMessage = true;
                    RequestErrorMessage = ex.Message;
                }

                throw;
            }
            finally
            {
                IsControlEnabled = true;
                ShowRequestStatus = false;
                Debug.WriteLine("Setting _isLoading to false");
                _isLoading = false;
            }
        }

        /// <summary>
        /// Repeatedly make list log entries request till it gets desired number of logs or it reaches end.
        /// _nextPageToken is used to control if it is getting first page or continuous page.
        /// 
        /// On complex filters, scanning through logs take time. The server returns empty results 
        ///   with a next page token. Continue to send request till some logs are found.
        /// </summary>
        private async Task LoadLogsAsync(CancellationToken cancellationToken)
        {
            int count = 0;
            while (count < DefaultPageSize)
            {
                Debug.WriteLine($"LoadLogs, count={count}, firstPage={_nextPageToken == null}");

                // Here, it does not do pageSize: _defaultPageSize - count, 
                // Because this is requried to use same page size for getting next page. 
                var results = await _dataSource.Value.ListLogEntriesAsync(
                    pageSize: DefaultPageSize, nextPageToken: _nextPageToken, cancelToken: cancellationToken);
                AddLogs(results?.LogEntries);
                _nextPageToken = results.NextPageToken;
                if (results?.LogEntries != null)
                {
                    count += results.LogEntries.Count;
                }

                if (String.IsNullOrWhiteSpace(_nextPageToken))
                {
                    _nextPageToken = null;
                    break;
                }
            }
        }

        /// <summary>
        /// Send request to get logs using new filters, orders etc.
        /// </summary>
        private void Reload()
        {
            if (String.IsNullOrWhiteSpace(Project))
            {
                return;
            }

            LogLoaddingWrapperAsync(async (cancelToken) => {
                _nextPageToken = null;
                _logs.Clear();
                await LoadLogsAsync(cancelToken);
            });
        }

        /// <summary>
        /// Create <seealso cref="LoggingDataSource"/> object with current project id.
        /// </summary>
        private LoggingDataSource CreateDataSource()
        {
            if (CredentialsStore.Default.CurrentProjectId != null)
            {
                return new LoggingDataSource(
                    CredentialsStore.Default.CurrentProjectId,
                    CredentialsStore.Default.CurrentGoogleCredential,
                    GoogleCloudExtensionPackage.VersionedApplicationName);
            }
            else
            {
                return null;
            }
        }
    }
}
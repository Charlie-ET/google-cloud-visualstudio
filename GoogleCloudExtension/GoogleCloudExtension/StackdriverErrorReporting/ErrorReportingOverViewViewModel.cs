// Copyright 2016 Google Inc. All Rights Reserved.
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

using Google.Apis.Clouderrorreporting.v1beta1.Data;
using GoogleCloudExtension.Accounts;
using GoogleCloudExtension.DataSources.ErrorReporting;
using GoogleCloudExtension.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;

namespace GoogleCloudExtension.StackdriverErrorReporting
{
    /// <summary>
    /// The view model for Stackdriver Error Reporting overview Window
    /// </summary>
    class ErrorReportingOverViewViewModel : ViewModelBase
    {
        private Lazy<SerDataSource> _dataSource;
        private ObservableCollection<ErrorGroupItem> _groupStatsCollection;

        public ListCollectionView GroupStatsView { get; }

        public ErrorReportingOverViewViewModel()
        {
            _groupStatsCollection = new ObservableCollection<ErrorGroupItem>();
            _dataSource = new Lazy<SerDataSource>(CreateDataSource);
            GroupStatsView = new ListCollectionView(_groupStatsCollection);
            GetGroupStats();
        }

        private async Task GetGroupStats()
        {
            var results = await _dataSource.Value.ListGroupStatusAsync();
            AddItems(results.GroupStats);
        }

        private void AddItems(IList<ErrorGroupStats> groupStats)
        {
            if (groupStats == null)
            {
                return;
            }

            Debug.WriteLine($"Gets {groupStats.Count} items");
            foreach (var item in groupStats)
            {
                if (item == null)
                {
                    return;
                }

                _groupStatsCollection.Add(new ErrorGroupItem(item));
            }
        }

        /// <summary>
        /// Create <seealso cref="LoggingDataSource"/> object with current project id.
        /// </summary>
        private SerDataSource CreateDataSource()
        {
            if (CredentialsStore.Default.CurrentProjectId != null)
            {
                return new SerDataSource(
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

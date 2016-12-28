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
using GoogleCloudExtension.DataSources;
using GoogleCloudExtension.DataSources.ErrorReporting;
using GoogleCloudExtension.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Linq;

namespace GoogleCloudExtension.StackdriverErrorReporting
{
    /// <summary>
    /// The view model for Stackdriver Error Reporting overview Window
    /// </summary>
    class ErrorReportingViewModel : ViewModelBase
    {
        static ErrorReportingViewModel()
        {
            Instance = new ErrorReportingViewModel();
        }

        private bool isLoading;

        private bool _showException;
        private string _exceptionString;

        public string ExceptionString
        {
            get { return _exceptionString; }
            set { SetValueAndRaise(ref _exceptionString, value); }
        }

        public bool ShowException
        {
            get { return _showException; }
            set { SetValueAndRaise(ref _showException, value); }
        }

        public bool IsLoadingComplete
        {
            get { return !isLoading; }
            set { SetValueAndRaise(ref isLoading, !value); }
        }

        public static ErrorReportingViewModel Instance { get; }

        public TimeRangeButtonsViewModel TimeRangeButtonsModel { get; }

        private ObservableCollection<ErrorGroupItem> _groupStatsCollection;

        public ListCollectionView GroupStatsView { get; }

        public string CurrentTimeRangeCaption => TimeRangeButtonsModel.SelectedTimeRangeItem.Caption;

        public ErrorReportingViewModel()
        {
            TimeRangeButtonsModel = new TimeRangeButtonsViewModel();
            TimeRangeButtonsModel.OnTimeRangeChanged += OnTimeRangeChanged;
            _groupStatsCollection = new ObservableCollection<ErrorGroupItem>();
            GroupStatsView = new ListCollectionView(_groupStatsCollection);
            GetGroupStats();
        }

        private void OnTimeRangeChanged(object sender, EventArgs v)
        {
            GetGroupStats();
            RaisePropertyChanged(nameof(CurrentTimeRangeCaption));
        }

        private async Task GetGroupStats()
        {
            IsLoadingComplete = false;
            _groupStatsCollection.Clear();
            GroupStatsRequestResult results = null;
            ShowException = false;
            try
            {
                results = await SerDataSourceInstance.Instance.Value.ListGroupStatusAsync(
                    TimeRangeButtonsModel.SelectedTimeRangeItem.TimeRange,
                    TimeRangeButtonsModel.SelectedTimeRangeItem.TimedCountDuration);
            }
            catch (DataSourceException ex)
            {
                ShowException = true;
                ExceptionString = ex.ToString();       
            }
            finally
            {
                IsLoadingComplete = true;
            }

            AddItems(results?.GroupStats);
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

                _groupStatsCollection.Add(new ErrorGroupItem(item, TimeRangeButtonsModel.SelectedTimeRangeItem.TimeRange));
            }
        }
    }
}

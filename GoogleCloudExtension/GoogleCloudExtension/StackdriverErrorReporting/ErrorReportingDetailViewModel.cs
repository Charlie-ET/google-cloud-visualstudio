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
using System.Linq;


namespace GoogleCloudExtension.StackdriverErrorReporting
{
    public class EventItem
    {
        private readonly ErrorEvent _error;
        public string SummaryMessage { get; }
        public string Message => _error.Message;
        public object EventTime => _error.EventTime;

        public EventItem(ErrorEvent error)
        {
            _error = error;
            var splits = _error.Message?.Split(new string[] { Environment.NewLine, "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            SummaryMessage = splits?[0];
        }
    }

    public class ErrorReportingDetailViewModel : ViewModelBase
    {
        public TimeRangeButtonsViewModel TimeRangeButtonsModel { get; }

        public ErrorGroupItem GroupItem { get; private set; }

        public string Message => GroupItem?.Message;
        public string Stack => GroupItem?.ErrorGroup?.Representative?.Message;
        public string StakcSummary => GroupItem?.Stack;

        public string TimeRanges { get; private set; }

        public CollectionView EventItemCollection { get; private set; }

        public TimedCountBarChartViewModel BarChartModel => GroupItem?.BarChartModel;

        public ErrorReportingDetailViewModel()
        {
            TimeRangeButtonsModel = new TimeRangeButtonsViewModel();
            TimeRangeButtonsModel.OnTimeRangeChanged += (s, e) => UpdateView();
        }

        private IList<TimedCount> GenerateFakeRanges()
        {
            List<TimedCount> tCounts = new List<TimedCount>();
            for (int i = 10; i > 0; --i)
            {
                TimedCount t = new TimedCount();
                t.StartTime = DateTime.UtcNow.AddDays(-1 * i);
                t.EndTime = DateTime.UtcNow.AddDays(-1 * i + 1);
                t.Count = i;
                tCounts.Add(t);
            }

            return tCounts;
        }

        public void UpdateView(ErrorGroupItem errorGroupItem, TimeRangeItem selectedTimeRangeItem)
        {
            GroupItem = errorGroupItem;
            if (selectedTimeRangeItem.TimeRange == TimeRangeButtonsModel.SelectedTimeRangeItem.TimeRange)
            {
                UpdateView();
            }
            else
            {
                // This will end up calling UpdateView() too. 
                TimeRangeButtonsModel.OnTimeRangeCommand(TimeRangeButtonsModel.TimeRanges.First(x => x.TimeRange == selectedTimeRangeItem.TimeRange));
            }
        }

        public async void UpdateView()
        {
            var events = await SerDataSourceInstance.Instance.Value.ListEventsAsync(GroupItem.ErrorGroup, TimeRangeButtonsModel.SelectedTimeRangeItem.EventTimeRange);
            EventItemCollection = CollectionViewSource.GetDefaultView(events.ErrorEvents?.Select(x => new EventItem(x))) as CollectionView;

            //  It is necessary to notify the View to update binding sources.
            RaiseAllPropertyChanged();
        }
    }
}

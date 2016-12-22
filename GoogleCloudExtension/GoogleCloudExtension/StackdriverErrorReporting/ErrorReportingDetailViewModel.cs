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
using System.Linq;


namespace GoogleCloudExtension.StackdriverErrorReporting
{
    public class ErrorReportingDetailViewModel : ViewModelBase
    {
        public ErrorGroupItem GroupItem { get; private set; }

        public string Stack { get; private set; }

        public string TimeRanges { get; private set; }

        public CollectionView EventItemCollection { get; private set; }

        public ErrorReportingDetailViewModel()
        {
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

        public async void UpdateView(ErrorGroupItem errorGroupItem)
        {
            GroupItem = errorGroupItem;
            var events = await SerDataSourceInstance.Instance.Value.ListEventsAsync(errorGroupItem.ErrorGroup);
            Debug.Assert(events.ErrorEvents?.Count > 0);
            Stack = events.ErrorEvents?[0].Message;


            // TimeRanges = String.Join(" ", errorGroupItem.ErrorGroup.TimedCounts.Select(x => $"{x.StartTime}-{x.EndTime} {x.Count}"));
            TimeRanges = String.Join(" ", GenerateFakeRanges().Select(x => $"{x.StartTime}-{x.EndTime} {x.Count}"));
            EventItemCollection = CollectionViewSource.GetDefaultView(events.ErrorEvents) as CollectionView;

            //  It is necessary to notify the View to update binding sources.
            RaiseAllPropertyChanged();
            //RaisePropertyChanged(nameof(EventItemCollection));
        }
    }
}

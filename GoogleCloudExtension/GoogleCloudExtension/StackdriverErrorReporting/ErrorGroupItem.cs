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
using EventGroupTimeRangeEnum = Google.Apis.Clouderrorreporting.v1beta1.ProjectsResource.GroupStatsResource.ListRequest.TimeRangePeriodEnum;

namespace GoogleCloudExtension.StackdriverErrorReporting
{
    public class ErrorGroupItem : Model
    {
        public ErrorGroupStats ErrorGroup { get; set; }

        public ProtectedCommand NavigateDetailCommand { get; }
        public string Error => ErrorGroup.Representative.Message;
        public long Count => ErrorGroup.Count.GetValueOrDefault();
        public object FirstSeen => ErrorGroup.FirstSeenTime;
        public object LastSeen => ErrorGroup.LastSeenTime;
        public object SeenIn => "What is this?";
        public string Message { get; }
        public string Stack { get; }

        // TODO: not necessary?  remove.
        public EventGroupTimeRangeEnum EventGroupTimeRange { get; }

        public TimedCountBarChartViewModel BarChartModel { get; }

        public ErrorGroupItem(ErrorGroupStats errorGroup, EventGroupTimeRangeEnum groupTimeRange)
        {
            EventGroupTimeRange = groupTimeRange;
            ErrorGroup = errorGroup;
            string[] lines = ErrorGroup.Representative.Message.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            Message = lines?[0];
            Stack = lines?[1];
            NavigateDetailCommand = new ProtectedCommand(NavigateDetail);
            BarChartModel = new TimedCountBarChartViewModel(errorGroup.TimedCounts, groupTimeRange);
        }
        
        private void NavigateDetail()
        {
            Debug.WriteLine($"{Message} is clicked");
            if (ErrorReportingDetailToolWindowCommand.Instance == null)
            {
                var detailWindow = DetailWindow.Instance;
                detailWindow.Show();
                detailWindow.ViewModel.UpdateView(this, ErrorReportingViewModel.Instance.TimeRangeButtonsModel.SelectedTimeRangeItem); 
            }
            else
            {
                var detailWindow = ErrorReportingDetailToolWindowCommand.ShowWindow();
                detailWindow.ViewModel.UpdateView(this, ErrorReportingViewModel.Instance.TimeRangeButtonsModel.SelectedTimeRangeItem);
            }
        }
    }
}

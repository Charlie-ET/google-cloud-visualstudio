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

using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Clouderrorreporting.v1beta1;
using Google.Apis.Clouderrorreporting.v1beta1.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using TimeRangeEnum = Google.Apis.Clouderrorreporting.v1beta1.ProjectsResource.GroupStatsResource.ListRequest.TimeRangePeriodEnum;
using EventTimeRange = Google.Apis.Clouderrorreporting.v1beta1.ProjectsResource.EventsResource.ListRequest.TimeRangePeriodEnum;
using System.Diagnostics;
using System;

namespace GoogleCloudExtension.DataSources.ErrorReporting
{
    /// <summary>
    /// Data source that returns Google Cloud Stackdriver Error Reporting.
    /// </summary>
    public class SerDataSource : DataSourceBase<ClouderrorreportingService>
    {
        /// <summary>
        /// Initializes an instance of <seealso cref="SerDataSource"/> class.
        /// </summary>
        /// <param name="projectId">The project id that contains the GCE instances to manipulate.</param>
        /// <param name="credential">The credentials to use for the call.</param>
        public SerDataSource(string projectId, GoogleCredential credential, string appName)
                : base(projectId, credential, init => new ClouderrorreportingService(init), appName)
        {}

        public async Task<GroupStatsRequestResult> ListGroupStatusAsync(
            TimeRangeEnum timeRange, string timedCountDuration, string groupId = null)
        {

            var request = Service.Projects.GroupStats.List(ProjectIdQuery);
            request.TimeRangePeriod = timeRange;
            request.TimedCountDuration = timedCountDuration;
            request.GroupId = groupId;
            try
            {
                var response = await request.ExecuteAsync();
                return new GroupStatsRequestResult(response.ErrorGroupStats, response.NextPageToken);
            }
            catch (GoogleApiException ex)
            {
                Debug.WriteLine($"Failed to get GroupStats: {ex.Message}");
                throw new DataSourceException(ex.Message, ex);
            }
        }

        public async Task<ErrorEventsRequestResult> ListEventsAsync(ErrorGroupStats errorGroup,
            EventTimeRange period = EventTimeRange.PERIOD30DAYS)
        {
            var request = Service.Projects.Events.List(ProjectIdQuery);
            request.TimeRangePeriod = period;
            request.GroupId = errorGroup.Group.GroupId;
            try
            {
                var response = await request.ExecuteAsync();
                return new ErrorEventsRequestResult(response.ErrorEvents, response.NextPageToken);
            }
            catch (GoogleApiException ex)
            {
                Debug.WriteLine($"Failed to get ErrorEvents: {ex.Message}");
                throw new DataSourceException(ex.Message, ex);
            }
        }
    }
}
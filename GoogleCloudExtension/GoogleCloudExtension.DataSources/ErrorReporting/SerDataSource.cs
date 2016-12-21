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

using Google.Apis.Auth.OAuth2;
using Google.Apis.Clouderrorreporting.v1beta1;
using Google.Apis.Clouderrorreporting.v1beta1.Data;
using Google.Cloud.ErrorReporting.V1Beta1;
using GoogleCloudExtension.DataSources;
using System.Threading.Tasks;
using GroupStatsResource = Google.Apis.Clouderrorreporting.v1beta1.ProjectsResource.GroupStatsResource;
using System.Diagnostics;

namespace GoogleCloudExtension.DataSources.ErrorReporting
{
    /// <summary>
    /// Data source that returns Google Cloud Stackdriver Error Reporting.
    /// </summary>
    public class SerDataSource : DataSourceBase<ClouderrorreportingService>
    {
        private readonly GoogleCredential _credential;

        /// <summary>
        /// Initializes an instance of <seealso cref="SerDataSource"/> class.
        /// </summary>
        /// <param name="projectId">The project id that contains the GCE instances to manipulate.</param>
        /// <param name="credential">The credentials to use for the call.</param>
        public SerDataSource(string projectId, GoogleCredential credential, string appName)
                : base(projectId, credential, init => new ClouderrorreportingService(init), appName)
        {
            _credential = credential;
        }

        public async Task<GroupStatsRequestResult> ListGroupStatusAsync()
        {

            var request = Service.Projects.GroupStats.List($"projects/{ProjectId}");
            request.TimeRangePeriod = GroupStatsResource.ListRequest.TimeRangePeriodEnum.PERIOD30DAYS;
            var response = await request.ExecuteAsync();
            return new GroupStatsRequestResult(response.ErrorGroupStats, response.NextPageToken);
        }
    }
}

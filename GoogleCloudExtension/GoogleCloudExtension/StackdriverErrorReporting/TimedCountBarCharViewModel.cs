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
using System.Globalization;

namespace GoogleCloudExtension.StackdriverErrorReporting
{
    public class TimedCountItem : Model
    {
        public long ZIndex { get; }

        public bool IsVisible { get; }

        private readonly TimedCount _timedCount;

        public string YScale { get; }

        public uint BarHeight => (UInt32)(_timedCount.Count * TimedCountBarCharViewModel.HeightMultiplier);

        public TimedCountItem(TimedCount timedCount, bool isVisible)
        {
            _timedCount = timedCount;
            YScale = ((DateTime)(timedCount.StartTime)).ToString("MMM-dd");
            IsVisible = isVisible;
            ZIndex = (long)(DateTime.Now - ((DateTime)(timedCount.StartTime))).TotalSeconds;
        }
    }

    public class XLine : Model
    {
        public string XScale { get; }

        public uint RowHeight => TimedCountBarCharViewModel.RowHeight;

        public XLine(double xScale)
        {
            XScale = String.Format(((Math.Round(xScale) == xScale) ? "{0:0}" : "{0:0.00}"), xScale);
        }
    }

    public class TimedCountBarCharViewModel : ViewModelBase
    {
        public const uint RowNumber = 4;
        public const uint BarMaxHeight = 120;
        public static uint RowHeight => BarMaxHeight / RowNumber;
        public static double HeightMultiplier { get; private set; }

        public IList<TimedCountItem> TimedCountCollection { get; }

        public IList<XLine> XLines { get; }

        public TimedCountBarCharViewModel()
        {
            var timedCounts = GenerateFakeRanges();
            long maxCount = 0;
            TimedCountCollection = new List<TimedCountItem>();
            int k = 0;
            Debug.Assert(timedCounts.Count > 5);
            foreach (var counter in timedCounts)
            {
                bool isVisible = (k == 0 || k == timedCounts.Count-1 || k == timedCounts.Count / 3 || k == timedCounts.Count * 2 / 3);
                maxCount = Math.Max(counter.Count.Value, maxCount);
                TimedCountCollection.Add(new TimedCountItem(counter, isVisible));
                ++k;
            }
           
            HeightMultiplier = maxCount == 0 ? 0 : (double)BarMaxHeight / (double)maxCount;

            XLines = new List<XLine>();
            double xScaleUnit = (double)maxCount / RowNumber;
            for (uint i = RowNumber; i > 0; --i)
            {
                XLines.Add(new XLine(xScaleUnit * i));
            }
        }

        private IList<TimedCount> GenerateFakeRanges()
        {
            List<TimedCount> tCounts = new List<TimedCount>();
            for (int i = 10; i > 0; --i)
            {
                TimedCount t = new TimedCount();
                t.StartTime = DateTime.UtcNow.AddDays(-1 * i);
                t.EndTime = DateTime.UtcNow.AddDays(-1 * i + 1);
                t.Count = (i%3)*i;
                tCounts.Add(t);
            }

            return tCounts;
        }
    }
}

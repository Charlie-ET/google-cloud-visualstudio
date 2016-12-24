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

    public class MultiplyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double v0 = (double)value;
            int v1;
            int.TryParse(parameter as string, out v1);
            var ret = (int)(v0 * v1);
            return ret;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class TimedCountItem : Model
    {
        private readonly TimedCount _timedCount;

        private long Count => _timedCount.Count.GetValueOrDefault();

        public bool ShowYScale { get; }

        public string YScale { get; }

        public string ToolTipMessage => $"{Count} times in {"1 day"} {Environment.NewLine} Starting from {_timedCount.StartTime}.";

        public int BarHeight { get; }

        public double BarHeightRatio { get; }

        public TimedCountItem(TimedCount timedCount, bool isYScaleVisible, double heightMultiplier, double countScaleMultiplier)
        {
            _timedCount = timedCount;
            ShowYScale = isYScaleVisible;
            YScale = isYScaleVisible ? ((DateTime)(timedCount.StartTime)).ToString("MMM-dd") : null;
            BarHeight = (int)(Count * heightMultiplier);
            BarHeightRatio = Count * countScaleMultiplier;
        }
    }

    public class XLine : Model
    {
        public string XScale { get; }

        public int RowHeight => TimedCountBarChartViewModel.RowHeight;

        public XLine(double xScale)
        {
            XScale = String.Format(((Math.Round(xScale) == xScale) ? "{0:0}" : "{0:0.00}"), xScale);
        }
    }

    public class TimedCountBarChartViewModel : ViewModelBase
    {
        public const int RowNumber = 4;
        public const double BarMaxHeight = 120.00;
        public static int RowHeight => (int)(BarMaxHeight / RowNumber);
        private double heightMultiplier;
        private double countScaleMultiplier;

        public IList<TimedCountItem> TimedCountCollection { get; }

        public IList<XLine> XLines { get; }

        public TimedCountBarChartViewModel(IList<TimedCount> timedCounts)
        {
            // TODO : test if timedCounts == null;
            if (timedCounts == null)
            {
                return;
            }

            long maxCount = timedCounts.Max(x => x.Count.GetValueOrDefault());
            heightMultiplier = maxCount == 0 ? 0 : BarMaxHeight / maxCount;
            countScaleMultiplier = maxCount == 0 ? 0 : 1.00 / maxCount;

            TimedCountCollection = new List<TimedCountItem>();
            int k = 0;
            Debug.Assert(timedCounts.Count > 5);
            foreach (var counter in timedCounts)
            {
                bool isVisible = (k == 0 || k == timedCounts.Count - 3 || k == timedCounts.Count / 3 || k == timedCounts.Count * 2 / 3);
                maxCount = Math.Max(counter.Count.GetValueOrDefault(), maxCount);
                TimedCountCollection.Add(new TimedCountItem(counter, isVisible, heightMultiplier, countScaleMultiplier));
                ++k;
            }

            XLines = new List<XLine>();
            double xScaleUnit = (double)maxCount / RowNumber;
            for (int i = RowNumber; i > 0; --i)
            {
                XLines.Add(new XLine(xScaleUnit * i));
            }
        }

        public TimedCountBarChartViewModel(): this(GenerateFakeRanges())
        {
        }

        private static IList<TimedCount> GenerateFakeRanges()
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

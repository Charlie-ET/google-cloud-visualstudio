﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleCloudExtension.CloudExplorerSources.Gce
{
    public class GceGaeInstanceItem : GceInstanceItem
    {
        private const string GaeCategory = "AppEngine Properties";

        public GceGaeInstanceItem(GceInstance instance): base(instance)
        { }

        [Category(GaeCategory)]
        [Description("The AppEngine module for the instance.")]
        public string Module => _instance.GetGaeModule();

        [Category(GaeCategory)]
        [Description("The version of the module for the instance.")]
        public string Version => _instance.GetGaeVersion();
    }
}
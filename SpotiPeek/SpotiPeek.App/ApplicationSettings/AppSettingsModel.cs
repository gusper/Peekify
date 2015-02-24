﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SpotiPeek.App.ApplicationSettings
{
    [DataContract]
    public class AppSettingsModel
    {
        [DataMember]
        public int LocationX { get; set; }

        [DataMember]
        public int LocationY { get; set; }
    }
}
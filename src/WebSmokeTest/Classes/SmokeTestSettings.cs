using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppSettings;

namespace SmokeTest.Classes
{
    public class SmokeTestSettings
    {
        public bool AutoUpdatePage { get; set; }

        [SettingDefault(8000)]
        public int AutoUpdateFrequency { get; set; }
    }
}

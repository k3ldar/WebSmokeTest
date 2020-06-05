using AppSettings;

namespace SmokeTest.Internal
{
    public class SmokeTestSettings
    {
        public bool AutoUpdatePage { get; set; }

        [SettingDefault(8000)]
        public int AutoUpdateFrequency { get; set; }
    }
}

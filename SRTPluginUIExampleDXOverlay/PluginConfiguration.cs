namespace SRTPluginUIExampleDXOverlay
{
    public class PluginConfiguration
    {
        public bool Debug { get; set; }
        public bool ShowMoney { get; set; }
        public bool ShowKudos { get; set; }
        public bool ShowConvictions { get; set; }
        public float ScalingFactor { get; set; }
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public string StringFontName { get; set; }
        public float FontSize { get; set; }
        public string MoneyString { get; set; }
        public string KudosString { get; set; }
        public string LibertyString { get; set; }
        public string UtilityString { get; set; }
        public string MoralityString { get; set; }

        public PluginConfiguration()
        {
            Debug = false;
            ShowMoney = true;
            ShowConvictions = true;
            Debug = true;
            ScalingFactor = 1f;
            PositionX = 0f;
            PositionY = 948f;
            StringFontName = "Courier New";
            FontSize = 16f;
            MoneyString = "MON";
            KudosString = "KUD";
            LibertyString = "LIB";
            UtilityString = "UTL";
            MoralityString = "MOR";
        }
    }
}
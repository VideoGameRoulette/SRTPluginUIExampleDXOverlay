namespace SRTPluginUIExampleDXOverlay
{
    public class PluginConfiguration
    {
        public bool Debug { get; set; }
        public float ScalingFactor { get; set; }
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public string StringFontName { get; set; }
        public string MoneyString { get; set; }
        public string KudosString { get; set; }
        public string LibertyString { get; set; }
        public string UtilityString { get; set; }
        public string MoralityString { get; set; }

        public PluginConfiguration()
        {
            Debug = false;
            ScalingFactor = 1f;
            PositionX = 5f;
            PositionY = 300f;
            StringFontName = "Courier New";
            MoneyString = "Money:";
            KudosString = "Kudos:";
            LibertyString = "Liberty:";
            UtilityString = "Utility:";
            MoralityString = "Morality:";
        }
    }
}

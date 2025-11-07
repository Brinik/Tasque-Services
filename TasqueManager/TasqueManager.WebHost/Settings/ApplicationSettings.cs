namespace TasqueManager.WebHost.Settings
{
    public class ApplicationSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public RmqSettings? RmqSettings { get; set; }
    }
}

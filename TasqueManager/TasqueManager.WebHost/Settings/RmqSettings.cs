namespace TasqueManager.WebHost.Settings
{    
    public class RmqSettings
    {
        public string Host { get; set; } = string.Empty;
        public string VHost { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
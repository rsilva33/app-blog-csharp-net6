namespace Blog;

public static class Configuration
{
    public static string JwtKey = "ASklasN426BNf@7M9S6F78Bf#io76A$609Vtycnb6w=";
    public static string ApiKey = "course_api_IlTevUNU/z09utyi22vC/dhu3S@==";
    public static string ApiKeyName = "api_key";
    public static SmtpConfiguration Smtp = new();

    public class SmtpConfiguration
    {
        public string Host { get; set; }
        public int Port { get; set; } = 25;
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}

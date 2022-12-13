namespace CourseWebAPI.Settings
{
    public class JwtSettings
    {
        public string Secret { get; set; }
        public TimeSpan LifeTime { get; set; }
    }
}

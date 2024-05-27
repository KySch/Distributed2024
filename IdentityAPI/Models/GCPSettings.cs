namespace IdentityAPI.Models
{
    public class GCPSettings
    {
        public string Topic { get; set; } = null!;
        public string Sub { get; set; } = null!;

        public string Project { get; set; } = null!;
    }
}

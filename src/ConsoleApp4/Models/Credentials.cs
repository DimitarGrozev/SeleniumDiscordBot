using System.Text.Json.Serialization;

namespace ConsoleApp4.Models
{
    public class Credentials
    {
        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }
    }
}

using System.Text.Json.Serialization;

namespace Data.Models;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    
    [JsonIgnore]
    public string Password { get; set; }
}
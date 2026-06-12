using Microsoft.Identity.Client;

namespace Models.User;
public class User
{
    public int Id {get; set;}
    public string Username {get; set;} = "";
    public string Email {get; set;} = "";
    public string Password {get; set;} = "";
    public string Role {get; set;} = "User";
    public string? ResetToken {get; set;} = "";
    public DateTime? resetTokenTimer {get; set;} = DateTime.UtcNow;
    public byte resetTokenAttempts {get; set;} = 0;
}
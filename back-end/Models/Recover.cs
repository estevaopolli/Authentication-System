using System.ComponentModel.DataAnnotations;

namespace Models.Recover;

public class RecoverUser
{
    public string Email {get; set;} = "";
    public string ResetToken {get; set;} = "";
    public string NewPassword {get; set;} = "";
    public string ConfirmNewPassword {get; set;} = "";
}
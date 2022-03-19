using System.ComponentModel.DataAnnotations;

namespace JBS.Model.RequestModels.Auth;

public class AccountResetModel
{
    [Required]
    public string Key { get; set; }
    [Required]
    public string Secret { get; set; }
    [Required]
    public string NewPassword { get; set; }
    [Required]
    [Compare("NewPassword")]
    public string ConfirmPassword { get; set; }
}


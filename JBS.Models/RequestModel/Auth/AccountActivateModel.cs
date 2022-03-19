using System.ComponentModel.DataAnnotations;

namespace JBS.Model.RequestModels.Auth;
public class AccountActivateModel
{
    [Required]
    public string Key { get; set; }
    [Required]
    public string Secret { get; set; }
    [Required]
    public string Password { get; set; }
    [Required]
    [Compare("Password")]
    public string ConfirmPassword { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace JBS.Model.RequestModels.Auth;
public class AccountRecoverModel
{
    [Required]
    public string Email { get; set; }
}

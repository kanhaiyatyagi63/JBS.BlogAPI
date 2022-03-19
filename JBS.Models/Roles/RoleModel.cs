using System.ComponentModel.DataAnnotations;
namespace JBS.Model.Roles;
public class RoleModel
{
    public string Id { get; set; }
    [Required]
    public string Name { get; set; }
}

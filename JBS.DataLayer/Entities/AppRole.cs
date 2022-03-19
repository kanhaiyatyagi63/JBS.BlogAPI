using JBS.DataLayer.Entities;
using Microsoft.AspNetCore.Identity;

namespace JBS.DataLayer;
public class AppRole : IdentityRole
{
    public string Description { get; set; }
    public bool IsSystemGenerated { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public string CreatedBy { get; set; }
    public string UpdatedBy { get; set; }
    public ICollection<AppUserRole> UserRoles { get; set; }

}

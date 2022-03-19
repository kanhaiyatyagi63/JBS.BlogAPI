using JBS.DataLayer.Entities;
using JBS.Utility.Enumerations;
using Microsoft.AspNetCore.Identity;

namespace JBS.DataLayer;
public class AppUser : IdentityUser
{
    public long UserId { get; set; }
    public string Name { get; set; }
    public string FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string LastName { get; set; }
    public string? Title { get; set; }
    public string? Country { get; set; }
    public string? Dob { get; set; }
    public string? ProfileImage { get; set; }
    public UserType UserType { get; set; }
    public DateTime? LastLoggedOn { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsActive { get; set; }
    public bool IsSystemGenerated { get; set; }
    public ICollection<AppUserRole> UserRoles { get; set; }
    public ICollection<Post> Posts { get; set; }
}

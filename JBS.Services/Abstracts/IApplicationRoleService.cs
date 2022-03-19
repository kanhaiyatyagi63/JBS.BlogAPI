using JBS.DataLayer;
using JBS.Model.Shared;
using Microsoft.AspNetCore.Identity;

namespace JBS.Service.Abstracts
{
    public interface IApplicationRoleService
    {
        IEnumerable<AppRole> Roles { get; }
        Task<AppRole> FindByNameAsync(string roleName);
        Task<AppRole> FindByIdAsync(string roleId);
        Task<IdentityResult> UpdateAsync(AppRole role);
        Task<IdentityResult> CreateAsync(AppRole role);
        Task<IEnumerable<AppRole>> GetRoles(IEnumerable<string> roleNames);
        Task<IEnumerable<SelectListItem<string>>> GetRoles();
        Task DeleteAsync(string[] roleIds);
        Task UnDeleteRoles(string[] roleIds);
    }
}

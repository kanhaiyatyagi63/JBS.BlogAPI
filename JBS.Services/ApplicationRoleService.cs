using AutoMapper;
using JBS.DataLayer;
using JBS.DataLayer.Abstracts;
using JBS.Model.Shared;
using JBS.Service.Abstracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JBS.Service;

public class ApplicationRoleService : IApplicationRoleService
{
    private readonly RoleManager<AppRole> _roleManager;
    private readonly UserManager<AppUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ApplicationRoleService(
        IUnitOfWork unitOfWork,
        RoleManager<AppRole> roleManager,
        UserManager<AppUser> userManager,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _roleManager = roleManager;
        _mapper = mapper;
        _userManager = userManager;
    }

    public IEnumerable<AppRole> Roles => _roleManager.Roles;

    public async Task<IdentityResult> CreateAsync(AppRole role)
    {
        return await _roleManager.CreateAsync(role);
    }

    public async Task<IdentityResult> UpdateAsync(AppRole role)
    {
        return await _roleManager.UpdateAsync(role);
    }

    public async Task<AppRole> FindByNameAsync(string roleName)
    {
        return await _roleManager.FindByNameAsync(roleName);
    }
    public async Task<AppRole> FindByIdAsync(string roleId)
    {
        return await _roleManager.FindByIdAsync(roleId);
    }

    public async Task<IEnumerable<AppRole>> GetRoles(IEnumerable<string> roleNames)
    {
        var roles = await _roleManager.Roles
            .Where(x => x.IsActive && x.IsDeleted == false && roleNames.Contains(x.Name))
            .ToListAsync();

        return roles;
    }

    private async Task<string> GetUniqueUserName(string name)
    {
        //_logger.LogDebug("GetUnique User Name Started");

        long counter = 0;
        string generatedUserName = GenerateUserName(name);
        var originalGeneratedUserName = generatedUserName.Clone();

        var userNames = await _roleManager.Roles.Where(x => x.NormalizedName.StartsWith(generatedUserName))
            .Select(x => x.NormalizedName)
            .OrderBy(x => x)
            .ToListAsync();

        while (userNames.Any(x => x.Equals(generatedUserName)))
        {
            generatedUserName = string.Concat(originalGeneratedUserName, ++counter);
        }
        //_logger.LogDebug("GetUnique User Name Completed");
        return generatedUserName;
    }

    private string GenerateUserName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        string generatedUserName;

        //Break string into words
        var words = name.Split(" ", StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 1)
        {
            generatedUserName = name;
        }
        else
        {
            generatedUserName = string.Concat(words.First()[0], words.Last());
            ////All except last
            //var exceptLast = words.Take(words.Length - 1);
            //generatedUserName = string.Concat(string.Join("", exceptLast.Select(x => x[0])), words.Last());
        }

        return generatedUserName.ToLowerInvariant();
    }

    public async Task<IEnumerable<SelectListItem<string>>> GetRoles()
    {
        return await _roleManager.Roles.Where(x => x.IsActive && !x.IsDeleted).Select(x => new SelectListItem<string>
        {
            Id = x.Id,
            Value = x.Name
        }).ToListAsync();
    }


    public async Task DeleteAsync(string[] roleIds)
    {
        foreach (var roleId in roleIds)
        {
            var role = _roleManager.Roles
                                   .FirstOrDefault(x => x.Id == roleId);
            if (role == null)
                throw new Exception("Roles deletion failed. Error: Invalid role");

            if (await IsUserAssociatedWithRole(role.Name))
                throw new Exception($"Roles deletion failed. Error: {role.Name} associated with users");

            role.IsDeleted = true;

        }

        await _unitOfWork.CommitAsync();
    }

    public async Task UnDeleteRoles(string[] roleIds)
    {
        foreach (var roleId in roleIds)
        {
            var role = _roleManager.Roles
                                   .FirstOrDefault(x => x.Id == roleId);
            if (role == null)
                throw new Exception("Roles deletion failed. Error: Invalid role");


            role.IsDeleted = false;

        }

        await _unitOfWork.CommitAsync();
    }

    private async Task<bool> IsUserAssociatedWithRole(string roleName)
    {
        return (await _userManager.GetUsersInRoleAsync(roleName)).Any();
    }


}

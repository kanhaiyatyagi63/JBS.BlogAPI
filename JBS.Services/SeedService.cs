using JBS.DataLayer;
using JBS.DataLayer.Abstracts;
using JBS.Service.Abstracts;
using JBS.Utility.Enumerations;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace JBS.Service
{
    public class SeedApplicationRole
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string NormalizedName { get; internal set; }
    }

    public class SeedService : ISeedService
    {
        private const string adminRole = "IT Admin";
        private readonly IApplicationUserService _applicationUserService;

        private readonly IApplicationRoleService _roleService;
        private readonly ILogger<SeedService> _logger;
        private readonly IUnitOfWork _unitOfWork;


        public SeedService(IApplicationRoleService roleService,
            ILogger<SeedService> logger, IUnitOfWork unitOfWork,
            IApplicationUserService applicationUserService)
        {
            _roleService = roleService;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _applicationUserService = applicationUserService;
        }
        public async Task SeedAsync()
        {
            await SeedRolesAsync();
            await SeedAdminUserAsync();
        }

        private async Task SeedAdminUserAsync()
        {
            try
            {
                if (!_roleService.Roles.Any())
                {
                    _logger.LogError("Admin user seed failed, no role found");
                }

                var user = await _applicationUserService.FindByNameAsync("admin");
                if (user == null)
                {
                    user = new AppUser
                    {
                        Name = "Kanhaiya Tyagi",
                        UserName = "kanhaiyatyagi63",
                        FirstName = "Kanhaiya",
                        LastName = "Tyagi",
                        Email = "kanhaiyatyagi63@gmail.com",
                        EmailConfirmed = true,
                        IsActive = true,
                        UserType = UserType.SuperAdmin,
                        CreatedDate = DateTime.UtcNow,
                        UpdatedDate = DateTime.UtcNow,
                        IsSystemGenerated = true
                    };

                    var result = await _applicationUserService.CreateAsync(user, "Password@123");
                    if (!result.Succeeded)
                    {
                        _logger.LogError("Admin user seed failed");
                        _logger.LogError(string.Join(",", result.Errors.Select(x => x.Description)));
                        return;
                    }

                    user = await _applicationUserService.FindByNameAsync("kanhaiyatyagi63");
                }

                var isInRole = await _applicationUserService.IsInRoleAsync(user, adminRole);
                if (!isInRole)
                {
                    var result = await _applicationUserService.AddToRoleAsync(user, adminRole);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("Admin user seed completed");
                        return;
                    }

                    _logger.LogError("Admin user seed failed");
                    _logger.LogError(string.Join(",", result.Errors.Select(x => x.Description)));

                    await _applicationUserService.DeleteAsync(user);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in system user seeding {ex}");
            }
        }

        private async Task SeedRolesAsync()
        {
            var roles = new List<SeedApplicationRole>
            {
                new SeedApplicationRole()
                {
                    Name = adminRole,
                    Description = "Administrator of System",
                    NormalizedName = "JBS ADMIN",
                }
            };
            foreach (var role in roles)
            {
                try
                {
                    {
                        IdentityResult identityResult;
                        var applicationRole = await _roleService.FindByNameAsync(role.Name);
                        if (applicationRole == null)
                        {
                            identityResult = await _roleService.CreateAsync(new AppRole
                            {
                                Name = role.Name,
                                NormalizedName = role.NormalizedName,
                                IsSystemGenerated = true,
                                IsActive = true,
                                Description = role.Description
                            });

                            _logger.LogInformation($"Role ({role.Name}) seed result: {identityResult}");

                            if (identityResult.Succeeded)
                            {
                                applicationRole = await _roleService.FindByNameAsync(role.Name);

                                await _unitOfWork.CommitAsync();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error in Seeding Role {role.Name}");
                }
            }
        }
    }
}

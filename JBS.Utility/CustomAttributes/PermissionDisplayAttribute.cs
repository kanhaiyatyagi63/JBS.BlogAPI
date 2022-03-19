using JBS.Utility.Enumerations;

namespace JBS.Utility.CustomAttributes;
public class PermissionDisplayAttribute : Attribute
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string GroupName { get; private set; }
    public UserType[] PermissionFor { get; private set; }


    public PermissionDisplayAttribute(string name, string description, string group, params UserType[] permissionFor)
    {
        this.Name = name;
        this.Description = description;
        this.GroupName = group;
        this.PermissionFor = permissionFor;
    }
}
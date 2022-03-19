using JBS.Utility.CustomAttributes;

namespace JBS.Utility.Enumerations;
internal enum Permission
{
    [PermissionDisplay("Add Post", "Any user are able to add post", "Post related permission", UserType.SuperAdmin, UserType.Admin, UserType.User)]
    AddPost = 1,
    [PermissionDisplay("Edit Post", "Any user are able to edit post", "Post related permission", UserType.SuperAdmin, UserType.Admin, UserType.User)]
    EditPost = 2,
    [PermissionDisplay("View Post", "Any user are able to view post", "Post related permission", UserType.SuperAdmin, UserType.Admin, UserType.User)]
    ViewPost = 3,
}

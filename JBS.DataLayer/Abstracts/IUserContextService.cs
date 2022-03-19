namespace JBS.DataLayer.Abstracts;
public interface IUserContextService
{
    string? GetUserId();
    string? GetUserRole();
    string? GetIpAddress();
}

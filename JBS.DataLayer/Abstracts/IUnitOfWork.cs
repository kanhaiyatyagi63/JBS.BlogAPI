namespace JBS.DataLayer.Abstracts;
public interface IUnitOfWork
{
    void BeginTransaction();
    int SaveChanges();
    Task<int> SaveChangesAsync();
    void Commit();
    Task CommitAsync();
    void Rollback();
}

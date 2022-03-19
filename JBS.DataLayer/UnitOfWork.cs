using JBS.DataLayer.Abstracts;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace JBS.DataLayer;
public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly DataContext _dataContext;
    private IDbContextTransaction _dbTransaction;
    private readonly ILogger<UnitOfWork> _logger;

    public UnitOfWork(DataContext dataContext,
          ILogger<UnitOfWork> logger)
    {
        _dataContext = dataContext;
        _logger = logger;
    }

    public void BeginTransaction()
    {
        _dbTransaction = _dataContext.Database.BeginTransaction();
    }

    public int SaveChanges()
    {
        return _dataContext.SaveChanges();
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _dataContext.SaveChangesAsync();
    }

    public void Commit()
    {
        _dbTransaction.Commit();
    }
    public async Task CommitAsync()
    {
        try
        {
            await _dataContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occured on SaveChanges.");
            throw;
        }
    }
    public void Rollback()
    {
        _dbTransaction.Rollback();
    }
    void IDisposable.Dispose()
    {
        if (_dataContext != null)
        {
            _dataContext.Dispose();
        }
    }
}

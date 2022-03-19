namespace JBS.DataLayer.Entities.Base;
public class BaseEntity<TKey> : IBaseEntity<TKey>
{
    public TKey Id { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}

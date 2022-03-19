using JBS.DataLayer.Entities.Base;

namespace JBS.DataLayer.Entities
{
    public class PostTag : BaseEntity<int>
    {
        public Guid PostId { get; set; }
        public int TagId { get; set; }
        public virtual Tag Tag { get; set; }
        public virtual Post Post { get; set; }
    }
}

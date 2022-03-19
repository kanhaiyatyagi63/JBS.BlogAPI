using JBS.DataLayer.Entities.Base;

namespace JBS.DataLayer.Entities
{
    public class PostCategory : BaseEntity<int>
    {
        public Guid PostId { get; set; }
        public int CategoryId { get; set; }
        public virtual Category Category { get; set; }
        public virtual Post Post { get; set; }

    }
}

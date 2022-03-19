using JBS.DataLayer.Entities.Base;

namespace JBS.DataLayer.Entities
{
    public class PostMeta : BaseEntity<int>
    {
        public Guid PostId { get; set; }
        public string Key { get; set; }
        public string Content { get; set; }
        public Post Post { get; set; }
    }
}

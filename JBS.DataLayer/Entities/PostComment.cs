using JBS.DataLayer.Entities.Base;

namespace JBS.DataLayer.Entities
{
    public class PostComment : BaseEntity<int>
    {
        public Guid PostId { get; set; }
        public int? ParentId { get; set; } // for comment chaining
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime PublishedAt { get; set; }
        public bool IsPublished { get; set; }
    }
}

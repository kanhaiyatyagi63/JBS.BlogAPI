using JBS.DataLayer.Entities.Base;

namespace JBS.DataLayer.Entities
{
    public class Tag : BaseEntity<int>
    {
        public string Title { get; set; }
        public string? MetaTitle { get; set; }
        public string? Slug { get; set; }
        public string? Content { get; set; }
        public virtual ICollection<PostTag> PostTags { get; set; }
    }
}

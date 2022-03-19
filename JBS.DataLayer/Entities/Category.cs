using JBS.DataLayer.Entities.Base;

namespace JBS.DataLayer.Entities
{
    public class Category : BaseEntity<int>
    {
        public string Title { get; set; }
        public string? MetaTitle { get; set; }
        public int? ParentId { get; set; }
        public string? Slug { get; set; }
        public string Content { get; set; }
        public virtual ICollection<PostCategory> PostCategories { get; set; }

    }
}

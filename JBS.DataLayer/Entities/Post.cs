using JBS.DataLayer.Entities.Base;

namespace JBS.DataLayer.Entities
{
    public class Post : BaseEntity<Guid>
    {
        public string AuthorId { get; set; }
        public Guid? ParentId { get; set; }
        public string Title { get; set; }
        public string? MetaTitle { get; set; }
        public string? Slug { get; set; }
        public string? Content { get; set; }
        public string? Summary { get; set; }
        public DateTime PublishedAt { get; set; }
        public bool IsPublished { get; set; }
        public virtual AppUser Author { get; set; }
        public virtual ICollection<PostCategory> PostCategories { get; set; }
        public virtual ICollection<PostTag> PostTags { get; set; }

    }
}

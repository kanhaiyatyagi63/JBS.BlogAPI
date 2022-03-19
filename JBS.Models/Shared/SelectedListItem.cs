namespace JBS.Model.Shared
{
    public class SelectListItem<T>
    {
        public T Id { get; set; }

        public string Value { get; set; }

        public bool IsSelected { get; set; }
    }
}

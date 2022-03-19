namespace JBS.Model.JqDataTableModels;
public class JqDataTableResponse<TEntity> where TEntity : class
{
    public int Draw { get; set; }

    public int RecordsTotal { get; set; }
    public IEnumerable<string> VisibleColumns { get; set; }

    public int PageLength { get; set; }

    public int RecordsFiltered { get; set; }

    public IEnumerable<TEntity> Data { get; set; }

    public string Error { get; set; }
}

using System.Linq.Expressions;

namespace JBS.Model.JqDataTableModels;
public class JqDataTableRequest
{
    public int Draw { get; set; }

    public int Start { get; set; }

    public int Length { get; set; }

    public JqDataTableSearch Search { get; set; }

    public List<JqDataTableSort> Order { get; set; }

    public List<JqDataTableColumn> Columns { get; set; }

    public bool IsDtInitialized { get; set; }

    public string GetSortExpression()
    {
        var columnIndex = Order.FirstOrDefault()?.Column ?? 0;
        var sortDir = Order.FirstOrDefault()?.Dir ?? "asc";
        var columnName = Columns[columnIndex].Data;
        return $"{columnName} {sortDir}";
    }

    public IQueryable<T> OrderByDynamic<T>(IQueryable<T> query)
    {
        var columnIndex = Order.FirstOrDefault()?.Column ?? 0;
        var sortDir = Order.FirstOrDefault()?.Dir ?? "asc";
        var columnName = Columns[columnIndex].Data;

        bool desc = sortDir != "asc";
        var queryElementTypeParam = Expression.Parameter(typeof(T));

        var memberAccess = Expression.PropertyOrField(queryElementTypeParam, columnName);

        var keySelector = Expression.Lambda(memberAccess, queryElementTypeParam);

        var orderBy = Expression.Call(
            typeof(Queryable),
            desc ? "OrderByDescending" : "OrderBy",
            new Type[] { typeof(T), memberAccess.Type },
            query.Expression,
            Expression.Quote(keySelector));

        return query.Provider.CreateQuery<T>(orderBy);
    }
}

using System.Data;
using Dapper;

namespace Kcow.Infrastructure.Database;

/// <summary>
/// Dapper type handler for System.TimeOnly, which is not natively supported.
/// Stores as string in HH:mm:ss format in SQLite.
/// </summary>
public sealed class TimeOnlyTypeHandler : SqlMapper.TypeHandler<TimeOnly>
{
    public override TimeOnly Parse(object value)
    {
        if (value is string str && TimeOnly.TryParse(str, out var time))
            return time;
        return default;
    }

    public override void SetValue(IDbDataParameter parameter, TimeOnly value)
    {
        parameter.DbType = DbType.String;
        parameter.Value = value.ToString("HH:mm:ss");
    }
}

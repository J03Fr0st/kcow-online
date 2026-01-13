namespace Kcow.Infrastructure.Sql;

public static class SchoolQueries
{
    public const string GetAll = "SELECT * FROM schools";
    
    public const string GetById = "SELECT * FROM schools WHERE id = @Id";
    
    public const string GetActive = "SELECT * FROM schools WHERE is_active = 1";
}

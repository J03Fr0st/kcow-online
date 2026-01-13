namespace Kcow.Infrastructure.Sql;

public static class UserQueries
{
    public const string GetById = @"
        SELECT * FROM Users
        WHERE Id = @Id";

    public const string GetByEmail = @"
        SELECT * FROM Users
        WHERE Email = @Email";

    public const string GetAllWithRoles = @"
        SELECT u.*, r.*
        FROM Users u
        JOIN Roles r ON u.RoleId = r.Id";
        
    public const string GetByIdWithRole = @"
        SELECT u.*, r.*
        FROM Users u
        JOIN Roles r ON u.RoleId = r.Id
        WHERE u.Id = @Id";
}

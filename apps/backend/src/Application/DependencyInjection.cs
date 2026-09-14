using Kcow.Application.Activities;
using Kcow.Application.Attendance;
using Kcow.Application.Audit;
using Kcow.Application.ClassGroups;
using Kcow.Application.Evaluations;
using Kcow.Application.Families;
using Kcow.Application.Schools;
using Kcow.Application.Students;
using Kcow.Application.Trucks;
using Microsoft.Extensions.DependencyInjection;

namespace Kcow.Application;

/// <summary>
/// Extension methods for configuring Application services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Application services to the DI container.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<Billing.IBillingService, Billing.BillingService>();
        services.AddScoped<IActivityService, ActivityService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IClassGroupService, ClassGroupService>();
        services.AddScoped<IEvaluationService, EvaluationService>();
        services.AddScoped<IFamilyService, FamilyService>();
        services.AddScoped<ISchoolService, SchoolService>();
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<ITruckService, TruckService>();
        return services;
    }
}

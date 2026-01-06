namespace Kcow.Domain.Entities;

public class ClassGroup
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SchoolId { get; set; }
    public int? TruckId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int Sequence { get; set; } = 1;
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public School School { get; set; } = null!;
    public Truck? Truck { get; set; }
    public ICollection<Student> Students { get; set; } = new List<Student>();
}

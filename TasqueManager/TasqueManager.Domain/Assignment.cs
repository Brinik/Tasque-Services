namespace TasqueManager.Domain
{
    public enum AssignmentStatus 
    {
        None = 0,
        New,
        InProgress,
        Completed,
        Overdue
    }
    public class Assignment: IEntity<Guid>
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }

        public AssignmentStatus Status { get; set; } = AssignmentStatus.New;

        public bool Deleted { get; set; }
    }
}

public interface IReadonlyTaskManagementInfo
{
    public int ReadyToAssignCount { get; }
    public int AssignCount { get; }
    public int CompletedCount { get; }
}

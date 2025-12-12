public class TaskManagementInfo : IReadonlyTaskManagementInfo
{
    public int ReadyToAssignCount { get; set; }
    public int AssignCount { get; set; }
    public int CompletedCount { get; set; }

    public void SynchronizeReadyAssignValue(int value)
    {
        ReadyToAssignCount = value;
    }

    public void MoveToAssigned()
    {
        ReadyToAssignCount--;
        AssignCount++;
    }

    public void MoveToCompleted()
    {
        AssignCount--;
        CompletedCount++;
    }
}
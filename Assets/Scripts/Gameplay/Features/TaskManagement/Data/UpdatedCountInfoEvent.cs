public class UpdatedCountInfoEvent : IEvent
{
    public UpdatedCountInfoEvent(IReadonlyTaskManagementInfo taskManagementInfo)
    {
        TaskManagementInfo = taskManagementInfo;
    }

    public IReadonlyTaskManagementInfo TaskManagementInfo { get; }
}
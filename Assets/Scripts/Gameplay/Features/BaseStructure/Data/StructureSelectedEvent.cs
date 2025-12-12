public class StructureSelectedEvent : IEvent
{
    public StructureSelectedEvent(Structure selectedStructure)
    {
        SelectedStructure = selectedStructure;
    }

    public Structure SelectedStructure { get; }
}
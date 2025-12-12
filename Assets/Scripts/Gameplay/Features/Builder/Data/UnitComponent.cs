public class UnitComponent<T> : IBehaviourComponent
{
    public UnitComponent(T unit)
    {
        Unit = unit;
    }

    public T Unit { get; set; }
}
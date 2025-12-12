using System.Collections.Generic;
using System.Linq;

public readonly struct BehaviourAddContext<T>
{
    public BehaviourAddContext(
        BuildBehaviourContext<T> buildBehaviourContext,
        TimeBehaviourContext timeBehaviourContext,
        IEnumerable<IBaseBehaviour> additionalBehaviours = null)
    {
        BuildBehaviourContext = buildBehaviourContext;
        TimeBehaviourContext = timeBehaviourContext;
        AdditionalBehaviours = (additionalBehaviours?.ToList() ?? new List<IBaseBehaviour>()).AsReadOnly();
    }

    public BuildBehaviourContext<T> BuildBehaviourContext { get; }
    public TimeBehaviourContext TimeBehaviourContext { get; }
    public IReadOnlyList<IBaseBehaviour> AdditionalBehaviours { get; }
}
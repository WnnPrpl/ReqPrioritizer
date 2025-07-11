namespace ReqPrioritizer
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]

    public class PriorityAttribute : Attribute
    {
        public string Priority { get; }

        public PriorityAttribute(string priority)
        {
            Priority = priority;
        }
    }
}

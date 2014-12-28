namespace UnityAutoMoq.Tests
{
    public abstract class AbstractServiceWithAmbiguousConstructor
    {
        public AbstractServiceWithAmbiguousConstructor(IService service) { }
        public AbstractServiceWithAmbiguousConstructor(IAnotherService service) { }
    }
}

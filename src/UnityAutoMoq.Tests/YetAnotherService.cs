namespace UnityAutoMoq.Tests
{
    class YetAnotherService : IAnotherService
    {
        public AbstractService AbstractService { get; private set; }

        public YetAnotherService(AbstractService service)
        {
            AbstractService = service;
        }
    }
}

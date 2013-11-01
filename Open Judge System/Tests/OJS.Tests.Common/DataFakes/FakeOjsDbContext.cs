namespace OJS.Tests.Common.DataFakes
{
    using OJS.Data;

    public class FakeOjsDbContext : OjsDbContext
    {
        public FakeOjsDbContext()
            : base("FakeOjsDbContext")
        {
        }
    }
}
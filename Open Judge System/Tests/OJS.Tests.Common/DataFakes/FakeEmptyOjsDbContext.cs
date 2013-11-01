namespace OJS.Tests.Common.DataFakes
{
    using OJS.Data;

    public class FakeEmptyOjsDbContext : OjsDbContext
    {
        public FakeEmptyOjsDbContext()
            : base("FakeEmptyOjsDbContext")
        {
        }
    }
}

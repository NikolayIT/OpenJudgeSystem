namespace OJS.Tests.Common.DataFakes
{
    using OJS.Data;

    public class FakeOjsData : OjsData
    {
        public FakeOjsData(IOjsDbContext context)
            : base(context)
        {
        }
    }
}
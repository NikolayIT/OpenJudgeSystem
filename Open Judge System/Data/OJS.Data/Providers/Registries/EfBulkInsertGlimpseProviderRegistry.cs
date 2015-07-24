namespace OJS.Data.Providers.Registries
{
    using EntityFramework.BulkInsert;

    public static class EfBulkInsertGlimpseProviderRegistry
    {
        public static void Execute()
        {
            ProviderFactory.Register<GlimpseConnectionEfSqlBulkInsertProvider>("Glimpse.Ado.AlternateType.GlimpseDbConnection");
        }
    }
}

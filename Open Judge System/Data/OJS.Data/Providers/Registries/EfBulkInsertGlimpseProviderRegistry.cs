namespace OJS.Data.Providers.Registries
{
    using EntityFramework.BulkInsert;

    public static class EfBulkInsertGlimpseProviderRegistry
    {
        public static void Register()
        {
            ProviderFactory.Register<GlimpleConnectionEfSqlBulkInsertProvider>("Glimpse.Ado.AlternateType.GlimpseDbConnection");
        }
    }
}

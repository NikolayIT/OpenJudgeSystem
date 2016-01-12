namespace OJS.Data.Providers
{
    using System;
    using System.Reflection;

    using EntityFramework.BulkInsert.Providers;

    using Glimpse.Ado.AlternateType;

    internal class GlimpseConnectionEfSqlBulkInsertProvider : EfSqlBulkInsertProviderWithMappedDataReader
    {
        protected override string ConnectionString => this.GetConnectionStringWithPassword();

        private string GetConnectionStringWithPassword()
        {
            const string PropName = "_connectionString";

            var innerConnection = ((GlimpseDbConnection)this.DbConnection).InnerConnection;
            var type = innerConnection.GetType();

            FieldInfo fieldInfo = null;
            PropertyInfo propertyInfo = null;
            while (fieldInfo == null && propertyInfo == null && type != null)
            {
                fieldInfo = type.GetField(PropName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (fieldInfo == null)
                {
                    propertyInfo = type.GetProperty(PropName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                }

                type = type.BaseType;
            }

            if (fieldInfo == null && propertyInfo == null)
            {
                throw new Exception("Field _connectionString was not found");
            }

            if (fieldInfo != null)
            {
                return (string)fieldInfo.GetValue(innerConnection);
            }

            return (string)propertyInfo.GetValue(innerConnection, null);
        }
    }
}

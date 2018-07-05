namespace OJS.Data.Contracts.CodeFirstConventions
{
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.ModelConfiguration.Conventions;

    using OJS.Data.Contracts.DataAnnotations;

    public class IsUnicodeAttributeConvention : PrimitivePropertyAttributeConfigurationConvention<IsUnicodeAttribute>
    {
        public override void Apply(ConventionPrimitivePropertyConfiguration configuration, IsUnicodeAttribute attribute)
        {
            configuration.IsUnicode(attribute.IsUnicode);
        }
    }
}

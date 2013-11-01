namespace OJS.Data.Configurations
{
    using System.Data.Entity.ModelConfiguration;

    using OJS.Data.Models;

    public class UserProfileConfiguration : EntityTypeConfiguration<UserProfile>
    {
        public UserProfileConfiguration()
        {
            this.Property(x => x.UserName)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
        }
    }
}

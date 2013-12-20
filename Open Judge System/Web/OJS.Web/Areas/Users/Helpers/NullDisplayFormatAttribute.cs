namespace OJS.Web.Areas.Users.Helpers
{
    using System.ComponentModel.DataAnnotations;

    using Resource = Resources.Areas.Users.ViewModels.ProfileViewModels;

    public class NullDisplayFormatAttribute : DisplayFormatAttribute
    {
        public NullDisplayFormatAttribute()
            : base()
        {
            this.NullDisplayText = Resource.No_information;
        }
    }
}
namespace OJS.Common.Models
{
    using System.ComponentModel;

    public enum SystemRole
    {
        [Description("Administrator")]
        Administrator = 1,

        [Description("Lecturer")]
        Lecturer = 1 << 1
    }
}

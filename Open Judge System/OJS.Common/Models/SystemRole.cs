namespace OJS.Common.Models
{
    using System.ComponentModel;

    public enum SystemRole
    {
        [Description("Administrator")]
        Administrator = 1,

        [Description("Lecturer")]
        Lecturer = 1 << 1,

        [Description("Moderator")]
        Moderator = 1 << 2,

        [Description("Forum Admininistrator")]
        ForumAdmininistrator = 1 << 3,

        [Description("ForumBanned")]
        ForumBanned = 1 << 4,

        [Description("Sales Manager")]
        SalesManager = 1 << 5,

        [Description("Intern")]
        Intern = 1 << 6,

        [Description("Business Assistant")]
        BusinessAssistant = 1 << 7,

        [Description("Trainier")]
        Trainier = 1 << 8,

        [Description("Author")]
        Author = 1 << 9,
    }
}

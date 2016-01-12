namespace OJS.Common.Models
{
    using OJS.Common.Attributes;

    using Resource = Resources.Enums.EnumTranslations;

    public enum ContestQuestionType
    {
        [LocalizedDescription("Default", typeof(Resource))]
        Default = 0, // If possible answers available then DropDown, else text box

        [LocalizedDescription("DropDown", typeof(Resource))]
        DropDown = 1,

        [LocalizedDescription("TextBox", typeof(Resource))]
        TextBox = 2,

        [LocalizedDescription("MultiLineTextBox", typeof(Resource))]
        MultiLineTextBox = 3,
    }
}

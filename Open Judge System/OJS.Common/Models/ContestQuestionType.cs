namespace OJS.Common.Models
{
    using System.ComponentModel;

    using OJS.Common.Attributes;

    public enum ContestQuestionType
    {
        [LocalizedDescription("По подразбиране")]
        Default = 0, // If possible answers available then DropDown, else text box

        [LocalizedDescription("Dropdown лист")]
        DropDown = 1,

        [LocalizedDescription("Едноредов текст")]
        TextBox = 2,

        [LocalizedDescription("Многоредов текст")]
        MultiLineTextBox = 3,
    }
}

namespace OJS.Common.Models
{
    using System.ComponentModel;
    
    public enum ContestQuestionType
    {
        [Description("По подразбиране")]
        Default = 0, // If possible answers available then DropDown, else text box

        [Description("Dropdown лист")]
        DropDown = 1,

        [Description("Едноредов текст")]
        TextBox = 2,

        [Description("Многоредов текст")]
        MultiLineTextBox = 3,
    }
}

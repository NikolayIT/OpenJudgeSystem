namespace OJS.Data.Models
{
    public enum ContestQuestionType
    {
        Default = 0, // If possible answers available then DropDown, else text box
        DropDown = 1,
        TextBox = 2,
        MultiLineTextBox = 3,
    }
}

namespace OJS.Web.Areas.Administration.InputModels.Contests
{
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;

    public class LabStartInputModel
    {
        private const int DefaultLabDuration = 30;

        public LabStartInputModel()
        {
            this.Duration = DefaultLabDuration;
        }

        [HiddenInput(DisplayValue = false)]
        public int ContestCreateId { get; set; }

        [Display(Name = "Продължителност в минути")]
        [Required(ErrorMessage = "Моля задайте продължителност.")]
        [Range(1, int.MaxValue, ErrorMessage = "Продължителността трябва да е положително число.")]
        [UIHint("PositiveInteger")]
        public int Duration { get; set; }
    }
}
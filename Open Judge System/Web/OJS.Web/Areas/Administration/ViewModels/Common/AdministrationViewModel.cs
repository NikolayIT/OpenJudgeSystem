namespace OJS.Web.Areas.Administration.ViewModels.Common
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class AdministrationViewModel
    {
        [Display(Name = "Дата на създаване")]
        [DataType(DataType.DateTime)]
        [UIHint("_NonEditable")]
        public DateTime CreatedOn { get; set; }

        [Display(Name = "Дата на промяна")]
        [DataType(DataType.DateTime)]
        [UIHint("_NonEditable")]
        public DateTime? ModifiedOn { get; set; }
    }
}
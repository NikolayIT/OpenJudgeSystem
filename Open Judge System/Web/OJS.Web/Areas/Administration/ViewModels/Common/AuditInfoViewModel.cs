namespace OJS.Web.Areas.Administration.ViewModels.Common
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class AuditInfoViewModel
    {
        [Display(Name = "Дата на създаване")]
        [ScaffoldColumn(false)]
        [DataType(DataType.DateTime)]
        public DateTime CreatedOn { get; set; }

        [Display(Name = "Дата на промяна")]
        [ScaffoldColumn(false)]
        [DataType(DataType.DateTime)]
        public DateTime? ModifiedOn { get; set; }
    }
}
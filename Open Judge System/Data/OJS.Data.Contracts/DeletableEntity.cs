namespace OJS.Data.Contracts
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public abstract class DeletableEntity : AuditInfo, IDeletableEntity
    {
        [Display(Name = "Изтрит?")]
        [Editable(false)]
        [Index]
        public bool IsDeleted { get; set; }

        [Display(Name = "Дата на изтриване")]
        [Editable(false)]
        [DataType(DataType.DateTime)]
        public DateTime? DeletedOn { get; set; }
    }
}

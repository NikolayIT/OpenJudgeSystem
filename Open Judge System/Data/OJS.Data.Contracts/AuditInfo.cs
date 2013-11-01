namespace OJS.Data.Contracts
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public abstract class AuditInfo : IAuditInfo
    {
        [Display(Name = "Дата на създаване")]
        [Editable(false)]
        [DataType(DataType.DateTime)]
        public DateTime CreatedOn { get; set; }

        /// <summary>
        /// Specifies whether or not the CreatedOn property should be automatically set.
        /// </summary>
        [NotMapped]
        public bool PreserveCreatedOn { get; set; }

        [Display(Name = "Дата на промяна")]
        [Editable(false)]
        [DataType(DataType.DateTime)]
        public DateTime? ModifiedOn { get; set; }
    }
}

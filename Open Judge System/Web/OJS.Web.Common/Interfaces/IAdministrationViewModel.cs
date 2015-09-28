namespace OJS.Web.Common.Interfaces
{
    using System;

    public interface IAdministrationViewModel<T>
        where T : class
    {
        DateTime? CreatedOn { get; set; }

        DateTime? ModifiedOn { get; set; }

        T GetEntityModel(T model = null);
    }
}

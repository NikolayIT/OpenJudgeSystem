namespace OJS.Web.Areas.Administration.ViewModels.AccessLogs
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq.Expressions;
    using System.Web.Mvc;

    using OJS.Common.DataAnnotations;
    using OJS.Data.Models;
    using OJS.Web.Areas.Administration.ViewModels.Common;

    public class AccessLogGridViewModel : AdministrationViewModel<AccessLog>
    {
        [ExcludeFromExcel]
        public static Expression<Func<AccessLog, AccessLogGridViewModel>> ViewModel
        {
            get
            {
                return log => new AccessLogGridViewModel
                {
                    Id = log.Id,
                    UserName = log.User != null ? log.User.UserName : null,
                    IpAddress = log.IpAddress,
                    RequestType = log.RequestType,
                    Url = log.Url,
                    PostParams = log.PostParams,
                    CreatedOn = log.CreatedOn,
                    ModifiedOn = log.ModifiedOn
                };
            }
        }

        [Display(Name = "№")]
        public long Id { get; set; }

        [Display(Name = "Име")]
        public string UserName { get; set; }

        [Display(Name = "IP")]
        public string IpAddress { get; set; }

        [Display(Name = "Заявка")]
        public string RequestType { get; set; }

        [Display(Name = "URL")]
        public string Url { get; set; }

        [Display(Name = "POST параметри")]
        public string PostParams { get; set; }
    }
}
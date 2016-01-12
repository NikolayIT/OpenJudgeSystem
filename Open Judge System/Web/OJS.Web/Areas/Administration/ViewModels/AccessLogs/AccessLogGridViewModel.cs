namespace OJS.Web.Areas.Administration.ViewModels.AccessLogs
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq.Expressions;

    using OJS.Common.DataAnnotations;
    using OJS.Data.Models;
    using OJS.Web.Areas.Administration.ViewModels.Common;

    using Resource = Resources.Areas.Administration.AccessLogs.ViewModels.AccessLogGridViewModel;

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

        [Display(Name = "UserName", ResourceType = typeof(Resource))]
        public string UserName { get; set; }

        [Display(Name = "Ip", ResourceType = typeof(Resource))]
        public string IpAddress { get; set; }

        [Display(Name = "Request_type", ResourceType = typeof(Resource))]
        public string RequestType { get; set; }

        [Display(Name = "Url", ResourceType = typeof(Resource))]
        public string Url { get; set; }

        [Display(Name = "Post_params", ResourceType = typeof(Resource))]
        public string PostParams { get; set; }
    }
}
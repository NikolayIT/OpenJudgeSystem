namespace OJS.Web.Areas.Api.Models
{
    using System.Collections.Generic;

    public class UsersExamGroupModel
    {
        public IEnumerable<string> UserIds { get; set; }

        public string AppId { get; set; }

        public ExamGroupInfoModel ExamGroupInfoModel { get; set; }
    }
}
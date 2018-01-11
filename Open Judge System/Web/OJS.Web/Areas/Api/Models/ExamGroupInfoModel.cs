namespace OJS.Web.Areas.Api.Models
{
    using System;

    public class ExamGroupInfoModel
    {
        public int Id { get; set; }

        public string ExamGroupTrainingLabNameBg { get; set; }

        public string ExamGroupTrainingLabNameEn { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }
    }
}
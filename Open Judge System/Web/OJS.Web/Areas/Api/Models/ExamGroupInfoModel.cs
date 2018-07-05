namespace OJS.Web.Areas.Api.Models
{
    using System;

    public class ExamGroupInfoModel
    {
        public int Id { get; set; }

        public int? JudgeSystemContestId { get; set; }

        public string ExamName { get; set; }

        public string ExamGroupTrainingLabName { get; set; }

        public DateTime? StartTime { get; set; }
    }
}
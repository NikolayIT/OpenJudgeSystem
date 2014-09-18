namespace OJS.Web.Areas.Administration.ViewModels.AntiCheat
{
    using System.Collections.Generic;

    public class IpSubmissionsAdministrationViewModel
    {
        public string Ip { get; set; }

        public IEnumerable<int> SubmissionIds { get; set; }
    }
}
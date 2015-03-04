namespace OJS.Web.Areas.Administration.Models
{
    using System;
    using System.Linq.Expressions;

    using OJS.Data.Models;

    public class ParticipantModel
    {
        public static Expression<Func<Participant, ParticipantModel>> Model
        {
            get
            {
                return p => new ParticipantModel
                {
                    Id = p.Id,
                    UserName = p.User.UserName,
                    FirstName = p.User.UserSettings.FirstName,
                    LastName = p.User.UserSettings.LastName
                };
            }
        }

        public int Id { get; set; }

        public string UserName { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }
    }
}
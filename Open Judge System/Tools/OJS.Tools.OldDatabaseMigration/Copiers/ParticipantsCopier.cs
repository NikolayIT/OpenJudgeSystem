namespace OJS.Tools.OldDatabaseMigration.Copiers
{
    using System.Linq;

    using OJS.Data;
    using OJS.Data.Models;

    internal sealed class ParticipantsCopier : ICopier
    {
        public void Copy(OjsDbContext context, TelerikContestSystemEntities oldDb)
        {
            context.Configuration.AutoDetectChangesEnabled = false;
            var participants = oldDb.Participants.Select(x => 
                new
                    {
                        x.Id,
                        x.Contest,
                        OldUserId = x.User1.Id,
                        x.RegisteredOn,
                        x.IsOfficial,
                        x.Answer,
                    }).ToList();

            var contests = context.Contests.Select(x => new { x.OldId, x.Id, Question = x.Questions.FirstOrDefault() }).ToDictionary(x => x.OldId);
            var users = context.Users.Select(x => new { x.OldId, x.Id }).ToDictionary(x => x.OldId);
            
            foreach (var oldParticipant in participants)
            {
                if (!contests.ContainsKey(oldParticipant.Contest))
                {
                    continue;
                }

                var contest = contests[oldParticipant.Contest];

                if (!users.ContainsKey(oldParticipant.OldUserId))
                {
                    continue;
                }

                var participant = new Participant
                                      {
                                          OldId = oldParticipant.Id,
                                          PreserveCreatedOn = true,
                                          CreatedOn = oldParticipant.RegisteredOn,
                                          IsOfficial = oldParticipant.IsOfficial,
                                          ContestId = contest.Id,
                                          UserId = users[oldParticipant.OldUserId].Id,
                                      };

                if (contest.Question != null)
                {
                    var answer = new ParticipantAnswer
                            {
                                ContestQuestionId = contest.Question.Id,
                                Answer = oldParticipant.Answer,
                                Participant = participant,
                            };

                    context.ParticipantAnswers.Add(answer);
                }

                context.Participants.Add(participant);
            }

            context.SaveChanges();
            context.Configuration.AutoDetectChangesEnabled = true;
        }
    }
}

/*
 * Id --> [PK]
 * User --> [FK]
 * Contest --> [FK]
 * Answer --> ParticipantAnswers table
 * IsOfficial --> move
 * TotalPoints --> *remove*
 * RegisteredOn --> CreatedOn
*/
namespace OJS.Services.Business.Contests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Data.Repositories.Contracts;
    using OJS.Services.Common;
    using OJS.Services.Data.Contests;
    using OJS.Services.Data.ExamGroups;
    using OJS.Services.Data.Participants;
    using OJS.Services.Data.ParticipantScores;

    public class ContestsBusinessService : IContestsBusinessService
    {
        private readonly IEfDeletableEntityRepository<Contest> contests;
        private readonly IContestsDataService contestsData;
        private readonly IParticipantsDataService participantsData;
        private readonly IParticipantScoresDataService participantScoresData;
        private readonly IExamGroupsDataService examGroupsData;

        public ContestsBusinessService(
            IEfDeletableEntityRepository<Contest> contests,
            IContestsDataService contestsData,
            IParticipantsDataService participantsData,
            IParticipantScoresDataService participantScoresData,
            IExamGroupsDataService examGroupsData)
        {
            this.contests = contests;
            this.contestsData = contestsData;
            this.participantsData = participantsData;
            this.participantScoresData = participantScoresData;
            this.examGroupsData = examGroupsData;
        }

        public bool IsContestIpValidByContestAndIp(int contestId, string ip) =>
            this.contestsData
                .GetByIdQuery(contestId)
                .Any(c => !c.AllowedIps.Any() || c.AllowedIps.Any(ai => ai.Ip.Value == ip));

        public bool CanUserCompeteByContestByUserAndIsAdmin(
            int contestId,
            string userId,
            bool isAdmin,
            bool allowToAdminAlways = false)
        {
            var contest = this.contestsData.GetById(contestId);

            var isUserAdminOrLecturerInContest = isAdmin || this.contestsData
                .IsUserLecturerInByContestAndUser(contestId, userId);

            if (contest.IsOnline && !isUserAdminOrLecturerInContest)
            {
                var participant = contest.Participants.FirstOrDefault(p => p.UserId == userId && p.IsOfficial);

                if (participant == null)
                {
                    return contest.CanBeCompeted;
                }

                return participant.ParticipationEndTime >= DateTime.Now;
            }

            if (contest.CanBeCompeted || (isUserAdminOrLecturerInContest && allowToAdminAlways))
            {
                return true;
            }

            if (isUserAdminOrLecturerInContest && contest.IsActive)
            {
                return true;
            }

            return false;
        }

        // TODO: Extract different logic blocks in separate services
        public ServiceResult TransferParticipantsToPracticeById(int contestId)
        {
            var contest = this.contestsData.GetById(contestId);

            if (contest == null)
            {
                return new ServiceResult("Contest cannot be found");
            }

            if (contest.IsActive)
            {
                return new ServiceResult("The Contest is active and participants cannot be transferred");
            }

            var competeOnlyParticipants = contest.Participants
                .GroupBy(p => p.UserId)
                .Where(g => g.Count() == 1 && g.All(p => p.IsOfficial))
                .Select(gr => gr.FirstOrDefault());

            foreach (var participant in competeOnlyParticipants)
            {
                foreach (var participantScore in participant.Scores)
                {
                    participantScore.IsOfficial = false;
                }

                participant.IsOfficial = false;
            }

            var competeAndPracticeParticipants = contest.Participants
                .GroupBy(p => p.UserId)
                .Where(g => g.Count() == 2)
                .ToDictionary(grp => grp.Key, grp => grp.OrderBy(p => p.IsOfficial));

            var participantsForDeletion = new List<Participant>();

            foreach (var competeAndPracticeParticipant in competeAndPracticeParticipants)
            {
                var unofficialParticipant = competeAndPracticeParticipants[competeAndPracticeParticipant.Key].First();
                var officialParticipant = competeAndPracticeParticipants[competeAndPracticeParticipant.Key].Last();
                participantsForDeletion.Add(officialParticipant);

                foreach (var officialParticipantSubmission in officialParticipant.Submissions)
                {
                    officialParticipantSubmission.Participant = unofficialParticipant;
                }

                var scoresForDeletion = new List<ParticipantScore>();

                foreach (var officialParticipantScore in officialParticipant.Scores)
                {
                    var unofficialParticipantScore = unofficialParticipant
                        .Scores
                        .FirstOrDefault(s => s.ProblemId == officialParticipantScore.ProblemId);

                    if (unofficialParticipantScore != null)
                    {
                        if (unofficialParticipantScore.Points < officialParticipantScore.Points ||
                            (unofficialParticipantScore.Points == officialParticipantScore.Points &&
                             unofficialParticipantScore.Id < officialParticipantScore.Id))
                        {
                            unofficialParticipantScore = officialParticipantScore;
                            unofficialParticipantScore.IsOfficial = false;
                            unofficialParticipantScore.Participant = unofficialParticipant;
                        }

                        scoresForDeletion.Add(officialParticipantScore);
                    }
                    else
                    {
                        officialParticipantScore.IsOfficial = false;
                        officialParticipantScore.Participant = unofficialParticipant;
                    }
                }

                this.participantScoresData.Delete(scoresForDeletion);
            }

            this.participantsData.Delete(participantsForDeletion);

            return ServiceResult.Success;
        }

        public void DeleteById(int id)
        {
            this.examGroupsData.RemoveContestByContest(id);

            this.contests.Delete(id);
            this.contests.SaveChanges();
        }
    }
}
namespace OJS.Services.Business.Participants
{
    using System;
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface IParticipantsBusinessService : IService
    {
        Participant CreateNewByContestByUserByIsOfficialAndIsAdmin(
            Contest contest,
            string userId,
            bool isOfficial,
            bool isAdmin);

        /// <summary>
        /// Updates constest duration for participants in contest, 
        /// in time range with amount of minutes provided. If any participants' contest duration
        /// would be reduced below the base contest duration they are not updated.
        /// </summary>
        /// <param name="contestId">The id of the contest</param>
        /// <param name="minutes">Amount of minutes to be added to the participant's contest end time. 
        /// Amount can be negative</param>
        /// <param name="contestStartTimeRangeStart">The lower bound against which participants' contest start time would be checked</param>
        /// <param name="contestStartTimeRangeEnd">The upper bound against which participants' contest start time would be checked</param>
        /// <returns>Returns participants who were not updated
        /// because their contest duration would be reduced below
        /// the base contest duration</returns>
        IQueryable<Participant> UpdateContestEndTimeForAllParticipantsByContestByParticipantContestStartTimeRangeAndTimeIntervalInMinutes(
            int contestId,
            int minutes,
            DateTime contestStartTimeRangeStart,
            DateTime contestStartTimeRangeEnd);
    }
}
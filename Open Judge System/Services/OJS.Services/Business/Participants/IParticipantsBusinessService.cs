namespace OJS.Services.Business.Participants
{
    using System;
    using System.Collections.Generic;

    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface IParticipantsBusinessService : IService
    {
        Participant CreateNewByContestByUserByIsOfficialAndIsAdmin(
            Contest contest,
            string userId,
            bool isOfficial,
            bool isAdmin);

        ServiceResult<string> UpdateParticipationEndTimeByIdAndTimeInMinutes(int id, int minutes);

        /// <summary>
        /// Updates contest duration for participants in contest,
        /// in time range with amount of minutes provided. If any participants' contest duration
        /// would be reduced below the base contest duration they are not updated, but returned in the result data
        /// </summary>
        /// <param name="contestId">The id of the contest</param>
        /// <param name="minutes">Amount of minutes to be added to the participant's contest end time.
        /// Amount can be negative</param>
        /// <param name="participationStartTimeRangeStart">The lower bound against which participants' participation start time would be checked</param>
        /// <param name="participationStartTimeRangeEnd">The upper bound against which participants' participation start time would be checked</param>
        ServiceResult<ICollection<string>> UpdateParticipationsEndTimeByContestByParticipationStartTimeRangeAndTimeInMinutes(
            int contestId,
            int minutes,
            DateTime participationStartTimeRangeStart,
            DateTime participationStartTimeRangeEnd);
    }
}
namespace OJS.Web.ViewModels.Common
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;

    using OJS.Common.Extensions;
    using OJS.Data.Models;

    public class DropdownViewModel
    {
        private const int DefaultExcelColumnsCount = 26;
        private const char CapitalLetterA = 'A';

        public static Expression<Func<Contest, DropdownViewModel>> FromContest =>
            contest => new DropdownViewModel
            {
                Id = contest.Id,
                Name = contest.Name
            };

        public static Expression<Func<ContestCategory, DropdownViewModel>> FromContestCategory =>
            contestCategory => new DropdownViewModel
            {
                Id = contestCategory.Id,
                Name = contestCategory.Name
            };

        public static Expression<Func<ProblemGroup, DropdownViewModel>> FromProblemGroup =>
            problemGroup => new DropdownViewModel
            {
                Id = problemGroup.Id,
                Name = problemGroup.OrderBy.ToString()
            };

        public int Id { get; set; }

        public string Name { get; set; }

        public static IEnumerable<DropdownViewModel> GetExcelColumnsDropDownList(
            int columnsCount = DefaultExcelColumnsCount)
        {
            var excelColumnsDropDownList = new List<DropdownViewModel>();

            for (var i = 0; i < columnsCount; i++)
            {
                excelColumnsDropDownList.Add(new DropdownViewModel
                {
                    Id = i + 1,
                    Name = ((char)(CapitalLetterA + i)).ToString()
                });
            }

            return excelColumnsDropDownList;
        }

        public static IEnumerable<DropdownViewModel> GetYearsDropDownList(
            int fromYear,
            int toYear,
            bool ascendingOrder = true)
        {
            var yearsDropDownList = new List<DropdownViewModel>();

            for (var i = fromYear; i <= toYear; i++)
            {
                yearsDropDownList.Add(new DropdownViewModel
                {
                    Id = i,
                    Name = i.ToString(CultureInfo.InvariantCulture)
                });
            }

            if (ascendingOrder)
            {
                return yearsDropDownList;
            }

            return yearsDropDownList.OrderByDescending(x => x.Id);
        }

        public static IEnumerable<DropdownViewModel> GetFromRange(int start, int end) =>
            Enumerable
                .Range(start, end - start + 1)
                .Select(x => new DropdownViewModel
                {
                    Id = x,
                    Name = x.ToString()
                });

        public static IEnumerable<DropdownViewModel> GetEnumValues<T>()
            where T : struct, IConvertible =>
                Enum
                    .GetValues(typeof(T))
                    .Cast<T>()
                    .Select(x => new DropdownViewModel
                    {
                        Id = Convert.ToInt32(x),
                        Name = x.GetDescription()
                    });

        public override bool Equals(object obj) =>
            obj is DropdownViewModel other && this.Id == other.Id;

        public override int GetHashCode() => this.Id.GetHashCode();
    }
}
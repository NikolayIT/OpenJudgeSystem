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

        public static Expression<Func<Contest, DropdownViewModel>> FromContest
        {
            get
            {
                return x => new DropdownViewModel { Id = x.Id, Name = x.Name };
            }
        }

        public static Expression<Func<ContestCategory, DropdownViewModel>> FromContestCategory
        {
            get
            {
                return x => new DropdownViewModel { Id = x.Id, Name = x.Name };
            }
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public static IEnumerable<DropdownViewModel> GetExcelColumnsDropDownList(int columnsCount = DefaultExcelColumnsCount)
        {
            var excelColumnsDropDownList = new List<DropdownViewModel>();
            for (var i = 0; i < columnsCount; i++)
            {
                excelColumnsDropDownList.Add(new DropdownViewModel { Id = i + 1, Name = ((char)(CapitalLetterA + i)).ToString() });
            }

            return excelColumnsDropDownList;
        }

        public static IEnumerable<DropdownViewModel> GetYearsDropDownList(int fromYear, int toYear, bool ascendingOrder = true)
        {
            var yearsDropDownList = new List<DropdownViewModel>();
            for (var i = fromYear; i <= toYear; i++)
            {
                yearsDropDownList.Add(new DropdownViewModel { Id = i, Name = i.ToString(CultureInfo.InvariantCulture) });
            }

            if (ascendingOrder)
            {
                return yearsDropDownList;
            }

            return yearsDropDownList.OrderByDescending(x => x.Id);
        }

        public static IEnumerable<DropdownViewModel> GetFromRange(int start, int end)
        {
            return Enumerable
                .Range(start, end - start + 1)
                .Select(x => new DropdownViewModel() { Id = x, Name = x.ToString() });
        }

        public static IEnumerable<DropdownViewModel> GetEnumValues<T>() where T : struct, IConvertible
        {
            return Enum
                    .GetValues(typeof(T))
                    .Cast<T>()
                    .Select(x => new DropdownViewModel
                    {
                        Id = Convert.ToInt32(x),
                        Name = x.GetDescription()
                    });
        }

        public override bool Equals(object obj)
        {
            var other = obj as DropdownViewModel;
            return other != null && this.Id == other.Id;
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
    }
}
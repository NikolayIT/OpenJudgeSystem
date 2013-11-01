namespace OJS.Common.Extensions
{
    using System;
    using System.Linq.Expressions;

    public static class FuncExtensions
    {
        public static Expression<Func<T1, T2>> ToExpression<T1, T2>(this Func<T1, T2> func)
        {
            return x => func(x);
        }
    }
}

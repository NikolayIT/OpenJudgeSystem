namespace OJS.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    public static class ExpressionBuilder
    {
        public static Expression<Func<TElement, bool>> BuildOrExpression<TElement, TValue>(
            IEnumerable<TValue> values,
            Expression<Func<TElement, TValue>> valueSelector)
        {
            if (valueSelector == null)
            {
                throw new ArgumentNullException(nameof(valueSelector));
            }

            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            var parameterExpression = valueSelector.Parameters.Single();

            if (!values.Any())
            {
                return e => false;
            }

            var equals =
                values.Select(
                    value =>
                    (Expression)Expression.Equal(valueSelector.Body, Expression.Constant(value, typeof(TValue))));

            var body = equals.Aggregate(Expression.Or);

            return Expression.Lambda<Func<TElement, bool>>(body, parameterExpression);
        }
    }
}

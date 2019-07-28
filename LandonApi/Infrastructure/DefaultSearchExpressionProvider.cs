using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace LandonApi.Infrastructure
{
    public class DefaultSearchExpressionProvider: ISearchExpressionProvider
    {
        public virtual ConstantExpression GetValue(string input)
            => Expression.Constant(input);

        public virtual Expression GetComparison(MemberExpression left, string termOperator, ConstantExpression right)
        {
            if(!termOperator.Equals("eq", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"Invalid operator '{termOperator}'.");

            return Expression.Equal(left, right);
        }
    }
}

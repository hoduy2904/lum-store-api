using System.Linq.Expressions;

namespace LumStoreAPI.Core.Models.Riches
{
    public class SetterBuilder<T>
    {
        private readonly List<(LambdaExpression property, LambdaExpression value)> _setters = new();
        public SetterBuilder<T> Set<TProperty>(Expression<Func<T, TProperty>> property, Expression<Func<T, TProperty>> value)
        {
            _setters.Add((property, value));
            return this;
        }
        public SetterBuilder<T> Set<TProperty>(Expression<Func<T, TProperty>> property, TProperty value)
        {
            var parameter = Expression.Parameter(typeof(T), "x");

            var constant = Expression.Constant(value, typeof(TProperty));

            var valueLambda =
                Expression.Lambda<Func<T, TProperty>>(constant, parameter);
            _setters.Add((property, valueLambda));

            return this;
        }

        public List<(LambdaExpression property, LambdaExpression value)> GetValues()
        {
            return _setters;
        }
    }
}

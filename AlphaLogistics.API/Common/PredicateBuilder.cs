using System.Linq.Expressions;

namespace WALMS.API.Common
{
    public static class PredicateBuilder
    {
        /// <summary>
        /// Returns a predicate that always evaluates to true.
        /// </summary>
        /// <typeparam name="T">The type of the object that the predicate will evaluate.</typeparam>
        /// <returns>An expression that always returns true.</returns>
        public static Expression<Func<T, bool>> True<T>() { return f => true; }

        /// <summary>
        /// Returns a predicate that always evaluates to false.
        /// </summary>
        /// <typeparam name="T">The type of the object that the predicate will evaluate.</typeparam>
        /// <returns>An expression that always returns false.</returns>
        public static Expression<Func<T, bool>> False<T>() { return f => false; }

        /// <summary>
        /// Combines two predicates using a logical OR operation.
        /// </summary>
        /// <typeparam name="T">The type of the object that the predicates will evaluate.</typeparam>
        /// <param name="expr1">The first predicate.</param>
        /// <param name="expr2">The second predicate.</param>
        /// <returns>A new predicate that represents the logical OR of the two input predicates.</returns>
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>(Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
        }

        /// <summary>
        /// Combines two predicates using a logical AND operation.
        /// </summary>
        /// <typeparam name="T">The type of the object that the predicates will evaluate.</typeparam>
        /// <param name="expr1">The first predicate.</param>
        /// <param name="expr2">The second predicate.</param>
        /// <returns>A new predicate that represents the logical AND of the two input predicates.</returns>
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
        }
    }
}

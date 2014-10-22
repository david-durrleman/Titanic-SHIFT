using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Titanic
{
    public static class PropertyUtil<TClass>
    {
        // Specifying a PropertyInfo is quite verbose and uncomfortable. To get it for Property P of type B on an
        // object of type A, one would write typeof(A).GetProperty("P"). Not only is this longer to write than the
        // chosen solution, it is also not type-safe. If the property is renamed to Q, the code will still compile, 
        // but fail at run time.

        // Instead we provide a way to get a PropertyInfo from a LambdaExpression, which is a special type of
        // object in C# akin to a syntactic tree for a function, which can then be explored to get information
        // about its code. It is much cleaner to specify than a PropertyInfo directly, is type safe, and can be
        // used to retrieve the PropertyInfo for the property referred-to, which we do below.
        // In our case, one would then just write new PropertyUtil<A>().GetPropInfo<B>(x => x.P, ...);

        // See http://stackoverflow.com/questions/491429/how-to-get-the-propertyinfo-of-a-specific-property
        // for more details
        public static PropertyInfo GetPropInfo<TProperty>(Expression<Func<TClass, TProperty>> propExpr)
        {
            Expression expr = propExpr;
            while (expr is LambdaExpression)
            {
                expr = ((LambdaExpression)expr).Body;
            }
            switch (expr.NodeType)
            {
                case ExpressionType.MemberAccess:
                    return (PropertyInfo)((MemberExpression)expr).Member;
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}

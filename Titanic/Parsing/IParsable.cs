using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

// Note that this file uses advanced C# constructs. I expect it to be a bit hard to understand
// without some background.

namespace Titanic.Parsing
{
    // A ParsableProperty represents a property of a given class that can be parsed
    // and then set on any object of that class.

    // It is defined by a Parser which will read strings and return values for the property,
    // and a PropertyInfo, which is a special type of C# object which holds information about
    // a given property in a given class.
    public class ParsableProperty
    {
        public IParser Parser { get; private set; }
        private PropertyInfo PropInfo { get; set; }

        // The name of the property can be read on the PropInfo object
        public string Name
        {
            get
            {
                return PropInfo.Name;
            }
        }

        // Given an object, we can get its value for the property we're interested in
        // using the PropertyInfo object
        public object GetValue(object obj)
        {
            return PropInfo.GetValue(obj);
        }

        // And we can also set that same value
        public void SetValue(object obj, object value)
        {
            PropInfo.SetValue(obj, value);
        }

        // This is a simplistic way of constructing a ParsableProperty,
        // but we actually only expose the Create<> function below to build them
        private ParsableProperty(PropertyInfo propInfo, IParser parser)
        {
            PropInfo = propInfo;
            Parser = parser;
        }

        // Specifying a PropertyInfo is quite verbose and uncomfortable. To get it for Property P of type B on an
        // object of type A, one would write typeof(A).GetProperty("P"). Not only is this longer to write than the
        // chosen solution, it is also not type-safe. If the property is renamed to Q, the code will still compile, 
        // but fail at run time.

        // Instead we provide a way to get a PropertyInfo from a LambdaExpression, which is a special type of
        // object in C# akin to a syntactic tree for a function, which can then be explored to get information
        // about its code. It is much cleaner to specify than a PropertyInfo directly, is type safe, and can be
        // used to retrieve the PropertyInfo for the property referred-to, which we do below.
        // In our case, one would then just write ParsableProperty.Create<A, B>(x => x.P, ...);

        // See http://stackoverflow.com/questions/491429/how-to-get-the-propertyinfo-of-a-specific-property
        // for more details
        public static ParsableProperty Create<TClass, TProperty>(Expression<Func<TClass, TProperty>> propExpr, IParser parser)
        {
            Expression expr = propExpr;
            while (expr is LambdaExpression)
            {
                expr = ((LambdaExpression)expr).Body;
            }
            switch (expr.NodeType)
            {
                case ExpressionType.MemberAccess:
                    return new ParsableProperty((PropertyInfo)((MemberExpression)expr).Member, parser);
                default:
                    throw new InvalidOperationException();
            }
        }
    }

    // The IParsable interface requires implementing classes to specify a list of ParsableProperty objects
    // These refer to the configurable parameters of the classes, which are thus defined as parsable and can
    // be set from user input or from a file.
    public interface IParsable
    {
        ParsableProperty[] Props { get; }
    }

    // Here we define an extension method for IParsable that allow to configure any class implementing
    // IParsable with an array containing the right number of strings. An extension method is a way to
    // extend any class satisfying a given interface with helper methods which use the functionality
    // defined in an interface.

    // As discussed in Commands.cs, there are other ways to do this such as using an abstract base class.
    // It wouldn't work in this case, because we have very different classes implementing IParsable
    // (models and passengers), and making them inherit from the same base class would be cumbersome
    // and a bad idea - note that C# forbids multiple inheritance.

    // We use the "fluent interface" pattern for naming the extension method:
    // http://en.wikipedia.org/wiki/Fluent_interface
    // They have pros and cons, but I reckoned in this case, why not?
    public static class IParsableExtensions
    {
        private static void CheckParamLength(this IParsable @this, int length)
        {
            if (length != @this.Props.Length)
                throw new TitanicException(String.Format("{0} expects {1} arguments, got {2}", @this, @this.Props.Length, length));
        }

        public static T WithParsableProps<T>(this T @this, string[] values) where T : IParsable
        {
            @this.CheckParamLength(values.Length);

            for (int i = 0; i < values.Length; i++)
                @this.Props[i].SetValue(@this, @this.Props[i].Parser.Parse(values[i]));

            return @this;
        }

        public static T WithConsoleInput<T>(this T @this) where T : IParsable
        {
            UI.PrintMessage(String.Format("Manual definition of object {0}:", @this));
            foreach (ParsableProperty p in @this.Props)
            {
                UI.PrintMessage(String.Format("Enter a value for {0}. Expected format : {1}", p.Name, (p.Parser.OutType).Name));
                while (true)
                {
                    try
                    {
                        p.SetValue(@this, p.Parser.Parse(UI.GetCommandLine()));
                        break;
                    }
                    catch (TitanicException e)
                    {
                        UI.PrintError(e.Message);
                    }
                }
            }
            return @this;
        }
    }
}

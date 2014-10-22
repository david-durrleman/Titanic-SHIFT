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

        public static ParsableProperty Create<TClass, TProperty>(Expression<Func<TClass, TProperty>> propExpr, IParser parser)
        {
            return new ParsableProperty(PropertyUtil<TClass>.GetPropInfo(propExpr), parser);
        }
    }

    // The IParsable interface requires implementing classes to specify a list of ParsableProperty objects
    // These refer to the configurable parameters of the classes, which are thus defined as parsable and can
    // be set from user input or from a file.
    public interface IParsable
    {
        ParsableProperty[] Props { get; }
    }

    // Here we define extension methods for IParsable that allow to configure any class implementing
    // IParsable with the right number of strings. An extension method is a way to extend any class 
    // satisfying a given interface with helper methods which use the functionality defined in an 
    // interface.

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
            {
                var prop = @this.Props[i];
                prop.SetValue(@this, prop.Parser.Parse(values[i]));
            }

            return @this;
        }

        private static Type GetBaseType(Type topType)
        {
            var type = topType;
            while (Nullable.GetUnderlyingType(type) != null)
                type = Nullable.GetUnderlyingType(type);

            return type;
        }

        public static T WithUserProps<T>(this T @this) where T : IParsable
        {
            for (int i = 0; i < @this.Props.Length; i++)
            {
                var prop = @this.Props[i];
                UI.PrintMessage(String.Format("Please provide a {0} ({1})", prop.Name, GetBaseType(prop.Parser.OutType).Name));
                prop.SetValue(@this, prop.Parser.Parse(UI.GetLine()));
            }

            return @this;
        }
    }
}

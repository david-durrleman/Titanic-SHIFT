using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Titanic.Parsing
{
    // Parsers read a string and return an object (int, bool, enum, etc...) corresponding
    // to what they've read. IParser defines an interface for this behaviour.
    public interface IParser
    {
        // The type of the parsed objects
        Type OutType { get; }
        // The parsing function
        object Parse(string input);
    }

    // We name TryParseFunc a specific type of function which tries to
    // read and parse an input string, and modifies its second argument
    // setting it to the parse result. It returns a boolean recording its
    // success in parsing. It is polymorphic in the type of the type of the
    // output value.
    public delegate bool TryParseFunc<TOutput>(string input, out TOutput value);

    // A generic parser. It is polymorphic so the OutType is neatly defined
    // by its type argument. It uses a TryParseFunc (delegate defined above)
    // as its main way of parsing things. This is because TryParse is a common
    // idiom in C# that we want to reuse for brevity (e.g. Int32.TryParse as can
    // be seen below).

    // Every actual parser we'll define inherits from this one.
    public class Parser<TOutput> : IParser
    {
        public Type OutType { get { return typeof(TOutput); } }
        public TryParseFunc<TOutput> TryParseFunc { get; private set; }

        public Parser(TryParseFunc<TOutput> tryParseFunc)
        {
            TryParseFunc = tryParseFunc;
        }

        // Implementing Parse given a TryParseFunc is really easy. It suffices to
        // throw if the try wasn't successful.
        public object Parse(string input)
        {
            TOutput value;

            if (!TryParseFunc(input, out value))
                throw new TitanicException(String.Format("Couldn't parse {0}: {1}", typeof(TOutput).Name, input));

            return value;
        }
    }

    // Parsing a string is trivial. The corresponding TryParseFunc always returns true and sets its output to its input
    public class StringParser : Parser<string>
    {
        public StringParser()
            : base((string input, out string value) => { value = input; return true; })
        {
        }
    }

    // The following classes parse basic C# types using the already defined TryParse functions in the C# library
    public class IntParser : Parser<int>
    {
        public IntParser()
            : base(Int32.TryParse)
        {
        }
    }

    public class UIntParser : Parser<uint>
    {
        public UIntParser()
            : base(UInt32.TryParse)
        {
        }
    }

    public class DoubleParser : Parser<double>
    {
        public DoubleParser()
            : base(Double.TryParse)
        {
        }
    }
    
    // Here we explicitly are not using bool.TryParse. Why?
    // Because in our specific case, the CSV files we are reading use 1 for True and 
    // 0 for False (in the Survival column). bool.TryParse is unable to parse 0 and 1
    // so instead we parse ints which we then convert to booleans.

    // This is a bad idea in general because this parser is now unable to parse e.g. "True"
    // as a literal. But it works for our usecase.
    public class BoolParserFromInt : Parser<bool>
    {
        public BoolParserFromInt()
            : base((string input, out bool value) => { int intValue; var result = Int32.TryParse(input, out intValue); value = Convert.ToBoolean(intValue); return result; })
        {
        }
    }

    public class BoolParser :  Parser<bool>
    {
        public BoolParser()
            :base (Boolean.TryParse)
        {
        }
    }

    // This parser can parse any enumerable type. We don't just use Enum.TryParse because this is too liberal (it allows values
    // which are out of range). So we also check Enum.IsDefined afterwards.
    public class EnumParser<TEnum> : Parser<TEnum> where TEnum : struct
    {
        public EnumParser()
            : base((string input, out TEnum value) => Enum.TryParse<TEnum>(input, out value) && Enum.IsDefined(typeof(TEnum), value))
        {
        }
    }

    // Finally this one is the most complicated one. It is a parser that, given
    // a Parser<T>, can parse value of type T?. The easy way to do so is to just
    // return null if a value of type T could not be parsed by the base parser.
    public class NullableParser<TValue> : Parser<Nullable<TValue>> where TValue : struct
    {
        public NullableParser(Parser<TValue> baseParser)
            : base((string input, out Nullable<TValue> value) => { TValue baseValue; value = baseParser.TryParseFunc(input, out baseValue) ? (TValue?)baseValue : null; return true; })
        {
        }
    }

    // Parsing a property for a class given its name is easy using Reflection.
    // An exception is raised if the property doesn't exist, in which case we
    // fail gracefully
    public class PropertyInfoParser<TClass> : Parser<PropertyInfo>
    {
        public PropertyInfoParser()
            : base((string input, out PropertyInfo value) => { try { value = typeof(TClass).GetProperty(input); return true; } catch { value = null; return false; } })
        {
        }
    }

    // This parses an array of values of the same types given a parser for the
    // underlying values, and a separator char.
    // For example "1,2,3" could be parsed as new int[] { 1, 2, 3 }
    public class ArrayParser<TElement> : Parser<TElement[]>
    {
        public ArrayParser(Parser<TElement> baseParser, char separator)
            : base((string input, out TElement[] value) =>
        {
            var splits = input.Split(separator);
            value = new TElement[splits.Length];

            int i = 0;
            foreach (var split in splits)
            {
                TElement element;
                if (!baseParser.TryParseFunc(split, out element))
                    return false;

                value[i++] = element;
            }

            return true;
        })
        {
        }

        public ArrayParser(Parser<TElement> baseParser)
            : this(baseParser, ',')
        {
        }
    }
}

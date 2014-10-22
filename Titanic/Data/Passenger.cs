using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Titanic.Parsing;

// In this file we define our data model, i.e. the specifications of the data we will be working with.

namespace Titanic.Data
{
    // The first three enums represent scalar types which can only take a specific set of values.
    // Enums are useful because they give better names to purely numerical data (as in the case of
    // PassengerClass), and also allow for type checking the data (if I were to write Sex.Alien,
    // the code wouldn't compile).
    public enum PassengerClass { FirstClass = 1, SecondClass = 2, ThirdClass = 3 };
    public enum Sex { male, female };
    public enum Port { C, S, Q };

    // The main data class represents a Titanic passenger. It mostly contains scalar data in the
    // form of properties.
    // It implements IParsable meaning that it can be parsed from a file or from user input
    // (and we make use of this functionality in the code).

    // Note that some properties are defined as Nullable ("Nullable<T>" == "T?"). Nullable is a 
    // useful type in C# that allows for a given variable to contain either a value, or null, so 
    // we can represent the fact that this variable is not always known, but that when it is, it 
    // is e.g. an int. It's better than reserving an extra value in the type for this purpose 
    // because null is special so it forces the developer to handle it specially rather than 
    // possibly ignore it. It even allows the compiler to check it (I can not add an int? with 
    // another int?, I first have to check that they are not null and retrieve their values or the 
    // code won't  compile)
    // One could then ask why Cabin is not defined as nullable as it can also be unknown. The
    // reason is that TicketId is a string, which is a reference type and not a value type (i.e.
    // a class and not a struct or a scalar). Reference types can always be equal to null so
    // it makes no sense to have a nullable reference type (and C# doesn't allow it).
    // See also: http://msdn.microsoft.com/en-us/library/vstudio/1t3y8s4s(v=vs.110).aspx

    // Note also that I have encoded the required positivity of some variables (Age for example)
    // in the type system by making them be uint (unsigned int), so it's just impossible to have
    // them be negative. The more the compiler can check for us, the better!

    // Passenger does *not* contain information about whether the passenger survived or not.
    // This will be included only in a child class below. Indeed, it is useful to encode
    // into the type system whether or not we have information about a passenger's survival.
    // This way, if we were to try to feed a basic Passenger to a Model for training, the
    // compiler would alert us that this is an error (instead of problems showing up at runtime)
    public class Passenger : IParsable
    {
        // The Props array is required to implement IParsable.
        public ParsableProperty[] Props { get; protected set; }

        public Passenger()
        {
            Props = new ParsableProperty[]
            {
                // Here we specify all the properties that we will set after reading them, in the order they are
                // written in the CSV file, along with how they should be parsed.
                ParsableProperty.Create<Passenger, int>(x => x.Id, new IntParser()),
                ParsableProperty.Create<Passenger, PassengerClass>(x => x.Class, new EnumParser<PassengerClass>()),
                ParsableProperty.Create<Passenger, string>(x => x.Name, new StringParser()),
                ParsableProperty.Create<Passenger, Sex>(x => x.Sex, new NullableParser<Sex>(new EnumParser<Sex>())),
                ParsableProperty.Create<Passenger, uint?>(x => x.Age, new NullableParser<uint>(new UIntParser())),
                ParsableProperty.Create<Passenger, uint>(x => x.NumSiblingsOrSpouses, new UIntParser()),
                ParsableProperty.Create<Passenger, uint>(x => x.NumParentsOrChildren, new UIntParser()),
                ParsableProperty.Create<Passenger, string>(x => x.TicketId, new StringParser()),
                ParsableProperty.Create<Passenger, double?>(x => x.Fare, new NullableParser<double>(new DoubleParser())),
                ParsableProperty.Create<Passenger, string>(x => x.Cabin, new StringParser()),
                ParsableProperty.Create<Passenger, Port?>(x => x.DeparturePort, new NullableParser<Port>(new EnumParser<Port>())),
            };
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public PassengerClass Class { get; set; }
        public Sex Sex { get; set; }
        public uint NumSiblingsOrSpouses { get; set; }
        public uint NumParentsOrChildren { get; set; }
        public string TicketId { get; set; }
        public uint? Age { get; set; }
        public double? Fare { get; set; }
        public string Cabin { get; set; }
        public Port? DeparturePort { get; set; }

        public string FamilyName { get { return Name.Split(',')[0]; } }
        public char? Deck { get { return String.IsNullOrEmpty(Cabin) ? null : (char?)Cabin[0]; } }
    }

    // As described above, this child class represents a passenger with the extra information of whether
    // they survived the disaster or not. They are used in training (whereas only regular passengers would
    // be used in testing).
    public class TrainingPassenger : Passenger
    {
        public bool Survives { get; set; }

        public TrainingPassenger()
            : base()
        {
            // We have to do some array gymnastics to put the Survival property in the right place in the parsable
            // properties list
            var propsList = Props.ToList();
            propsList.Insert(1, ParsableProperty.Create<TrainingPassenger, bool>(x => x.Survives, new BoolParserFromInt()));
            Props = propsList.ToArray();
        }
    }
}

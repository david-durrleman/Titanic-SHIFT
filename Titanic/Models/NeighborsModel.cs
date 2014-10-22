using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Titanic.Data;
using Titanic.Parsing;

namespace Titanic.Models
{
    class NeighborsModel : IModel
    {
        private bool m_isTrained = false;

        public ParsableProperty[] Props { get; private set; }
        public PropertyInfo[] PassengerProps { get; private set; }

        /* This model consider that a passenger is likely to share the fate of other passengers related to him in some way 
         * (same deck, same age, etc.). 
         * This models defines uses a dictionnary to store congruent neighbor classes dictionnaries using...XXX
         * */

        /* This is the table used to store neighbor information. Its structure is the following:
         * m_neighbors[Property Name][Property value].Real = nb of neighbors of that kind met during training
         * m_neighbors[Property Name][Property value].Imaginary = nb of neighbors of that kind met during training that survived
         * */

        // Why complex numbers? 
        // Well, we want to record exactly two numbers. Arrays and List are not great for this because there's no way to enforce their size
        // to be fixed.
        // So we should use a type that can only contain two values. A good datatype for storing and retrieving N values, where N is known,
        // is a tuple. Tuple<string, string, int> for example, always contains two strings and an int.
        // In the case of just two numeric values however, complex numbers can sometimes be useful. Not only do they allow for changing and
        // retrieving each value independently, they also provide basic arithmetic functions (addition, subtraction ...), which we make
        // use of here for achieving slightly less verbose code. One drawback of using Complex numbers as record classes is that the intent
        // is not obvious at first sight, so the real intended meaning should be well explained in comments like above.

        // Of course, in more complicated cases, rolling our own custom class with the functionality we require would be better than either
        // tuples or complex numbers.
        private Dictionary<string, Dictionary<object, Complex>> m_neighbors = new Dictionary<string, Dictionary<object, Complex>>();

        public NeighborsModel()
        {
            Props = new ParsableProperty[] { ParsableProperty.Create<NeighborsModel, PropertyInfo[]>(m => m.PassengerProps, new ArrayParser<PropertyInfo>(new PropertyInfoParser<Passenger>())) };
        }

        public IEnumerable<String> ModelInfo
        {
            get
            {
                return new String[]{
                    "This model consider that a passenger is likely to share the fate of other passengers related to him in some way (same deck, same age, etc.)",
                    "The model consider then that the survival chance of a passenger is equal to the average survival rate amongst his neighbors.",
                    "Construction call must be create Neighbors Prop1,Prop2,Prop3,...,PropN where PropI is the name of a passenger property"
                };
            }
        }

        public IEnumerable<KeyValuePair<String, object>> TrainingInfo
        {
            get
            {
                return m_neighbors.Select(d => new KeyValuePair<String, object>(String.Format("Number of neighbors class for {0}", d.Key), d.Value.Count));
            }
        }

        // Indexers are properties that can take arguments. They are accessed as this[arg1, ..., argN]
        // Here we use an indexer because it gives us a nice clean syntax for doing operations such as this[x, y] += z
        // which gets automatically converted as setIndexerValue(x, y, getIndexerValue(x, y) + z) and we can customize the
        // set and get functions. This way we put our sanity checking code (initializing dictionaries, checking for null, 
        // etc...) in the getter/setter and can have very clean training and simulation functions

        // It's private because it's an implementation detail. External code is not supposed to directly access passenger
        // statistics
        private Complex this[string propName, object propValue]
        {
            get
            {
                if (!m_neighbors.ContainsKey(propName))
                    m_neighbors[propName] = new Dictionary<object, Complex>();

                // null can't be used as a key in a dictionary, so we force null values
                // to return zero statistics. This makes sense anyway because a null value
                // represents an unknown in our data model, so we shouldn't consider other
                // passengers with null values for the same property as "neighbors"
                if (propValue == null || !m_neighbors[propName].ContainsKey(propValue))
                    return Complex.Zero;
                else
                    return m_neighbors[propName][propValue];
            }
            set
            {
                // As explained above, null values are special. Any passenger statistics for
                // them are silently ignored
                if (propValue == null)
                    return;

                m_neighbors[propName][propValue] = value;
            }
        }

        public int Train(IEnumerable<TrainingPassenger> passengers)
        {
            foreach (var passenger in passengers)
                foreach (var prop in PassengerProps)
                    this[prop.Name, prop.GetValue(passenger)] += new Complex(1, passenger.Survives ? 1 : 0);

            m_isTrained = true;

            return passengers.Count();
        }

        public double Calculate(Data.Passenger passenger)
        {
            if (!m_isTrained)
                throw new TitanicException("Model is not yet trained");

            // IEnumerable<Complex>.Sum() is not defined (probably an oversight from Microsoft) so we write it explicitly
            // This is where the use of complex numbers shines, as we can sum the whole bunch indiscriminately and still
            // get the values we want at the end.
            var passengerNeighbors = PassengerProps.Select(pp => this[pp.Name, pp.GetValue(passenger)]).Aggregate(Complex.Zero, (c1, c2) => c1 + c2);
            return passengerNeighbors.Imaginary / passengerNeighbors.Real; // number of neighboring survivors / number of neighboring passengers
        }
    }
}

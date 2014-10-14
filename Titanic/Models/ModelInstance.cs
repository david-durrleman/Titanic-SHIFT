using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Titanic.Data;
using Titanic.Parsing;

namespace Titanic.Models
{
    // This class is a container for models. It is created with a model type and some
    // parameters, builds and configures the corresponding model, and stores it.
    // It is intended to be the only way to create a model, and specifically to enforce the
    // fact that any model both can and must be created using only a model type and an array
    // of parameters (parsable strings or parsed values).
    
    // The outside world can not see the underlying IModel, as it is private, so it also defines
    // the *real* public interface for models.
    
    // It provides helper functions for interacting with the raw model interface in an easier way,
    // such as calculating survival for a set of passengers, or even simulating that survival
    // using a simple RNG.

    // It also extends the training information to provide additional useful bits such as
    // the type of the model and the size of the training set.

    // Finally it can duplicate a model into another with the same parameters (but untrained).

    // This is yet another example of how to extend a minimal interface with common functions
    // which only depend on the base interface functions. For other possible patterns, such
    // as abstract base classes or extension methods, see the comments in Command.cs
    public class ModelInstance
    {
        private Random m_random = new Random(1234567);

        // The model type specified upon creation
        public ModelType Type { get; private set; }
        // The model which was initialized from model type and parameters
        private IModel Model { get; set; }
        // The following two properties record information on if and how the model was trained
        public bool IsTrained { get; private set; }
        public int TrainingSetSize { get; private set; }

        // Information about this specific instance of the model, generated at the user's request.
        // This is defined as a list of key and values (not a dictionary because we want them ordered)
        public IEnumerable<KeyValuePair<string, object>> InstanceInfo
        {
            get
            {
                var result = new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("Model Type", Type)
                };

                foreach (var prop in Model.Props)
                    result.Add(new KeyValuePair<string, object>(prop.Name, prop.GetValue(Model)));

                result.Add(new KeyValuePair<string, object>("Is Trained", IsTrained));

                if (IsTrained)
                {
                    result.Add(new KeyValuePair<string, object>("Training Set Size", TrainingSetSize));
                    result.AddRange(Model.TrainingInfo);
                }

                return result;
            }
        }

        // The following two constructors are the only way the outside world should ever
        // be building models. So to be given a model is the same as being given a model
        // type and some parameters.

        // The constructor works by asking the ModelManager to construct an unconfigured
        // model of the given type, and then configures it using the fact that it implements
        // IParsable
        public ModelInstance(ModelType type, string[] paramValues)
        {
            Model = ModelManager.CreateModel(type).WithParsableProps(paramValues);
        }

        // This trains the underlying model and record information about the training set
        public int Train(IEnumerable<TrainingPassenger> passengers)
        {
            var result = Model.Train(passengers);
            IsTrained = true;
            TrainingSetSize = result;
            return result;
        }

        // The following 4 functions allow for the underlying model, once trained, to do some
        // computations on one or more Passengers. Either to calculate their survival probability,
        // or to simulate their survival directly
        public double Calculate(Passenger passenger)
        {
            if (!IsTrained)
                throw new Exception("Cannot simulate as the model hasn't been trained yet");
            else
                return Model.Calculate(passenger);
        }

        public IEnumerable<double> Calculate(IEnumerable<Passenger> passengers)
        {
            // Look ma, no loops!
            return passengers.Select(Calculate);
        }

        public bool Simulate(Passenger passenger)
        {
            return m_random.NextDouble() < Calculate(passenger);
        }

        public IEnumerable<bool> Simulate(IEnumerable<Passenger> passengers)
        {
            // Look ma, no loops!
            return passengers.Select(Simulate);
        }

        // Finally, this allows for obtaining an untrained copy of a given model, with the same parameters
        public ModelInstance Duplicate()
        {
            return new ModelInstance(Type, Model.Props.Select(p => p.GetValue(Model).ToString()).ToArray());
        }
    }
}

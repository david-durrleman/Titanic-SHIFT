using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Titanic.Data;
using Titanic.Parsing;

namespace Titanic.Models
{
    // The minimal set of functionality a given model has to satisfy.

    // We require to be able to report some info about the model (in the form of strings),
    // as well as to train the model on a list of TrainingPassengers, and to estimate the
    // probability of survival of a Passenger once that's done. (Note the difference in
    // types - it's not possible to train a model if the survival isn't known, but on the
    // other hand calculating the probability of survival of a passenger whose survival is
    // known would be cheating.)

    // IModel inherits from IParsable as we want users to be able to specify the model
    // parameters by entering strings into the UI (so they need to be parsed)
    public interface IModel : IParsable
    {
        // Information about the model. This is only intended to be called on uninitialized
        // models and as such shouldn't depend on the model's parameters.
        // It is specified as a list of lines to be displayed to the user
        IEnumerable<string> ModelInfo { get; }
        // Information about a trained model. This is called on initialized and trained
        // instances, so it can depend on the model's parameters.
        // It is specified as a list of key-value pairs to be displayed to the user.
        IEnumerable<KeyValuePair<string, object>> TrainingInfo { get; }

        // Train the model on a list of TrainingPassenger (these could come from the user or a file)
        int Train(IEnumerable<TrainingPassenger> passengers);
        // Calculate the probability of survival of a Passenger (can throw if the model hasn't
        // been trained yet)
        double Calculate(Passenger passenger);
    }
}

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Titanic.Data;
using Titanic.Parsing;

namespace Titanic.Models
{
    public class MeanProbabilityModel : IModel
    {
        private bool m_isTrained = false;
        private int m_numSurvivors = 0;

        // Records the average survival probability after training
        public double MeanProbability { get; private set; }
        public ParsableProperty[] Props { get; private set; }

        public MeanProbabilityModel()
        {
            // This model has no parameters so there are no properties to parse.
            Props = new ParsableProperty[0];
        }

        public IEnumerable<string> ModelInfo
        {
            get
            {
                return new string[]{
                    "This model is a very simple one, designed for testing purposes mainly.",
                    "We consider that every passenger has the same probability to survive.",
                    "This probability is set to the rate of survival observed in the training data set."
                };
            }
        }

        public IEnumerable<KeyValuePair<string, object>> TrainingInfo
        {
            get
            {
                return new KeyValuePair<string, object>[]
                {
                    new KeyValuePair<string, object>("Number of Survivors", m_numSurvivors),
                    new KeyValuePair<string, object>("Probability of Survival", MeanProbability)
                };
            }
        }

        public int Train(IEnumerable<TrainingPassenger> passengers)
        {
            MeanProbability = passengers.Select(p => Convert.ToDouble(p.Survives)).Average(); // Convert.ToDouble(boolean b) returns 1.0 for true and 0.0 for false, averaging gives the mean probability
            m_numSurvivors = passengers.Where(p => p.Survives).Count();                       // Counting the survivors here
            m_isTrained = true;

            return passengers.Count();
        }

        public double Calculate(Passenger passenger)
        {
            if (!m_isTrained)
                throw new TitanicException("Model is not yet trained");

            return MeanProbability;
        }
    }
}

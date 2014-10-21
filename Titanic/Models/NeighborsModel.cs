using System;
using System.Collections.Generic;
using System.Linq;
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

        /* This model consider that a passenger is likely to share the fate of other passengers related to him in some way 
         * (same deck, same age, etc.). 
         * This models defines uses a dictionnary to store congruent neighbor classes dictionnaries using...XXX
         * */

        /* This is the table used to store neighbor information. Its structure is the following:
         * neighbors[Passenger Property Name][Neighbor value][0] = nb of neighbors of that kind met during training
         * neighbors[Passenger Property Name][Neighbor value][1] = nb of neighbors of that kind met during training that survived
         * */
        private Dictionary<String, Dictionary<String, int[]>> neighbors { get; set; }

        // AJA: Exceptionally, constructor will use direct input from console, as I prefer to link the model properties
        // to those of a passenger (i.e. don't use a "bool usethatparameter"), and don't see a way to do it except from the inside
        // Another way to do it would have been to use dedicated parsable props for each Passenger property
        // We use preDefined to avoid the model definition process when not necessary (e.g. for Model Info)
        private bool _predefined;
        public bool preDefined
        {
            get { return _predefined; }
            set
            {
                _predefined = value;
                if (! value)
                {
                    new Passenger().Props.ToList().ForEach(p =>
                   {
                       if (UI.GetYesOrNo(String.Format("Do you want to consider Passenger's {0} ? [Y/N]", p.Name)))
                           neighbors.Add(p.Name, new Dictionary<String, int[]>());
                   });
                }//eventually specify here a default model
            }
        } // condition whether we use default passenger data as neighbor classes

        public NeighborsModel()
        {
            neighbors = new Dictionary<String, Dictionary<String, int[]>>();
            Props = new ParsableProperty[1] { ParsableProperty.Create<NeighborsModel, bool>(x => x.preDefined, new BoolParser()) };
        }

        public IEnumerable<String> ModelInfo
        {
            get
            {
                return new String[]{
                    "This model consider that a passenger is likely to share the fate of other passengers related to him in some way (same deck, same age, etc.)",
                    "The model consider then that the survival chance of a passenger is equal to the average survival rate amongst his neighbors.",
                    "Construction call must be create Neighbors <Predefined>, this last argument being: True = use default configuration, False = run configuration process."
                };
            }
        }

        public IEnumerable<KeyValuePair<String, object>> TrainingInfo
        {
            get
            {
                return neighbors.Select(d => new KeyValuePair<String, object>(String.Format("Number of neighbors class for {0}", d.Key), d.Value.Count));
            }
        }

        public int Train(IEnumerable<TrainingPassenger> passengers)
        {
            foreach (var p in passengers)
            {
                p.Props.Where(pp => (pp.GetValue(p) != null && neighbors.ContainsKey(pp.Name))).ToList().ForEach(pp =>
                {
                    String neighborClass = getNeighborClass(pp, p);
                    if (!neighbors[pp.Name].ContainsKey(neighborClass))
                    {// initiate the value
                        neighbors[pp.Name].Add(neighborClass, new int[] { 0, 0 });
                    }
                    neighbors[pp.Name][neighborClass][0]++;
                    neighbors[pp.Name][neighborClass][1] += Convert.ToInt32(p.Survives);
                });
            }

            m_isTrained = true;
            return passengers.Count();
        }

        public double Calculate(Data.Passenger passenger)
        {
            if (!m_isTrained)
                throw new TitanicException("Model is not yet trained");

            int nbNeighbors = 0;
            int nbNeighborsSurvivors = 0;

            passenger.Props.Where(pp => (pp.GetValue(passenger) != null && neighbors.ContainsKey(pp.Name))).ToList().ForEach(pp =>
                {
                    String neighborClass = getNeighborClass(pp, passenger);
                    if (neighbors[pp.Name].ContainsKey(neighborClass))
                    {
                        nbNeighbors += neighbors[pp.Name][neighborClass][0];
                        nbNeighborsSurvivors += neighbors[pp.Name][neighborClass][1];
                    }
                });
            return (double) nbNeighborsSurvivors / nbNeighbors;
        }


        //Methods returning neighbor class from passenger data (comparison between passengers in that model)
        //Allows specific rules for certain neighbor class
        private String getNeighborClass(ParsableProperty pp, Passenger p)
        {
            switch (pp.Name.ToUpper())
            {
                case "NAME": return getFamilyName(pp.GetValue(p).ToString());
                case "CABIN": return getDeck(pp.GetValue(p).ToString());
                default: return pp.GetValue(p).ToString();
            }

        }

        // Comparison method to get Family name from passenger name
        private String getFamilyName(String s)
        {
            return s.Split(',')[0];
        }


        // Comparison method to get Deck from Passenger Cabin
        private String getDeck(String s)
        {
            return "" + s[0];
        }

    }
}

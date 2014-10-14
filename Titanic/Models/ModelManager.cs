using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Titanic.Models
{
    // Available model types should be named here, and added to the mapping in the ModelManager() constructor below
    public enum ModelType { MeanProbability };

    // This class keeps track of all the available models in the program. It is static because it is intended to be
    // global at the program level.
    // It has a dictionary that maps model types to a builder (a function which creates an unconfigured model of the
    // given type, to be fed parameters later). We use builders because we can't require a specific constructor
    // in an interface, so there is no guarantee that an IModel can be constructed! Hence we ask for an explicit
    // way to do it :-)

    // The class also maintains a list to keep track of the currently living model instances and their ids.
    public static class ModelManager
    {
        private static IDictionary<ModelType, Func<IModel>> ModelBuilders = new Dictionary<ModelType, Func<IModel>>();
        public static int NumModelTypes { get { return ModelBuilders.Count(); } }
        
        private static List<ModelInstance> ModelInstances = new List<ModelInstance>();
        public static IEnumerable<int> ModelIds { get { return ModelInstances.Select((m, i) => new { Model = m, Id = i }).Where(x => x.Model != null).Select(x => x.Id).ToList(); } }

        // The static constructor (which gets executed only once, when the class is first accessed by the program)
        // is the right place to actually instantiate the list of available model types.
        static ModelManager()
        {
            AddModelType(ModelType.MeanProbability, () => new MeanProbabilityModel());
        }

        // This public method could be used (although it isn't at the moment) to dynamically add new model types
        // say if we wanted to give the user the possiblity to define new model types at run-time)
        public static void AddModelType(ModelType type, Func<IModel> builder)
        {
            ModelBuilders.Add(type, builder);
        }

        // This is used to create an unconfigured model of the given type, using the builder dictionary
        public static IModel CreateModel(ModelType type)
        {
            if (!ModelBuilders.ContainsKey(type))
                throw new TitanicException(String.Format("Don't know how to build a model of type {0}", type));

            return ModelBuilders[type]();
        }

        // This returns information about a given model type. For this it has to build an unconfigured model
        // and then ask for it's ModelInfo property, which is defined in the IModel interface.

        // It would be cleaner if interfaces would allow to define static methods (so we wouldn't have to
        // construct an object just to get this property which is actually independent of the underlying
        // object, and also we could enforce that it really is independent), but unfortunately C# doesn't
        // allow it.
        public static IEnumerable<string> ModelInfo(ModelType type)
        {
            return new string[] { String.Format("Model Type: {0}", type) }.Concat(CreateModel(type).ModelInfo);
        }

        // This records a ModelInstance in the global ModelInstances variable and returns its id.
        public static int AddModel(ModelInstance instance)
        {
            ModelInstances.Add(instance);
            return ModelInstances.Count() - 1;
        }

        // This creates a ModelInstance container, which will contain a model of the given type and configure it
        // with the given parameters. It then records it in the global ModelInstances variable and returns its id.
        public static int AddModel(ModelType type, string[] paramValues)
        {
            return AddModel(new ModelInstance(type, paramValues));
        }

        // This helper checks whether a given modelId has been assigned and still exists, and throws otherwise.
        private static void CheckHasModelId(int modelId)
        {
            if (modelId < 0 || modelId >= ModelInstances.Count || ModelInstances[modelId] == null)
                throw new TitanicException(String.Format("Model {0} doesn't exist", modelId));
        }

        // And this returns the model associated with the given modelId
        public static ModelInstance GetModel(int modelId)
        {
            CheckHasModelId(modelId);
            return ModelInstances[modelId];
        }

        // Finally this deletes the model with the given modelId
        public static void DeleteModel(int modelId)
        {
            CheckHasModelId(modelId);
            ModelInstances[modelId] = null;
        }
    }
}

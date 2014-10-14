using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Titanic.Data;
using Titanic.Models;
using Titanic.Parsing;

namespace Titanic.Commands
{
    public class TrainCommand : Command
    {
        public override string Description
        {
            get { return "Trains a model on the given dataset"; }
        }

        public override string ArgSyntax
        {
            get { return " <ModelId> <InputPath>"; }
        }

        public override CmdResult ExecuteUnsafe(string cmdName, string[] cmdArgs)
        {
            if (cmdArgs.Length != 2)
                return UsageFailure(cmdName);

            var modelId = (int)(new IntParser().Parse(cmdArgs[0])); // As usual, the parser will throw a TitanicException if a non-int was supplied.
            var path = cmdArgs[1];

            var model = ModelManager.GetModel(modelId); // Here again we'll get a TitanicException if the model doesn't exist
            var allFields = new CsvReader().ReadFile(path); // And here as well if we can't access or parse the file

            // The below line may be a bit complex. Here is what it does.
            // For each set of fields read in the file, we create a new passenger for training from these fields.
            // We then feed the resulting list of training passengers to the model for training. The model
            // helpfully takes care of the rest of the work.
            model.Train(allFields.Select(fields => new TrainingPassenger().WithParsableProps(fields)));

            return CmdResult.Success(String.Format("Model successfully trained with {0}/{1} passengers", allFields.Count(), model.TrainingSetSize));
        }
    }
}

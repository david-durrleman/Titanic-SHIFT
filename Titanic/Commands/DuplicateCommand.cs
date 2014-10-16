using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Titanic.Models;
using Titanic.Parsing;

namespace Titanic.Commands
{
    public class DuplicateCommand : Command
    {

        public override string Description
        {
            get { return "Creates an untrained model from given model with same parameters"; }
        }

        public override string ArgSyntax
        {
            get { return " <modelId>"; }
        }


        public override CmdResult ExecuteUnsafe(string cmdName, string[] cmdArgs)
        {
            if (cmdArgs.Length != 1)
                return UsageFailure(cmdName);

            var modelId = (int)(new IntParser().Parse(cmdArgs[0])); // The parser will throw a TitanicException if an int wasn't supplied. This will be caught in Command.Execute() and result in a failure
            var model = ModelManager.GetModel(modelId).Duplicate();

            var newModelId = ModelManager.AddModel(model); // Most of the work is dispatched to the ModelManager so the model-specific code is centralized in one place.
            return CmdResult.Success(String.Format("Duplicated model {0} from model {1} of type {2}", newModelId, modelId,model.Type));
                    }
    }
}

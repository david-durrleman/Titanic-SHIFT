using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Titanic.Models;
using Titanic.Parsing;

namespace Titanic.Commands
{
    public class CreateCommand : Command
    {
        public override string Description
        {
            get { return "Creates a model with given type and arguments"; }
        }

        public override string ArgSyntax
        {
            get { return " <type> <args>*"; }
        }

        public override CmdResult ExecuteUnsafe(string cmdName, string[] cmdArgs)
        {
            if (cmdArgs.Length < 1)
                return UsageFailure(cmdName);

            var modelType = (ModelType)(new EnumParser<ModelType>().Parse(cmdArgs[0])); // As usual, the parser will throw a TitanicException if a wrong model type was supplied.
            var paramValues = cmdArgs.Skip(1).ToArray();

            var modelId = ModelManager.AddModel(modelType, paramValues); // Most of the work is dispatched to the ModelManager so the model-specific code is centralized in one place.
            return CmdResult.Success(String.Format("Created model {0} with type {1} and arguments '{2}'", modelId, modelType, String.Join(", ", paramValues)));
        }
    }
}

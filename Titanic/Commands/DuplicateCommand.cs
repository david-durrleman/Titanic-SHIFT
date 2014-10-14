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
            get { return "Duplicates a model into another"; }
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
            var model = ModelManager.GetModel(modelId);             // Same here, the ModelManager will throw a TitanicException if the model doesn't exist, which will be caught properly in the same place.
            ModelManager.AddModel(model.Duplicate());

            return CmdResult.Success(String.Format("Created model of type {0} with id {1}", model.Type, modelId));
        }
    }
}
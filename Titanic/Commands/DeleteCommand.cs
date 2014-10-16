using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Titanic.Models;
using Titanic.Parsing;

namespace Titanic.Commands
{
    public class DeleteCommand : Command
    {
        public override string Description
        {
            get { return "Deletes given model"; }
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
            if (UI.GetYesOrNo(String.Format("Are you sure you want to delete model {0} ?", modelId)))
            {
                ModelManager.DeleteModel(modelId);

                return CmdResult.Success(String.Format("Deleted model {0}", modelId));
            }
            else
                return CmdResult.None();
        }
    }
}

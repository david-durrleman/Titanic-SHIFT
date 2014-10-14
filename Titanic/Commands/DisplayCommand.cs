using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Titanic.Models;
using Titanic.Parsing;

namespace Titanic.Commands
{
    public class DisplayCommand : Command
    {
        public override string Description
        {
            get { return "Displays key information on given model"; }
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

            // The way ModelInstance.InstanceInfo is defined, while awkward, allows
            // us to build the message to display really easily here as each line
            // is just equal to "<key>: <value>", where both key and value are
            // defined by the model (so the command doesn't have to know about them
            // and the abstraction doesn't leak).
            var builder = new StringBuilder("+++ MODEL INFORMATION +++\n");
            foreach (var kv in model.InstanceInfo)
                builder.AppendLine(String.Format("{0}: {1}", kv.Key, kv.Value));
            builder.Append("+++");

            return CmdResult.Success(builder.ToString());
        }
    }
}

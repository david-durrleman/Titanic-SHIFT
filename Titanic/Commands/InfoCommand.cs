using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Titanic.Models;
using Titanic.Parsing;

namespace Titanic.Commands
{
    public class InfoCommand : Command
    {
        public override string Description
        {
            get { return "Displays information on given model type"; }
        }

        public override string ArgSyntax
        {
            get { return " <modelType>"; }
        }

        public override CmdResult ExecuteUnsafe(string cmdName, string[] cmdArgs)
        {
            if (cmdArgs.Length != 1)
                return UsageFailure(cmdName);

            var modelType = (ModelType)(new EnumParser<ModelType>().Parse(cmdArgs[0])); // As usual, the parser will throw a TitanicException if a wrong model type was supplied.
            var infos = ModelManager.ModelInfo(modelType);                              // Same here, the ModelManager will throw a TitanicException if the model type isn't supported

            // The way ModelInstance.ModelInfo is defined, while awkward, allows
            // us to build the message to display really easily here line by line
            // while adding some decoration for the user
            var builder = new StringBuilder("++++++++++++++++++++++++ ABOUT THE MODEL  ++++++++++++++++++++++++\n");
            foreach (var line in infos)
                builder.AppendLine(line);
            builder.Append("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");

            return CmdResult.Success(builder.ToString());
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Titanic.Commands
{
    public class HelpCommand : Command
    {
        public override string Description
        {
            get { return "Displays this message"; }
        }

        public override string ArgSyntax
        {
            get { return ""; }
        }

        public override CmdResult ExecuteUnsafe(string cmdName, string[] cmdArgs)
        {
            // Note that the help is defined in the CommandManager class. Indeed it makes more sense
            // as that is the class that manages the list of available commands.
            return CmdResult.Success(CommandManager.HelpMessage);
        }
    }
}

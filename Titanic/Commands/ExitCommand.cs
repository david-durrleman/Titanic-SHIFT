using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Titanic.Commands
{
    public class ExitCommand : Command
    {
        public override string Description
        {
            get { return "Exits the program"; }
        }

        public override string ArgSyntax
        {
            get { return ""; }
        }

        public override CmdResult ExecuteUnsafe(string cmdName, string[] cmdArgs)
        {
            if (UI.GetYesOrNo("Do you want to exit?"))
                return CmdResult.Exit("See you later!");
            else
                return CmdResult.None();
        }
    }
}

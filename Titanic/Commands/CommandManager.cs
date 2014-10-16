using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Titanic.Commands
{
    // This class keeps track of all the available commands in the program. It is static because it is intended to be
    // global at the program level.

    // It has a dictionary that maps command names to their implementations, plus a list of the names to keep track of
    // their order.
    public static class CommandManager
    {
        private static IDictionary<string, ICommand> Commands = new Dictionary<string, ICommand>(StringComparer.OrdinalIgnoreCase);
        private static IList<string> CommandNames = new List<string>();

        // This class is the right place to define the program's help message, because it is the one that knows
        // everything about the available commands. The help message can thus be computed directly from what's
        // available (instead of being hardcoded and potentially out of sync).

        // It is returned as a string (rather than being printed directly to the screen) so it can be used in multiple
        // places or even modified if needs be.
        public static string HelpMessage
        {
            get
            {
                var builder = new StringBuilder("Possible commands:");

                foreach (var name in CommandNames)
                {
                    var command = Commands[name];
                    builder.Append(String.Format("\n\t{0}{1}: {2}", name, command.ArgSyntax, command.Description));
                }

                return builder.ToString();
            }
        }

        // The static constructor (which gets executed only once, when the class is first accessed by the program)
        // is the right place to actually instantiate the list of available commands.
        static CommandManager()
        {
            AddCommand("create", new CreateCommand());
            AddCommand("train", new TrainCommand());
            AddCommand("display", new DisplayCommand());
            AddCommand("info", new InfoCommand());
            AddCommand("exit", new ExitCommand());
            AddCommand("help", new HelpCommand());
            AddCommand("delete", new DeleteCommand());
            AddCommand("duplicate", new DuplicateCommand());

        }


        // This public method could be used (although it isn't at the moment) to dynamically add new commands,
        // say if we wanted to give the user the possiblity to define new commands at run-time)
        public static void AddCommand(string cmdName, ICommand command)
        {
            Commands.Add(cmdName, command);
            CommandNames.Add(cmdName);
        }

        // This is the entry point to all the commands, through which they get executed by the program.
        // The main loop passes a command line to the command manager, which parses it, finds the
        // command implementation corresponding to it, runs it with the provided arguments, and returns
        // the results. It also handles failure cases such as an invalid command.
        public static CmdResult Execute(string cmdLine)
        {
            var parts = cmdLine.Split(' ');
            var cmdName = parts[0];
            var cmdArgs = parts.Skip(1).ToArray();

            if (cmdName == "")
                return CmdResult.None();

            if (!Commands.ContainsKey(cmdName))
                return CmdResult.Failure("Invalid command");
            else
                return Commands[cmdName].Execute(cmdName, cmdArgs);
        }
    }
}

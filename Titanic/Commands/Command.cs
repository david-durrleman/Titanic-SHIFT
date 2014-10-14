using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Titanic.Commands
{
    // This abstract class is inherited by all user commands. While it leaves the gist of the
    // implementation to its children, it allows to define a minimal set of functionality which
    // we intend to be shared among all commands and that we don't want to have to rewrite every
    // time.

    // Another way to do this would be to define extension methods for the ICommand interface,
    // which is what we do in IParsable.

    // The choice of using abstract classes vs extension methods is not an exact science, and I 
    // generally base my choice upon intuition. Some discussion on the topic can be found at the 
    // links below. As you can see there are widely differing opinions on the topic.
    // http://programmers.stackexchange.com/questions/41740/when-to-use-abstract-classes-instead-of-interfaces-with-extension-methods-in-c
    // http://stackoverflow.com/questions/651884/with-the-advent-of-extension-methods-are-abstract-classes-less-attractive
    // http://stackoverflow.com/questions/783312/interface-extension-mixin-vs-base-class
    public abstract class Command : ICommand
    {
        public abstract string Description { get; }
        public abstract string ArgSyntax { get; }

        // Helper method to use when a command fails because of invalid arguments
        public CmdResult UsageFailure(string cmdName)
        {
            return CmdResult.Failure(String.Format("Invalid arguments. Usage: {0}{1}", cmdName, ArgSyntax));
        }

        // Main execution method so that individual commands don't have to worry about catching
        // exceptions. The main classes catches them all (well, only the TitanicException which
        // we know we threw), so individual commands can be written as if exceptions can't
        // happen.
        public CmdResult Execute(string cmdName, string[] cmdArgs)
        {
            try
            {
                return ExecuteUnsafe(cmdName, cmdArgs);
            }
            catch (TitanicException exception)
            {
                return CmdResult.Failure(exception.Message);
            }
        }

        // The actual implementation of the execution is now dispatched to ExecuteUnsafe
        // which has the same signature as Execute but isn't required to catch exceptions.
        public abstract CmdResult ExecuteUnsafe(string cmdName, string[] cmdArgs);
    }
}

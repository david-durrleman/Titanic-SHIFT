using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Titanic.Commands
{
    // This class defines the result of a command. It contains a return code to signify
    // what happened (and especially what to do next), as well as a message to be displayed
    // to the user
    public class CmdResult
    {
        public enum RetCode { None, Success, Failure, Exit };

        // This return code is used by the main loop to decide what to do after the command
        // has run
        public RetCode Code { get; private set; }
        // This string is intended to be displayed to the user
        public string Message { get; private set; }

        // The below pattern (private constructor + 4 helper methods) is to ensure
        // the class is always well constructed (and easily too).
        // Indeed the protection level ensures we can only go through the helper
        // methods to build the class which for example forces a message in case
        // of Success, but not in case of None. Additionally, the syntax of
        // CmdResult.Success("yay!") is shorter than new CmdResult(RetCode.Success, "yay!")
        private CmdResult(RetCode code, string message)
        {
            Code = code;
            Message = message;
        }

        public static CmdResult None()
        {
            return new CmdResult(RetCode.None, "");
        }

        public static CmdResult Success(string message)
        {
            return new CmdResult(RetCode.Success, message);
        }

        public static CmdResult Failure(string message)
        {
            return new CmdResult(RetCode.Failure, message);
        }

        public static CmdResult Exit(string message)
        {
            return new CmdResult(RetCode.Exit, message);
        }
    }

    // This interface defines the minimal set of functionality all commands have to specify.
    // Two strings are used to display help about the command upon request, and the
    // Execute() method takes arguments specified by the user, does its own thing, and returns
    // a CmdResult upon completion.

    // As long as a class implements this, it can be defined as a user command in the program.
    // The goal is to make it easy to add/remove such commands.
    public interface ICommand
    {
        // This is used to describe the command in the program's help
        string Description { get; }
        // And this describes the arguments syntax (e.g. "" if the command takes no argument, and 
        // " <path>" if it takes a file - note the space in front of the latter)
        string ArgSyntax { get; }
        
        // Execution method, which individual commands are supposed to implement
        // to define their behaviour. Its arguments are the name of the command 
        // (e.g. "create") and its arguments (e.g. [ "mymodel" "myfile" ])
        CmdResult Execute(string cmdName, string[] cmdArgs);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.VisualBasic.FileIO;
using Microsoft.VisualBasic;
using Titanic.Commands;
using Titanic.Models;

// Note that all of the code is implemented in a single project. Code separation and modularity are good,
// but there is generally no need in C# to separate your code into multiple projects (i.e. multiple dlls
// and exe files), unless you intend to distribute them separately.

namespace Titanic
{
    // The main class. It's static, because I don't need to have two instances of it at the same time (there's only one program running at all times)
    // It handles the high-level program logic, i.e. the read-eval-print loop, but doesn't concern itself much with lower level stuff like
    // how to interact with the user, as well as which commands and models are available
    public static class Titanic
    {
        // The welcome message is data and is separated from the place where it is actually displayed.
        // This allows (although not used here) to reuse it in multiple places or even to have it be transformed later
        // (like a translation into the user's language) if needs be. This is generally good practice.
        private static string Intro 
        {
            get
            {
                var builder = new StringBuilder();

                builder.Append("############################\n");
                builder.Append("## TITANIC SIMULATOR 2014 ##\n");
                builder.Append("############################\n\n");
                builder.Append("Will you survive?\n");

                return builder.ToString();
            }
        }


        // Same here, the home screen is implemented as data which gets printed elsewhere.
        // This is actually an example of data that ends up depending on some parameters (the available models).
        private static string HomeScreen
        {
            get
            {

                var builder = new StringBuilder();

                builder.Append(String.Format("-----------------  {0} Active model(s)  ----------------\n", ModelManager.ModelIds.Count()));
                builder.Append("\tID\tType\t\tTrained\t\tTraining base size\n");

                foreach (var modelId in ModelManager.ModelIds)
                {
                    var Model = ModelManager.GetModel(modelId);

                    if (Model.IsTrained)
                        builder.Append(String.Format("\t{0}\t{1}\tyes\t\t{2}\n", modelId, Model.Type, Model.TrainingSetSize));
                    else
                        builder.Append(String.Format("\t{0}\t{1}\tno\n", modelId, Model.Type));
                }

                
                builder.Append(String.Format("---------------- {0} model type(s) implemented --------------------", ModelManager.NumModelTypes));

                return builder.ToString();
            }
        }

        // The main function of the program. It's short, simple and readable.
        // After the user is welcomed, the program keeps asking for input
        // which it dispatches to the CommandManager to be run.
        // Depending on the result it can show a success message, an error,
        // exit entirely...

        // All display functions are dispatched to a special static UI class.
        // This modularity is useful as the UI implementation can be changed
        // without affecting the rest of the program, and makes other parts
        // of the code easier to read and maintain. Same for the commands.
        public static void Main(string[] args)
        {
            UI.WaitMessage(Intro);
            UI.WaitMessage(CommandManager.HelpMessage);
            UI.PrintMessage(HomeScreen);

            while (true)
            {
                var result = CommandManager.Execute(UI.GetLine());

                switch (result.Code)
                {
                    case CmdResult.RetCode.None:
                        break;
                    case CmdResult.RetCode.Success:
                        UI.Clear();
                        UI.PrintMessage(HomeScreen);
                        UI.PrintSuccess(result.Message);
                        break;
                    case CmdResult.RetCode.Failure:
                        UI.PrintError(result.Message);
                        break;
                    case CmdResult.RetCode.Exit:
                        UI.PrintMessage(result.Message);
                        return;
                    default:
                        throw new Exception(String.Format("Unsupported return code: {0}", result.Code));
                }
            }
        }
    }
}

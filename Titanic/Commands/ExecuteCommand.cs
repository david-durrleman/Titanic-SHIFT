using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Titanic.Data;
using Titanic.Models;
using Titanic.Parsing;

namespace Titanic.Commands
{
    public class ExecuteCommand : Command
    {
        public bool Simulate { get; set; }

        public ExecuteCommand(bool simulate = false)
        {
            Simulate = simulate;
        }

        public override string Description
        {
            get { return (Simulate ? "Simulates" : "Calculates") + " survival"; }
        }

        public override string ArgSyntax
        {
            get { return " modelId [<InputPath> [<OutputPath>]]"; }
        }

        public override CmdResult ExecuteUnsafe(string cmdName, string[] cmdArgs)
        {
            if (cmdArgs.Length < 1 || cmdArgs.Length > 3)
                return UsageFailure(cmdName);

            var modelId = (int)(new IntParser().Parse(cmdArgs[0])); // As usual, the parser will throw a TitanicException if a non-int was supplied.
            var model = ModelManager.GetModel(modelId); // Here again we'll get a TitanicException if the model doesn't exist
            
            IEnumerable<Passenger> passengers;
            if (cmdArgs.Length >= 2)
            {
                var inPath = cmdArgs[1];
                var allFields = new CsvUtil().ReadFile(inPath); // And here as well if we can't access or parse the file

                passengers = allFields.Select(fields => new Passenger().WithParsableProps(fields)); // See TrainCommand.cs for help with this line
            }
            else
            {
                passengers = new Passenger[] { new Passenger().WithUserProps() }; // Similar to above but we ask the user for property values
            }
            
            if (cmdArgs.Length == 3)
            {
                var outPath = cmdArgs[2];
                var headers = new string[] { "Id", "Survival" };
                // A TitanicException will be raised below if the model isn't trained, or the file can't be written to
                var numLines = new CsvUtil().WriteFile(outPath, headers, passengers.Select(p => new object[] { p.Id, Simulate ? (object)(model.Simulate(p)) : (object)(model.Calculate(p)) }));

                return CmdResult.Success(String.Format("Successfully printed {0} lines to {1}", numLines, outPath));
            }
            else
            {
                foreach (var p in passengers)
                    if (Simulate)
                        if (model.Simulate(p))
                            UI.PrintSuccess(String.Format("Passenger {0} survives :-)", p.Id));
                        else
                            UI.PrintError(String.Format("Passenger {0} dies :'(", p.Id));
                    else
                        UI.PrintMessage(String.Format("Passenger {0} has a probability of {1}% to survive", p.Id, Math.Round(model.Calculate(p) * 100, 2)));

                UI.Wait();
                return CmdResult.Success("Done");
            }
        }
    }
}

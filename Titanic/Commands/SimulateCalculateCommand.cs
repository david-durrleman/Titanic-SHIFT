using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Titanic.Data;
using Titanic.Models;
using Titanic.Parsing;

namespace Titanic.Commands
{
    public class SimulateCalculateCommand : Command
    {
        public override string Description
        {
            get
            {
                return String.Format("{0} for the given model with the given set of Passengers, and eventually prints it to the given output file", isSimulate ? "Simulate survival outcome " : "calculates survival probability");
            }
        }

        public override string ArgSyntax
        {
            get { return " <modelId> <Input Path>* <Output Path>*"; }
        }

        private bool isSimulate { get; set; }

        public SimulateCalculateCommand(bool isSimulate)//difference between simulation and calculation is made when creating the command object
            : base()
        {
            this.isSimulate = isSimulate;
        }


        public override CmdResult ExecuteUnsafe(string cmdName, string[] cmdArgs)
        {
            if (cmdArgs.Length < 1 || cmdArgs.Length > 3)
                return UsageFailure(cmdName);
            else
            {
                var modelId = (int)(new IntParser().Parse(cmdArgs[0])); // The parser will throw a TitanicException if an int wasn't supplied. This will be caught in Command.Execute() and result in a failure
                var model = ModelManager.GetModel(modelId);
                var printToConsole = cmdArgs.Length != 3;
                int outputSize = 0;

                if (cmdArgs.Length == 1)
                {
                    UI.WaitMessage(ExecuteCalculateSimulate(new Passenger().WithConsoleInput(), model, printToConsole));
                    outputSize = 1;
                }

                else
                {
                    var allFields = new CsvReader().ReadFile(cmdArgs[1]);
                    var output = ExecuteCalculateSimulate(allFields.Select(fields => new Passenger().WithParsableProps(fields)), model, printToConsole);
                    outputSize = output.Count();
                    if (printToConsole)
                    {
                        output.ToList().ForEach(s => UI.PrintMessage(s));
                        UI.WaitMessage("");
                    }
                    else
                        try
                        {
                            System.IO.File.WriteAllLines(cmdArgs[2], output);
                        }
                        catch (Exception e) { throw new TitanicException(e.Message); };
                }
                return CmdResult.Success(String.Format("Successfully ran simulation on {0} passengers!{1}", outputSize, printToConsole ? "" : "\nOutput printed to "));
            }

        }

        public String ExecuteCalculateSimulate(Passenger p, ModelInstance model, bool printToConsole)
        {
            return printToConsole ?
                String.Format("Passenger {0} (Id {1}) {2}", p.Name, p.Id, isSimulate ? (model.Simulate(p) ? "lives :-)" : "die :'(") : String.Format("has a probability to survive of {0} %", Math.Round(model.Calculate(p) * 100, 2))) :
                String.Format("{0},{1}", p.Id, isSimulate ? (model.Simulate(p) ? 1 : 0) : model.Calculate(p));
        }

        public IEnumerable<String> ExecuteCalculateSimulate(IEnumerable<Passenger> passengers, ModelInstance model, bool printToConsole)
        {
            return passengers.Select(p => ExecuteCalculateSimulate(p, model, printToConsole));
        }







    }
}

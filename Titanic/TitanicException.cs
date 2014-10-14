using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Titanic
{
    // A simple exception class. This is intended to help us achieve the behaviour we want
    // Indeed, some errors can appear quite deep in our code, which we want to report to
    // the user at the prompt (like parse errors in the user input).

    // The best way to do this is to use exceptions as they allow to express complex
    // control flows without having to write the propagation explicitly everywhere into
    // the call graph.

    // We create our custom exception class so we can catch it and display its errors without
    // catching other exceptions which could be bugs in the code and shouldn't necessarily
    // be handled in the same way (in our case they would go unhandled and crash the program,
    // which for our purposes (developer-oriented code) is fine).
    public class TitanicException : Exception
    {
        public TitanicException(string message)
            : base(message)
        {
        }
    }
}

using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Titanic
{
    // This is our csv-file reading class. We configure the file format upon initialization.
    // It has a single method which reads a file and returns an enumerable of arrays, which
    // represent the lines in the file broken down into their separate fields.
    
    // The enumeration is eager, as in it reads all the lines before returning them.
    // We could have also implemented a lazy enumerable, which allows for larger files,
    // without going out of memory, but it isn't really useful here and a bit more difficult
    // to implement when also catching exceptions, which we do.
    public class CsvReader
    {
        public string Delimiters { get; set; }
        public bool HasQuotes { get; set; }
        public bool HasHeaders { get; set; }

        // The default configuration values represent the generally accepted "standard" for CSV
        // files, which helpfully happens to be the one we're dealing with in this project.
        public CsvReader()
        {
            Delimiters = ",";
            HasQuotes = true;
            HasHeaders = true;
        }

        public IEnumerable<string[]> ReadFile(string path)
        {
            var result = new List<string[]>();

            try
            {
                using (var parser = new TextFieldParser(path))
                {
                    parser.SetDelimiters(Delimiters.ToString());
                    parser.HasFieldsEnclosedInQuotes = HasQuotes;

                    if (HasHeaders)
                        parser.ReadFields();

                    while (!parser.EndOfData)
                        result.Add(parser.ReadFields());
                }
            }
            catch (Exception exception)
            {
                // Here we catch all possible exceptions and throw them back as TitanicException to be displayed to
                // the user. As discussed in TitanicException.cs, it's considered bad practice to catch all exceptions
                // (for example, an out of memory exception shouldn't generally be caught because there's nothing much
                // to do at that point).

                // The thing though is that many different exceptions could happen here, either because the file isn't
                // available for reading (e.g. wrong path), or because its format couldn't be parsed. But as I don't want
                // to spend too much time finding out about every possible exception that could happen here, I decided to
                // use a catch-all. For a toy project, this should be fine. In production, not so much...
                throw new TitanicException(String.Format("Failed to parse {0}: {1}", path, exception.Message));
            }

            return result;
        }
    }
}

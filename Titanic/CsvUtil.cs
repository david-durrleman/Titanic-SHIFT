using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Titanic
{
    // This is our csv-file utility class. We configure the file format upon initialization.
    // It has a method which reads a file and returns an enumerable of arrays, which
    // represent the lines in the file broken down into their separate fields.
    // It also provides a converse method which writes an array of field to a file
    // The enumeration is eager, as in it reads all the lines before returning them.
    // We could have also implemented a lazy enumerable, which allows for larger files,
    // without going out of memory, but it isn't really useful here and a bit more difficult
    // to implement when also catching exceptions, which we do.
    public class CsvUtil
    {
        public string Delimiters { get; set; }
        public bool HasQuotes { get; set; }
        public bool HasHeaders { get; set; }
        // The default configuration values represent the generally accepted "standard" for CSV
        // files, which helpfully happens to be the one we're dealing with in this project.
        public CsvUtil()
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
        private void WriteLine(StreamWriter writer, object[] fields)
        {
            var strings = fields.Select(f => f.ToString());
            var quoted = HasQuotes ? strings.Select(f => String.Format(@"""{0}""", f.Replace(@"""", @""""""))) : strings;
            writer.WriteLine(String.Join(",", quoted));
        }
        public int WriteFile(string path, string[] headers, IEnumerable<object[]> fields)
        {
            int numLines = 0;
            try
            {
                using (var writer = new StreamWriter(path))
                {
                    var actualFields = fields;
                    if (HasHeaders && headers != null)
                        actualFields = new object[][] { headers }.Concat(fields);
                    foreach (var f in actualFields)
                    {
                        WriteLine(writer, f);
                        numLines++;
                    }
                }
            }
            catch (Exception exception)
            {
                // Same issue here with the catch-all exception...
                throw new TitanicException(String.Format("Failed to write to {0}: {1}", path, exception.Message));
            }
            return numLines;
        }
        public int WriteFile(string path, IEnumerable<object[]> fields)
        {
            return WriteFile(path, null, fields);
        }
    }
}
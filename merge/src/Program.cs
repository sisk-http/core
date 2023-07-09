using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace src;

internal class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Must specify the Sisk source code directory.");
            return;
        }

        string siskPath = args[0];

        if (!Directory.Exists(siskPath))
        {
            Console.WriteLine("Cannot find the specified path.");
            return;
        }

        string[] skipDirectories = new string[]
        {
            "bin",
            "obj",
            "merge",
            "Properties"
        };

        string[] enumDirectories = Directory.GetDirectories(siskPath)
            .Where(d => !skipDirectories.Any(s => d.EndsWith(s)))
            .ToArray();

        List<string> sourceFiles = new List<string>();

        foreach (string d in enumDirectories)
        {
            string[] dirFiles = Directory.GetFiles(d, "*.cs", SearchOption.AllDirectories);
            sourceFiles.AddRange(dirFiles);
        }

        StringBuilder result = new StringBuilder();
        List<string> usings = new List<string>()
        {
            "System",
            "System.Collections.Generic",
            "System.IO",
            "System.Linq",
            "System.Net.Http",
            "System.Threading",
            "System.Threading.Tasks"
        };

        //Regex namespaceFinder = new Regex(@"\bnamespace\b\s+([a-zA-Z0-9_\.]+)\s*[{;]");
        Regex namespaceFinder = new Regex(@"\bnamespace\b\s+([a-zA-Z0-9_\.]+)\s*[;]", RegexOptions.Compiled);
        Regex usingFinder = new Regex(@"using\s+([\s\w\d_.]+);", RegexOptions.Compiled);

        foreach (string sourceFile in sourceFiles)
        {
            string sf = File.ReadAllText(sourceFile);
            int lastUsingPos = 0;
            var usingResults = usingFinder.Matches(sf);

            foreach (Match gr in usingResults)
            {
                string expression = gr.Groups[1].Value;
                if (!usings.Contains(expression))
                {
                    usings.Add(expression);
                }
                lastUsingPos = gr.Index + gr.Length;
            }

            sf = sf.Substring(lastUsingPos);
            if (namespaceFinder.IsMatch(sf))
            {
                sf = namespaceFinder.Replace(sf, result => result.Value.Replace(";", "{"));
                sf += "}";
            }

            result.AppendLine($"/* {sourceFile.Substring(siskPath.Length)} */");
            result.AppendLine(sf);
        }

        foreach (string import in usings.OrderBy(u => u.Length))
        {
            result.Insert(0, $"using {import};\n");
        }

        File.WriteAllText("output.cs", result.ToString());
        Console.WriteLine($"Merged source code exported to {Path.GetFullPath("output.cs")}");
    }
}

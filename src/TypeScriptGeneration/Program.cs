using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace TypeScriptGeneration
{

    class Program
    {
        class Options
        {
            [Option('i', "input", Required = true,
              HelpText = "Input assemblies.")]
            public IEnumerable<string> InputFiles { get; set; }

            [Option('o', "output", Required = true, HelpText = "Ouput path for the .ts file")]
            public string Ouput { get; set; }


            [Option('c', "camelcase", Required = false, HelpText = "Use camelcase (default: false)", Default = true)]
            public bool CamelCase { get; set; }
        }

        public static int Main(params string[] args)
        {
            var result = CommandLine.Parser.Default.ParseArguments<Options>(args);
            var exitCode = result
              .MapResult(GenerateTypescript, DisplayErrors);
            return exitCode;
        }

        private static int DisplayErrors(IEnumerable<Error> errors)
        {
            foreach (var error in errors)
            {
                Console.WriteLine(error);
            }
            return 1;
        }

        private static int GenerateTypescript(Options options)
        {
            Console.WriteLine("Assemblies to parse: {0}", string.Join(",", options.InputFiles.ToArray()));
            Console.WriteLine("Output file: " + options.Ouput);
            Console.WriteLine("Camelcase: " + options.CamelCase);
            var assemblies = options.InputFiles.Select(Assembly.LoadFrom).ToArray();

            var typescript = options.CamelCase
                ? TypeScriptBuilder.GetTypescriptContractsCamelCase(assemblies)
                : TypeScriptBuilder.GetTypescriptContracts(assemblies);

            File.WriteAllText(options.Ouput, typescript, Encoding.UTF8);
            return 0;
        }
    }
}

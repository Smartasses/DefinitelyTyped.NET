using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CommandLine;

namespace DefinitelyTypedNet
{

    class Program
    {
        class Options
        {
            [Option('i', "input", Required = true,
              HelpText = "Input assemblies.")]
            public IEnumerable<string> InputAssemblies { get; set; }

            [Option('o', "output", Required = true, HelpText = @"Ouput fully path for the .ts file (c:\myfolder\generated.ts)")]
            public string OutputFile { get; set; }


            [Option('c', "camelcase", Required = false, HelpText = "Use camelcase (default = true)", Default = true)]
            public bool CamelCase { get; set; }

            [Option('t', "typemapping", Required = false, HelpText ="Map types to default typescript files using Typename@buildintype",Separator = ';')]
            public IEnumerable<string> BuiltinTypeMappings { get; set; }
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
            Console.WriteLine("Assemblies to parse: {0}", string.Join(",", options.InputAssemblies.ToArray()));
            Console.WriteLine("Output file: " + options.OutputFile);
            Console.WriteLine("Camelcase: " + options.CamelCase);

            foreach(var typeMap in options.BuiltinTypeMappings)
            {
                var parsedMap = typeMap.Split('@');
                Console.WriteLine("Adding default type for {0} mapping on {1}", parsedMap[0], parsedMap[1]);           
                
                TypeScriptConvert.AddTypeMapping(Type.GetType(parsedMap[0], true, true), parsedMap[1]);
            }

            var assemblies = options.InputAssemblies.Select(Assembly.LoadFrom).ToArray();

            var typescript = options.CamelCase
                ? TypeScriptBuilder.GetTypescriptContractsCamelCase(assemblies)
                : TypeScriptBuilder.GetTypescriptContracts(assemblies);

            File.WriteAllText(options.OutputFile, typescript, Encoding.UTF8);
            return 0;
        }
    }
}

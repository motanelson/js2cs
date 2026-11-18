using Cosmos.HAL;
using Cosmos.HAL.BlockDevice.Registers;
using System;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text;
using System.Text.RegularExpressions;
using Sys = Cosmos.System;







namespace js2cs
{
    public class Kernel : Sys.Kernel
    {

 
        Sys.FileSystem.CosmosVFS fs;
        protected override void BeforeRun()
        {
            fs = new Sys.FileSystem.CosmosVFS();
            Sys.FileSystem.VFS.VFSManager.RegisterVFS(fs);
        }


        protected override void Run()
        {
            while (true)
            {
                Console.BackgroundColor = ConsoleColor.White;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.Clear();

                try
                {
                    var fs_type = fs.GetFileSystemType(@"0:\");
                    Console.WriteLine("File System Type: " + fs_type);

                    string[] directory_list = Directory.GetFiles(@"0:\");

                    Console.WriteLine();
                    Console.WriteLine("FILES:");

                    foreach (string file in directory_list)
                    {
                        Console.WriteLine(file);
                    }

                    var available_space = fs.GetAvailableFreeSpace(@"0:\");
                    Console.WriteLine("Available Free Space: " + available_space);

                    Console.WriteLine();
                    Console.Write("Input JS file: ");
                    string inputFile = Console.ReadLine();

                    Console.Write("Output CS file: ");
                    string outputFile = Console.ReadLine();

                    string[] args = new string[2];

                    args[0] = "0:\\" + inputFile;
                    args[1] = "0:\\" + outputFile;

                    Console.WriteLine();
                    Console.WriteLine("Starting conversion...");

                    Programs.Main(args);
                }
                catch (Exception ex)
                {
                    Console.WriteLine();
                    Console.WriteLine("EXCEPTION!");
                    Console.WriteLine(ex.ToString());
                }

                Console.WriteLine();
                Console.WriteLine("Press ENTER to continue...");
                Console.ReadLine();
            }
        }


    }
    public class Programs
    {
        public static int Main(string[] args)
        {
            
            // Check command-line arguments.
            if (args.Length < 2)
            {
                Console.WriteLine("JavaScript to C# Converter");
                Console.WriteLine();
                Console.WriteLine("Usage:");
                Console.WriteLine("  JsToCsConverter.exe <input.js> <output.cs>");
                Console.WriteLine();
                Console.WriteLine("Example:");
                Console.WriteLine("  JsToCsConverter.exe program.js program.cs");
                return 1;
            }

            string inputFile = args[0];
            string outputFile = args[1];

            try
            {
                if (!File.Exists(inputFile))
                {
                    Console.Error.WriteLine("Error: Input file not found: " + inputFile);
                    return 1;
                }

                // Read the JavaScript source file.
                string jsText = File.ReadAllText(inputFile, Encoding.UTF8);

                // Convert JavaScript source code to C#.
                string convertedCode = ConvertJsToCSharp(jsText);

                // Create the output directory when necessary.
                string outputDirectory = Path.GetDirectoryName(Path.GetFullPath(outputFile));
                if (!string.IsNullOrEmpty(outputDirectory))
                    Directory.CreateDirectory(outputDirectory);

                // Write the generated C# source file.
                File.WriteAllText(outputFile, convertedCode, new UTF8Encoding(false));

                Console.WriteLine("Converted Successfully");
                Console.WriteLine("Input : " + inputFile);
                Console.WriteLine("Output: " + outputFile);
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Conversion error: " + ex.Message);
                return 1;
            }
        }

        /// <summary>
        /// Converts JavaScript source code to formatted C#.
        /// This follows the replacement rules of the original JavaScript application.
        /// </summary>
        private static string ConvertJsToCSharp(string js)
        {
            // 1. Preserve string literals and template literals.
            List<string> strings = new List<string>();
            string temp = js;

            // Extract template strings and convert ${value} to C# interpolation.
            temp = Regex.Replace(
                temp,
                @"`([^`\\]*(\\.[^`\\]*)*)`",
                delegate (Match match)
                {
                    string content = match.Groups[1].Value;
                    string interpolated = "$\"" +
                        Regex.Replace(content, @"\$\{([^}]+)\}", "{$1}") +
                        "\"";

                    strings.Add(interpolated);
                    return "__STR_" + (strings.Count - 1) + "__";
                });

            // Extract regular double-quoted and single-quoted strings.
            temp = Regex.Replace(
                temp,
                "([\"'])(?:\\\\.|(?!\\1)[^\\\\])*?\\1",
                delegate (Match match)
                {
                    string value = match.Value;

                    // Normalize single-quoted strings to double-quoted strings.
                    if (value.StartsWith("'") && value.EndsWith("'") && value.Length > 3)
                    {
                        string inner = value.Substring(1, value.Length - 2);
                        inner = inner.Replace("\"", "\\\"");
                        value = "\"" + inner + "\"";
                    }

                    strings.Add(value);
                    return "__STR_" + (strings.Count - 1) + "__";
                });

            // 2. Function mappings.
            temp = Regex.Replace(temp, @"\bmessage\.log\s*\(", "Console.WriteLine(");
            temp = Regex.Replace(temp, @"\bprint\s*\(", "Console.WriteLine(");
            temp = Regex.Replace(temp, @"\bconsole\.log\s*\(", "Console.WriteLine(");
            temp = Regex.Replace(temp, @"\bconsole\.error\s*\(", "Console.Error.WriteLine(");

            // 3. Keywords and operator replacements.
            temp = Regex.Replace(temp, @"===", "==");
            temp = Regex.Replace(temp, @"!==", "!=");
            temp = Regex.Replace(temp, @"\blet\s+", "var ");
            temp = Regex.Replace(temp, @"\bconst\s+", "var ");
            temp = Regex.Replace(temp, @"\bundefined\b", "null");

            // Math library mappings.
            temp = Regex.Replace(temp, @"\bMath\.floor\b", "Math.Floor");
            temp = Regex.Replace(temp, @"\bMath\.ceil\b", "Math.Ceiling");
            temp = Regex.Replace(temp, @"\bMath\.round\b", "Math.Round");
            temp = Regex.Replace(temp, @"\bMath\.abs\b", "Math.Abs");
            temp = Regex.Replace(temp, @"\bMath\.max\b", "Math.Max");
            temp = Regex.Replace(temp, @"\bMath\.min\b", "Math.Min");
            temp = Regex.Replace(temp, @"\bMath\.sqrt\b", "Math.Sqrt");
            temp = Regex.Replace(temp, @"\bMath\.pow\b", "Math.Pow");
            temp = Regex.Replace(temp, @"\bMath\.PI\b", "Math.PI");

            // Common collection and string helpers.
            temp = Regex.Replace(temp, @"\.length\b", ".Length");
            temp = Regex.Replace(temp, @"\.toLowerCase\(\)", ".ToLower()");
            temp = Regex.Replace(temp, @"\.toUpperCase\(\)", ".ToUpper()");
            temp = Regex.Replace(temp, @"\.push\(", ".Add(");

            // Convert function declarations to public static void methods.
            temp = Regex.Replace(
                temp,
                @"function\s+([a-zA-Z0-9_$]+)\s*\(([^)]*)\)",
                delegate (Match match)
                {
                    string functionName = match.Groups[1].Value;
                    string arguments = match.Groups[2].Value;
                    string typedArguments = "";

                    if (arguments.Trim().Length > 0)
                    {
                        string[] parts = arguments.Split(',');
                        List<string> convertedArguments = new List<string>();

                        foreach (string part in parts)
                            convertedArguments.Add("dynamic " + part.Trim());

                        typedArguments = string.Join(", ", convertedArguments);
                    }

                    return "public static void " + functionName +
                           "(" + typedArguments + ")";
                });

            // 4. Restore preserved strings.
            temp = Regex.Replace(
                temp,
                @"__STR_(\d+)__",
                delegate (Match match)
                {
                    int index = int.Parse(match.Groups[1].Value);
                    return strings[index];
                });

            // 5. Indent the converted code inside the generated Main method.
            string[] lines = temp.Split('\n');
            StringBuilder indentedBody = new StringBuilder();

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].TrimEnd('\r');

                if (line.Length > 0)
                    indentedBody.Append("            ").Append(line);

                if (i < lines.Length - 1)
                    indentedBody.Append('\n');
            }

            // Return the complete C# program.
            return
                "using System;\n" +
                "using System.Collections.Generic;\n" +
                "using System.Linq;\n" +
                "\n" +
                "namespace TranspiledApp\n" +
                "{\n" +
                "    public class Program\n" +
                "    {\n" +
                "        public static void Main(string[] args)\n" +
                "        {\n" +
                indentedBody.ToString() + "\n" +
                "        }\n" +
                "    }\n" +
                "}\n";
        }
    }
}

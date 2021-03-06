using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using IllusionScript.Runtime;
using IllusionScript.Runtime.Binding;
using IllusionScript.Runtime.Compiling;
using IllusionScript.Runtime.Interpreting.Memory.Symbols;

namespace IllusionScript.Compiler.BCC
{
    public class Compiler : CompilerConnector
    {
        public override string name => "bcc";
        private string outDir;
        private string objDir;
        private string binDir;
        private string nameFile;

        public override bool BuildOutput()
        {
            string baseOutput = Path.Combine(baseDir, "out", name);
            if (!Directory.Exists(baseOutput))
            {
                Directory.CreateDirectory(baseOutput);
            }

            outDir = baseOutput;

            objDir = Path.Combine(outDir, "obj");
            if (!Directory.Exists(objDir))
            {
                Directory.CreateDirectory(objDir);
            }

            binDir = Path.Combine(outDir, "bin");
            if (!Directory.Exists(binDir))
            {
                Directory.CreateDirectory(binDir);
            }

            nameFile = Path.Combine(outDir, "name.txt");
            if (!File.Exists(nameFile))
            {
                Console.WriteLine("Enter the name of your project");
                string data = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(data))
                {
                    File.WriteAllText(nameFile, "program.ile");
                }
                else
                {
                    if (!data.EndsWith(".ile"))
                    {
                        data += ".ile";
                    }

                    File.WriteAllText(nameFile, data);
                }
            }


            return true;
        }

        public override bool BuildCore()
        {
            writer.WriteLine("Build Core ...");
            // writer.WriteLine("Bind Syscalls");
            // string syscall = Path.Combine(outDir, "syscall.php");
            // File.WriteAllText(syscall, SyscallBinder());

            return true;
        }

        public override bool Build(Compilation compilation, BoundProgram program)
        {
            List<string> files = new List<string>(compilation.functions.Length);
            AddressManager addressManager = new AddressManager();
            foreach (FunctionSymbol function in compilation.functions)
            {
                string path = Path.Combine(objDir, function.name + ".ilo");

                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                writer.WriteLine(
                    $"Compile item: {Path.GetFullPath(function.declaration.location.text.filename)}[{function.name}]");
                CompilerFiles file = new CompilerFiles(File.Open(path, FileMode.Create), addressManager);
                file.Write(function, program.functionBodies);
                file.Close();
                files.Add(path);
            }

            string filename = File.ReadAllText(nameFile);
            string outFile = Path.Combine(binDir, filename);

            if (File.Exists(outFile))
            {
                File.Delete(outFile);
            }

            using StreamWriter streamWriter = new StreamWriter(outFile);

            /*
                ile header
                8bit version 
                8bit entry point
             */

            byte[] bytes = new byte[8];
            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[i] = 0;
            }

            string s = Information.getLibVersion();
            for (var i = 0; i < s.Length; i++)
            {
                char c = s[i];
                bytes[i] = Encoding.ASCII.GetBytes(c.ToString())[0];
            }

            streamWriter.WriteBytes(bytes);

            bytes = new byte[8];
            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[i] = 0;
            }

            streamWriter.WriteBytes(
                CompilerFiles.ToByte(addressManager.Get((program.mainFunction ?? program.scriptFunction).name), 8));

            foreach (string file in files)
            {
                byte[] content = File.ReadAllBytes(file);
                int end = Array.LastIndexOf(content, (byte)2);

                streamWriter.WriteBytes(content, 0, end);
            }

            streamWriter.WriteBytes(new byte[] { (int)KeywordCollection.ItemEnd + 1, 0, 0, 0, 0, 0, 0, 0 });
            streamWriter.Close();

            writer.WriteLine("Finished compiling");
            writer.WriteLine($"BCC Executable at: {outFile}");

            return true;
        }

        public override bool CleanUp()
        {
            return true;
        }

        public override string SyscallBinder()
        {
            return "";
        }
    }
}

/*  TODO write interpreter for ile files
 *  Create a new Project
 *  Implement the new compiler
 *  ILL file implementation
 *  Init github workflows
 */
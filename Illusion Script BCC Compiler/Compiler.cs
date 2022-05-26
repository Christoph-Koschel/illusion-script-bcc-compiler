using System.Collections.Generic;
using System.IO;
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
            foreach (FunctionSymbol function in compilation.functions)
            {
                string path = Path.Combine(objDir, function.name + ".ilo");

                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                writer.WriteLine($"Compile item: {Path.GetFullPath(function.declaration.location.text.filename)}");
                CompilerFiles file = new CompilerFiles(File.Open(path, FileMode.Create));
                file.Write(function, program.functionBodies);
                file.Close();
                files.Add(path);
            }

            string outFile = Path.Combine(binDir, "program.ile");

            if (File.Exists(outFile))
            {
                File.Delete(outFile);
            }

            using StreamWriter streamWriter = new StreamWriter(outFile);
            foreach (string file in files)
            {
                streamWriter.WriteBytes(File.ReadAllBytes(file));
            }

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
 *  Program header for ile file
 *   - Add version number to header
 *  Setting system for project
 *  If name.txt not exists in out dir ask for the output name and write it to the file
 */
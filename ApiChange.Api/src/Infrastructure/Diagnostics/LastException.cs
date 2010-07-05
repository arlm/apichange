using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace ApiChange.Infrastructure
{
    class LastException
    {
        static FieldInfo myThreadPointerFieldInfo;
        static int myThreadOffset = -1;

        PtrConverter<Exception> myConverter = new PtrConverter<Exception>();
        
        public LastException()
        {
            if (myThreadPointerFieldInfo == null)
            {
                // Dont read the don´t or you will get scared. 
                // I prefer to read this more like: If you dont know the rules you will
                // get biten by the next .NET Framework update
                myThreadPointerFieldInfo = typeof(Thread).GetField("DONT_USE_InternalThread",
                   BindingFlags.Instance | BindingFlags.NonPublic);
            }

            if (myThreadOffset == -1)
            {
                ClrContext context = ClrContext.None;
                context |= (IntPtr.Size == 8) ? ClrContext.Is64Bit : ClrContext.Is32Bit;
                context |= (Environment.Version.Major == 2) ? ClrContext.IsNet2 : ClrContext.None;
                context |= (Environment.Version.Major == 4) ? ClrContext.IsNet4 : ClrContext.None;

                switch(context)
                {
                    case ClrContext.Is32Bit|ClrContext.IsNet2:
                        myThreadOffset = 0x180;
                        break;
                    case ClrContext.Is32Bit|ClrContext.IsNet4:
                        myThreadOffset = 0x188;
                        break;
                    case ClrContext.Is64Bit|ClrContext.IsNet2:
                        myThreadOffset = 0x240;
                        break;
                    case ClrContext.Is64Bit|ClrContext.IsNet4:
                        myThreadOffset = 0x250;
                        break;

                    default: // ups who did install .NET 5?
                        myThreadOffset = -1;
                        break;
                }
            }
        }

        /// <summary>
        /// Get from the current thread the last thrown exception object.
        /// </summary>
        /// <returns>null when none exists or the exception instance.</returns>
        public Exception GetLastException()
        {
            Exception lret = null;
            if (myThreadPointerFieldInfo != null)
            {
                IntPtr pInternalThread = (IntPtr)myThreadPointerFieldInfo.GetValue(Thread.CurrentThread);
                if (pInternalThread != IntPtr.Zero && myThreadOffset != -1)
                {
                    IntPtr ppEx = Marshal.ReadIntPtr(pInternalThread, myThreadOffset);
                    if (ppEx != IntPtr.Zero)
                    {
                        IntPtr pEx = Marshal.ReadIntPtr(ppEx);
                        if (pEx != IntPtr.Zero)
                        {
                            lret = myConverter.ConvertFromIntPtr(pEx);
                        }
                    }
                }
            }
            return lret;
        }
    }

    /// <summary>
    /// There are currently 4 combinations possible but I want to 
    /// stay extensible since some .NET Framework patch might need
    /// additional treatement.
    /// </summary>
    [Flags]
    enum ClrContext
    {
        None,
        Is32Bit = 1,
        Is64Bit = 2,
        IsNet2 = 4,
        IsNet4 = 8
    }
}

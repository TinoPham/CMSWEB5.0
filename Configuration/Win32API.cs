using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.NetworkInformation;
using System.Net;
using System.Collections;
using System.Runtime.InteropServices;

namespace ServiceConfig
{
    public sealed class Win32API: Commons.SingletonClassBase<Win32API>
    {
        [DllImport("iphlpapi.dll", SetLastError = true)]
        static extern uint GetExtendedTcpTable(IntPtr pTcpTable, ref int dwOutBufLen, bool sort, int ipVersion, TCP_TABLE_CLASS tblClass, uint reserved = 0);
        [StructLayout(LayoutKind.Sequential)]
        public struct MIB_TCPROW_OWNER_PID
        {
            // DWORD is System.UInt32 in C#
            public System.UInt32 state;
            public System.UInt32 localAddr;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] localPort;
            public System.UInt32 remoteAddr;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] remotePort;
            public System.UInt32 owningPid;

            public System.Net.IPAddress LocalAddress
            {
                get
                {
                    return new System.Net.IPAddress(localAddr);
                }
            }

            public ushort LocalPort
            {
                get
                {
                    if (localPort == null)
                        return 0;

                    return BitConverter.ToUInt16(
                    new byte[2] { localPort[1], localPort[0] }, 0);
                }
            }

            public System.Net.IPAddress RemoteAddress
            {
                get
                {

                    return new System.Net.IPAddress(remoteAddr);
                }
            }

            public ushort RemotePort
            {
                get
                {
                    return BitConverter.ToUInt16(
                    new byte[2] { remotePort[1], remotePort[0] }, 0);
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MIB_TCPTABLE_OWNER_PID
        {
            public uint dwNumEntries;
            MIB_TCPROW_OWNER_PID table;
        }

        enum TCP_TABLE_CLASS
        {
            TCP_TABLE_BASIC_LISTENER,
            TCP_TABLE_BASIC_CONNECTIONS,
            TCP_TABLE_BASIC_ALL,
            TCP_TABLE_OWNER_PID_LISTENER,
            TCP_TABLE_OWNER_PID_CONNECTIONS,
            TCP_TABLE_OWNER_PID_ALL,
            TCP_TABLE_OWNER_MODULE_LISTENER,
            TCP_TABLE_OWNER_MODULE_CONNECTIONS,
            TCP_TABLE_OWNER_MODULE_ALL
        }

        //public TcpRow[] GetAllTcpConnections()
        private MIB_TCPROW_OWNER_PID[] GetAllTcps(TCP_TABLE_CLASS tcpclass)
        {
            //  TcpRow is my own class to display returned rows in a nice manner.
            //    TcpRow[] tTable;
            MIB_TCPROW_OWNER_PID[] tTable;
            int AF_INET = 2;    // IP_v4
            int buffSize = 0;

            // how much memory do we need?
            uint ret = GetExtendedTcpTable(IntPtr.Zero, ref buffSize, true, AF_INET, tcpclass);
            IntPtr buffTable = Marshal.AllocHGlobal(buffSize);

            try
            {
                ret = GetExtendedTcpTable(buffTable, ref buffSize, true, AF_INET, tcpclass);
                if (ret != 0)
                {
                    return null;
                }

                // get the number of entries in the table
                //MibTcpTable tab = (MibTcpTable)Marshal.PtrToStructure(buffTable, typeof(MibTcpTable));
                MIB_TCPTABLE_OWNER_PID tab = (MIB_TCPTABLE_OWNER_PID)Marshal.PtrToStructure(buffTable, typeof(MIB_TCPTABLE_OWNER_PID));
                //IntPtr rowPtr = (IntPtr)((long)buffTable + Marshal.SizeOf(tab.numberOfEntries) );
                IntPtr rowPtr = (IntPtr)((long)buffTable + Marshal.SizeOf(tab.dwNumEntries));
                // buffer we will be returning
                //tTable = new TcpRow[tab.numberOfEntries];
                tTable = new MIB_TCPROW_OWNER_PID[tab.dwNumEntries];

                //for (int i = 0; i < tab.numberOfEntries; i++)        
                for (int i = 0; i < tab.dwNumEntries; i++)
                {
                    //MibTcpRow_Owner_Pid tcpRow = (MibTcpRow_Owner_Pid)Marshal.PtrToStructure(rowPtr, typeof(MibTcpRow_Owner_Pid));
                    MIB_TCPROW_OWNER_PID tcpRow = (MIB_TCPROW_OWNER_PID)Marshal.PtrToStructure(rowPtr, typeof(MIB_TCPROW_OWNER_PID));
                    //tTable[i] = new TcpRow(tcpRow);
                    tTable[i] = tcpRow;
                    rowPtr = (IntPtr)((long)rowPtr + Marshal.SizeOf(tcpRow));   // next entry
                }

            }
            finally
            {
                // Free the Memory
                Marshal.FreeHGlobal(buffTable);
            }


            return tTable;
        }

        //public TcpRow[] GetAllTcpConnections()
        public MIB_TCPROW_OWNER_PID[] GetAllTcpConnections()
        {
            return GetAllTcps(TCP_TABLE_CLASS.TCP_TABLE_OWNER_PID_ALL);
        }
        public MIB_TCPROW_OWNER_PID[] GetAllTcpListener()
        {
            return GetAllTcps(TCP_TABLE_CLASS.TCP_TABLE_OWNER_PID_LISTENER);
        }
        
    }
}

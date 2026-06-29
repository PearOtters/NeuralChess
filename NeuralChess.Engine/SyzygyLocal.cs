using System;
using System.Runtime.InteropServices;

namespace NeuralChess.Engine
{
    public static class SyzygyLocal
    {
        private const string FathomLibrary = "fathom";

        [DllImport(FathomLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool tb_init(string path);

        [DllImport(FathomLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern void tb_free();

        [DllImport(FathomLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint tb_probe_wdl(
            ulong white, 
            ulong black, 
            ulong kings, 
            ulong queens, 
            ulong rooks, 
            ulong bishops, 
            ulong knights, 
            ulong pawns, 
            uint rule50, 
            uint castling, 
            uint ep, 
            bool turn);
    }
}
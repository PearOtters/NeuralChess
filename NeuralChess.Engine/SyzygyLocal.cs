using System;
using System.Runtime.InteropServices;

namespace NeuralChess.Engine
{
    public static class SyzygyLocal
    {
        private const string FathomLibrary = "fathom";

        [DllImport(FathomLibrary, EntryPoint = "tb_init", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern bool tb_init(string path);

        [DllImport(FathomLibrary, EntryPoint = "tb_free", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void tb_free();

        [DllImport(FathomLibrary, EntryPoint = "tb_probe_wdl_impl", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern uint tb_probe_wdl(
            ulong white, 
            ulong black, 
            ulong kings, 
            ulong queens, 
            ulong rooks, 
            ulong bishops, 
            ulong knights, 
            ulong pawns, 
            uint ep, 
            bool turn);
    }
}
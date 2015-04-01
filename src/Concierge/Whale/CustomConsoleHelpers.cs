using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Qoollo.Concierge.Whale
{
    /// <summary>
    /// Some help 
    /// </summary>
    public class CustomConsoleHelpers
    {
        public enum CtrlTypes
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        private static Action _innerDelegate;


        // ============

        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(HandlerRoutine Handler, bool Add);

        [DllImport("kernel32")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32")]
        private static extern IntPtr GetSystemMenu(IntPtr hwnd, int bRevert);

        [DllImport("user32")]
        private static extern bool EnableMenuItem(IntPtr hMenu, uint itemId, uint uEnable);


        // =================

        private static bool OnConsoleClose(CtrlTypes ctrlType)
        {
            Action tmp = _innerDelegate;
            if (tmp != null)
            {
                if (ctrlType == CtrlTypes.CTRL_BREAK_EVENT ||
                    //ctrlType == CtrlTypes.CTRL_C_EVENT ||
                    ctrlType == CtrlTypes.CTRL_CLOSE_EVENT ||
                    ctrlType == CtrlTypes.CTRL_LOGOFF_EVENT ||
                    ctrlType == CtrlTypes.CTRL_SHUTDOWN_EVENT)
                {
                    tmp();
                }
            }


            return true;
        }


        public static event Action ConsoleClose
        {
            add
            {
                if (_innerDelegate == null)
                    SetConsoleCtrlHandler(OnConsoleClose, true);

                _innerDelegate += value;
            }
            remove { _innerDelegate -= value; }
        }

        // =============


        public static void DisableConsoleCloseButton()
        {
            IntPtr cnslWnd = GetConsoleWindow();
            if (cnslWnd == IntPtr.Zero)
                return;

            IntPtr sysMenu = GetSystemMenu(cnslWnd, 0);
            if (sysMenu == IntPtr.Zero)
                return;

            EnableMenuItem(sysMenu, (uint) SysMenuElems.SC_CLOSE, 1);
        }

        public static void EnableConsoleCloseButton()
        {
            IntPtr cnslWnd = GetConsoleWindow();
            if (cnslWnd == IntPtr.Zero)
                return;

            IntPtr sysMenu = GetSystemMenu(cnslWnd, 0);
            if (sysMenu == IntPtr.Zero)
                return;

            EnableMenuItem(sysMenu, (uint) SysMenuElems.SC_CLOSE, 0);
        }


        // ===================


        public static void SetConsoleTitleFromAssemblyInfo(string name)
        {
            Assembly curAssembly = Assembly.GetEntryAssembly();
            Console.Title = String.Format(name + ". v{0}.   Path: {1}", curAssembly.GetName().Version,
                curAssembly.Location);
        }

        public static string WaitForInput(params string[] inputs)
        {
            Contract.Requires(inputs != null && inputs.Length > 0);

            string newLn = null;
            do
            {
                newLn = Console.ReadLine();
            } while (newLn != null && !(inputs.Contains(newLn) || inputs.Contains(newLn.Trim())));

            return newLn;
        }

        public static string PlaceStringAtCenter(string str, int expectedWidth, string fillingStr = "=")
        {
            int restStrLength = expectedWidth - str.Length - 2;
            if (restStrLength <= 0)
                return fillingStr + " " + str + " " + fillingStr;
            if (fillingStr.Length <= 0)
                return str;

            int half = restStrLength/2;
            int fillLen = fillingStr.Length;

            var bld = new StringBuilder(expectedWidth);
            for (int i = 0; i < half; i += fillLen)
                bld.Append(fillingStr);
            bld.Append(" ").Append(str).Append(" ");
            for (int i = half; i < restStrLength; i += fillLen)
                bld.Append(fillingStr);

            return bld.ToString();
        }

        public static string PlaceStringAtWidth(string source, string add, int expectedWidth)
        {
            int count = expectedWidth - source.Length;
            if (count <= 0)
                return source + " " + add;

            var builder = new StringBuilder(source);
            for (int i = 0; i < count; i++)
            {
                builder.Append(" ");
            }
            builder.Append(add);
            return builder.ToString();
        }

        public static string FormatHelp(IEnumerable<string> lines)
        {
            var builder = new StringBuilder();

            foreach (string line in lines)
            {
                string[] split = line.Split(new[] {' '}, 2);
                builder.AppendLine(split.Length < 2 ? split[0] : PlaceStringAtWidth(split[0], split[1], 20));
            }
            return builder.ToString();
        }

        private delegate bool HandlerRoutine(CtrlTypes CtrlType);

        private enum SysMenuElems : uint
        {
            SC_SIZE = 61440,
            SC_MOVE = 61456,
            SC_MINIMIZE = 61472,
            SC_MAXIMIZE = 61488,
            SC_NEXTWINDOW = 61504,
            SC_PREVWINDOW = 61520,
            SC_CLOSE = 61536,
            SC_VSCROLL = 61552,
            SC_HSCROLL = 61568,
            SC_MOUSEMENU = 61584,
            SC_KEYMENU = 61696,
            SC_ARRANGE = 61712,
            SC_RESTORE = 61728,
            SC_TASKLIST = 61744,
            SC_SCREENSAVE = 61760,
            SC_HOTKEY = 61776,
            SC_DEFAULT = 61792,
            SC_MONITORPOWER = 61808,
            SC_CONTEXTHELP = 61824,
            SC_SEPARATOR = 61455,
            SC_ICON = SC_MINIMIZE,
            SC_ZOOM = SC_MAXIMIZE
        }
    }
}
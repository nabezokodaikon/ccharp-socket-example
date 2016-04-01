using CommonLib.Command;
using CommonLib.ThreadSafe;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SocketClient.ConnectionIF
{
    class ClientConnection
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private Thread worker = null;
        private readonly ThreadSafeBoolean isShouldDisconnect = new ThreadSafeBoolean(false);
        private readonly LockFreeQueue<ExampleCommand> commandQueue = new LockFreeQueue<ExampleCommand>();
        private readonly object syncLock = new object();

        public ClientConnection()
        {

        }

        public void Connect()
        {
            lock (this.syncLock)
            {
                if (this.isConnecting())
                {
                    return;
                }

                log.Debug("通信スレッドが開始します。");

                this.isShouldDisconnect.Value = false;
                this.worker = new Thread(this.work);
                this.worker.Start();
                while (!this.worker.IsAlive)
                {
                    Thread.Sleep(0);
                }
            }
        }

        public void Disconnect()
        {
            lock (this.syncLock)
            {
                if (!this.isConnecting())
                {
                    return;
                }

                this.isShouldDisconnect.Value = true;
                this.worker.Join();
                this.worker = null;

                log.Debug("通信スレッドが終了しました。");
            }
        }

        public void Send(string contents)
        {
            lock (this.syncLock)
            {
                if (!this.isConnecting())
                {
                    return;
                }

                var command = new ExampleCommand(contents);
                this.commandQueue.Enqueue(command);
            }
        }

        private bool isConnecting()
        {
            if (this.worker != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void work()
        {
            const string host = "localhost";
            const int port = 9000;
            const int sendDataSize = 10;

            Thread.CurrentThread.Name = "Com";
            log.Debug("通信スレッドが開始しました。");

            try
            {
                using (var tcp = new TcpClient())
                {
                    while (true)
                    {
                        if (this.isShouldDisconnect.Value) return;

                        try
                        {
                            tcp.Connect(host, port);
                        }
                        catch (SocketException ex)
                        {
                            log.ErrorException("接続に失敗しました。", ex);
                            Thread.Sleep(1000);
                            continue;
                        }

                        log.Debug("接続しました。");

                        NetworkStream ns = null;
                        try
                        {
                            ns = tcp.GetStream();
                        }
                        catch (InvalidOperationException ex)
                        {
                            log.ErrorException("ネットワークストリームの取得に失敗しました。", ex);
                            if (ns != null)
                            {
                                ns.Dispose();
                            }

                            Thread.Sleep(1000);
                            continue;
                        }

                        log.Debug("ネットワークストリームを取得しました。");

                        this.commandQueue.Clear();

                        while (true)
                        {
                            if (this.isShouldDisconnect.Value) return;

                            var command = this.commandQueue.Dequeue();
                            if (command == null)
                            {
                                Thread.Sleep(1000);
                                continue;
                            }

                            log.Debug("コマンドの送信を開始します。");

                            try
                            {
                                var bin = command.ToBinary();
                                var offset = 0;
                                while (offset < bin.Length)
                                {
                                    if (this.isShouldDisconnect.Value) return;

                                    var buffer = bin.Skip(0).Take(sendDataSize).ToArray();
                                    ns.Write(buffer, 0, buffer.Length);
                                    offset += buffer.Length;

                                    Thread.Sleep(1000);
                                }
                            }
                            catch (IOException ex)
                            {
                                log.ErrorException("コマンドの送信が失敗しました。", ex);
                                ns.Dispose();
                                break;
                            }

                            log.Debug("コマンドの送信が終了しました。");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorException("通信スレッドで例外が発生しました。", ex);
            }
            finally
            {
                log.Debug("通信スレッドが終了します。");
            }
        }
    }
}

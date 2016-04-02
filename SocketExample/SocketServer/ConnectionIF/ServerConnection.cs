using CommonLib.Command;
using CommonLib.ThreadSafe;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SocketServer.ConnectionIF
{
    class ServerConnection
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private Thread worker = null;
        private readonly ThreadSafeBoolean isShouldClose = new ThreadSafeBoolean(false);
        private readonly object syncLock = new object();

        public ServerConnection()
        {

        }

        public void Open()
        {
            lock (this.syncLock)
            {
                if (this.isConnecting())
                {
                    return;
                }

                log.Debug("通信スレッドが開始します。");

                this.isShouldClose.Value = false;
                this.worker = new Thread(this.work);
                this.worker.Start();
                while (!this.worker.IsAlive)
                {
                    Thread.Sleep(0);
                }
            }
        }

        public void Close()
        {
            lock (this.syncLock)
            {
                if (!this.isConnecting())
                {
                    return;
                }

                this.isShouldClose.Value = true;
                this.worker.Join();
                this.worker = null;

                log.Debug("通信スレッドが終了しました。");
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
            const string host = "127.0.0.1";
            const int port = 9000;

            Thread.CurrentThread.Name = "Com";
            log.Debug("通信スレッドが開始しました。");

            try
            {
                while (true)
                {
                    if (this.isShouldClose.Value) return;

                    var listener = new TcpListener(IPAddress.Parse(host), port);

                    try
                    {
                        listener.Start();
                        log.Debug("リスナーを開始しました。");

                        try
                        {
                            while (true)
                            {
                                if (this.isShouldClose.Value) return;

                                while (!listener.Pending())
                                {
                                    if (this.isShouldClose.Value) return;
                                    Thread.Sleep(1000);
                                }

                                this.innerWork(listener);
                            }
                        }
                        catch (SocketException ex)
                        {
                            log.ErrorException("通信処理で例外が発生しました。", ex);
                        }
                        catch (InvalidOperationException ex)
                        {
                            log.ErrorException("通信処理で例外が発生しました。", ex);
                        }
                    }
                    finally
                    {
                        try
                        {
                            listener.Stop();
                        }
                        catch (SocketException ex)
                        {
                            log.ErrorException("リスナーの停止に失敗しました。", ex);
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

        private void innerWork(TcpListener listener)
        {
            const int readDataSize = 10;

            using (var client = listener.AcceptTcpClient())
            using (var stream = client.GetStream())
            {
                log.Debug("保留中の接続要求を受け入れました。");

                while (true)
                {
                    if (this.isShouldClose.Value) return;
                    if (listener.Pending()) return; // 次の接続要求が存在するため、既存の接続を終了する。

                    var headerAvailable = client.Available;
                    var headerDataAvailable = stream.DataAvailable;
                    log.Debug("headerAvailable = {0}", headerAvailable);
                    log.Debug("headerDataAvailable = {0}", headerDataAvailable);

                    if (headerAvailable < ExampleCommand.COMMAND_SIZE_DATA_SIZE)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }

                    var headerBuffer = new byte[ExampleCommand.COMMAND_SIZE_DATA_SIZE];
                    var headerBufferReadLength = stream.Read(headerBuffer, 0, headerBuffer.Length);
                    if (headerBufferReadLength != headerBuffer.Length)
                    {
                        throw new Exception(string.Format(
                            "読み込みサイズが不正です。読み込もうとしたサイズ = {0}, 読み込んだサイズ = {1}",
                            headerBuffer.Length,
                            headerBufferReadLength));
                    }

                    var commandSize = ExampleCommand.GetCommandSize(headerBuffer);
                    var bodySize = commandSize - ExampleCommand.COMMAND_SIZE_DATA_SIZE;
                    if (bodySize < 1)
                    {
                        // 内容が存在しないため、接続を終了する。
                        log.Debug("ヘッダのみのコマンドを受信しました。");
                        continue;
                    }

                    var commandBuffer = new List<byte>(commandSize);
                    commandBuffer.AddRange(headerBuffer);
                    var offset = bodySize;
                    while (true)
                    {
                        if (this.isShouldClose.Value) return;
                        if (listener.Pending()) return; // 次の接続要求が存在するため、既存の接続を終了する。

                        var bodyAvailable = client.Available;
                        var bodyDataAvailable = stream.DataAvailable;
                        log.Debug("bodyAvailable = {0}", bodyAvailable);
                        log.Debug("bodyDataAvailable = {0}", bodyDataAvailable);

                        var readSize = Math.Min(offset, readDataSize);
                        if (bodyAvailable < readSize)
                        {
                            Thread.Sleep(1000);
                            continue;
                        }

                        var buffer = new byte[readSize];
                        var readLength = stream.Read(buffer, 0, buffer.Length);
                        if (readLength != buffer.Length)
                        {
                            throw new Exception(string.Format(
                                "読み込みサイズが不正です。読み込もうとしたサイズ = {0}, 読み込んだサイズ = {1}",
                                buffer.Length,
                                readLength));
                        }

                        commandBuffer.AddRange(buffer);

                        offset -= readLength;
                        if (offset > 0)
                        {
                            Thread.Sleep(1000);
                            continue;
                        }

                        var command = new ExampleCommand(commandBuffer.ToArray());
                        log.Debug("コマンドを受信しました。コマンド = {0}", command.ToString());
                        break;
                    }
                }
            }
        }
    }
}

using CommonLib.ThreadSafe;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
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
            const string host = "localhost";
            const int port = 9000;
            const int sendDataSize = 10;

            Thread.CurrentThread.Name = "Com";
            log.Debug("通信スレッドが開始しました。");

            try
            {

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

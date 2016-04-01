using NLog;
using SocketClient.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace SocketClient
{
    static class Program
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Thread.CurrentThread.Name = "Main";
            log.Info("################################");
            log.Debug("アプリケーションを開始します。");

            Application.EnableVisualStyles();
            Application.Run(new MainForm());

            log.Debug("アプリケーションを終了します。");
        }
    }
}

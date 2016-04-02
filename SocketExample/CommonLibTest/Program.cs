using NUnit.Gui;
using System.Windows.Forms;

namespace CommonLibTest
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [System.STAThread]
        public static void Main()
        {
            AppEntry.Main(new string[] { Application.ExecutablePath });
        }
    }
}

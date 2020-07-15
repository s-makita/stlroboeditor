using System;
using System.Threading;
using System.Windows.Forms;

namespace StellarRoboProcess
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            //変数宣言
            string mutexName = "StellarRoboProcess";
            bool createdNew = false;

            //Mutexを取得する
            Mutex mutex = new Mutex(true, mutexName, out createdNew);

            //Mutexは作成済みか？
            if (createdNew)
            {
                try
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new FormMain());
                }
                catch
                {

                }
                finally
                {
                    //Mutexを開放する
                    mutex.ReleaseMutex();
                    mutex.Close();

                }
            }
            else
            {
                //既に起動している
                //マウスの制御を奪っているので、あえてメッセージ等は出力しない
                mutex.Close();
            }
        }
    }
}

using System;
using System.Net.Sockets;
using System.Text;
using System.Timers;

namespace CustomFTP
{
    class Tcp_Listener
    {
        int portForListen;
        byte[] cryptKey ;
        string userName;//используем имя юзера, которое получили на этапе 
        Timer portLeaveTimer;// не дает подвиснуть открытому порту, если нет входящих подключений( клиент завис)
        TcpListener tcpListener;
        public Tcp_Listener(int port, string key, string userlogin)
        {
            portForListen = port;
            cryptKey = Encoding.ASCII.GetBytes(key);
            userName = userlogin;
            CreateTimer();
        }


        public void Listen()
        {
            UniversalIO.Print($"  Start listen {portForListen} port for arriving {userName} files...");
            tcpListener = new TcpListener(portForListen);
            tcpListener.Start();
            portLeaveTimer.Start();
            try
            {
                using (TcpClient client = tcpListener.AcceptTcpClient())
                {
                    portLeaveTimer.Stop();
                    using (NetworkStream stream = client.GetStream())
                    {
                        FileFuncs.CreateFile(SymmetryCrypt.Decrypt(stream, cryptKey), userName);//расшифровываем поток и передаем на сборку
                    }
                }
            }
            catch (Exception ex)
            {
                UniversalIO.Print(ex.Message);
                FileFuncs.Log($"Dataport not started: {ex.Message}");
            }
            finally
            {
                tcpListener.Stop();
                portLeaveTimer.Dispose();
            }
        }

        private void CreateTimer()
        {
            portLeaveTimer = new Timer();
            portLeaveTimer.Interval = 30_000;
            portLeaveTimer.Elapsed += new ElapsedEventHandler((s,e) =>
             {
                 tcpListener.Stop();
                 UniversalIO.Print(" Время ожидания ответа от клиента закончилось. Порт данных будет закрыт");
                 FileFuncs.Log(" Время ожидания ответа от клиента закончилось. Порт данных будет закрыт");
             });
        }
    }
}

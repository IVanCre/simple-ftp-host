using System;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Authentication;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Net.NetworkInformation;

namespace CustomFTP
{
    class SslTcpServer
    {
         X509Certificate serverCertificate = null;


        //реализация синглтона. Закрытый конструктор, с возможностью первичной инициализации через статический метод(и выдачей статич. объекта)
        private static SslTcpServer instance;
        private SslTcpServer() { }
        public static SslTcpServer GetInstance()
        {
            if (instance == null)
                instance = new SslTcpServer();
            return instance;
        }


        public  async void RunServer(X509Certificate certificate, int port)
        {
            serverCertificate = certificate;
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();

            bool showFullCertInfo = false;//CertFuncs.ShowCertInfo();
            while (true)
            {
                UniversalIO.Print("\nWaiting for a new client to connect...");
                var connecttcp = await listener.AcceptTcpClientAsync();
                ProcessClient(connecttcp, showFullCertInfo);
            }
        }

        void ProcessClient(TcpClient client, bool infoFlag)
        {
            bool userDataValid = false;//проверка успешности всего цикла tcp
            DataForSession dataStruct = new DataForSession();
            SslStream sslStream = new SslStream(client.GetStream(), false);
            int freeDataPort = 0;

            FileFuncs.Log($"  ->   INPUT CONNECTION from address {client.Client.RemoteEndPoint}");
            try
            {
                sslStream.AuthenticateAsServer(serverCertificate, clientCertificateRequired: false, checkCertificateRevocation: true);
                if (infoFlag)
                    CertFuncs.AllInfo(sslStream);


                dataStruct = JsonSerializer.Deserialize<DataForSession>(CertFuncs.GetDataFromSSl(sslStream));
                FileFuncs.Log($"User data from {client.Client.RemoteEndPoint} :\r\n" +
                              $"     login      ={dataStruct.login}\r\n");
                UniversalIO.Print($"  Client {dataStruct.login} connect to server...");



                if (CheckUserToValid(dataStruct))
                {
                    freeDataPort = SelectFreePort();
                    byte[] message = Encoding.UTF8.GetBytes($"Server listen_{freeDataPort}" + "<EOF>");//передаем клиенту, какой порт слушает сервер, для получения данных
                    sslStream.Write(message);
                    sslStream.Flush();
                    userDataValid = true;
                }
                else
                {
                    throw new AuthenticationException("Not valid login or pasw!");
                }
            }
            catch (Exception e)
            {
                if (e.GetType() == typeof(AuthenticationException))//любая проблема во время идентификации приведет сюда
                { 
                    FileFuncs.Log($"Authentication failed from address {client.Client.RemoteEndPoint}. {e.Message}");
                    sslStream.Write(Encoding.UTF8.GetBytes("Authentication failed! Connection will closing...<EOF>"));
                }
                else
                {
                    FileFuncs.Log($"Inner Exception {client.Client.RemoteEndPoint}\n {e.Message}");
                    sslStream.Write(Encoding.UTF8.GetBytes("Exception on server! Connection will closing...< EOF>"));
                }
                UniversalIO.Print(e.Message);
            }
            finally
            {
                sslStream.Close();
                if (userDataValid == true)
                {
                    Task.Factory.StartNew(() =>
                    {
                        Tcp_Listener datalistener = new Tcp_Listener(freeDataPort, dataStruct.cryptKey, dataStruct.login);
                        datalistener.Listen();
                    });
                }
            }
        }


        private static bool CheckUserToValid(DataForSession data)//поиск данных клиента  во внутреннем хранилище(ну типа бд)
        {
            try
            {
                string pasw = UserDataInDB.SearchUserInDB(data.login, data.pasw);
                if (data.pasw != pasw)
                    return false;
                else
                    return true;
            }
            catch(Exception e)
            {
                throw new AuthenticationException(e.Message);
            }
        }
        private static int SelectFreePort()
        {
            IPEndPoint[] endPoints;
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            endPoints = properties.GetActiveTcpListeners();
            bool freePortFlag;
            int freePort = 65534;//запасной порт(если все порты уже заняты)
            
            for (int i=60_000; i<61000;i++)
            {
                freePortFlag = true;
                foreach (IPEndPoint endpoint  in endPoints)
                {
                    if (endpoint.Port== i)
                        freePortFlag = false;//данный порт уже занят
                }
                if (freePortFlag)
                {
                    freePort = i;
                    break;
                }
            }
            return freePort;
        }
    }
}


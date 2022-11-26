using System;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Authentication;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography.X509Certificates;


namespace SslTcp
{
    class SslTcpServer
    {
        static X509Certificate serverCertificate = null;
        // The certificate parameter specifies the name of the file
        // containing the machine certificate.

        public static async void RunServer(X509Certificate certificate, int port)
        {
            serverCertificate = certificate;
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();

            bool flag = ShowCertInfo();
            while (true)
            {
                Console.WriteLine("\nWaiting for a client to connect...");
                var connecttcp = await listener.AcceptTcpClientAsync();
                ProcessClient(connecttcp, flag);
            }
            
        }
        static void ProcessClient(TcpClient client, bool infoFlag)
        {
            SslStream sslStream = new SslStream(client.GetStream(), false);// Authenticate the server but don't require the client to authenticate.           
            try
            {
                sslStream.AuthenticateAsServer(serverCertificate, clientCertificateRequired: false, checkCertificateRevocation: true);

                if(infoFlag)
                 AllInfo(sslStream);

                //sslStream.ReadTimeout = 5000;//для ручного ввода можно отключить
                //sslStream.WriteTimeout = 5000;

                
                Console.WriteLine("Waiting for client message...");
                SessionData dataStruct = JsonSerializer.Deserialize<SessionData>(Funcs.GetDataFromSSl(sslStream));
Console.WriteLine($" login={dataStruct.login}\n" +
                  $" pasw={dataStruct.pasw}\n" +
                  $" key={dataStruct.cryptKey}");



                byte[] message = Encoding.UTF8.GetBytes("Settings accepted"+"<EOF>");
                sslStream.Write(message);
                sslStream.Flush();

                //тут надо запускать прием шифрованных данных по udp
            }
            catch (AuthenticationException e)
            {
                Console.WriteLine($"Exception: {e.Message}" );
                if (e.InnerException != null)
                {
                    Console.WriteLine("Inner exception: {0}", e.InnerException.Message);
                }
                Console.WriteLine("Authentication failed - closing the connection.");
                sslStream.Close();
                client.Close();
                return;
            }
            catch(SocketException e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                sslStream.Close();
                client.Close();
            }
        }



        static private void AllInfo(SslStream sslStream)
        {
            DisplaySecurityLevel(sslStream);
            DisplaySecurityServices(sslStream);
            DisplayCertificateInformation(sslStream);
            DisplayStreamProperties(sslStream);
        }
        static void DisplaySecurityLevel(SslStream stream)
        {
            Console.WriteLine("Cipher: {0} strength {1}", stream.CipherAlgorithm, stream.CipherStrength);
            Console.WriteLine("Hash: {0} strength {1}", stream.HashAlgorithm, stream.HashStrength);
            Console.WriteLine("Key exchange: {0} strength {1}", stream.KeyExchangeAlgorithm, stream.KeyExchangeStrength);
            Console.WriteLine("Protocol: {0}", stream.SslProtocol);
        }
        static void DisplaySecurityServices(SslStream stream)
        {
            Console.WriteLine("Is authenticated: {0} as server? {1}", stream.IsAuthenticated, stream.IsServer);
            Console.WriteLine("IsSigned: {0}", stream.IsSigned);
            Console.WriteLine("Is Encrypted: {0}", stream.IsEncrypted);
        }
        static void DisplayStreamProperties(SslStream stream)
        {
            Console.WriteLine("Can read: {0}, write {1}", stream.CanRead, stream.CanWrite);
            Console.WriteLine("Can timeout: {0}", stream.CanTimeout);
        }
        static void DisplayCertificateInformation(SslStream stream)
        {
            Console.WriteLine("Certificate revocation list checked: {0}", stream.CheckCertRevocationStatus);

            X509Certificate localCertificate = stream.LocalCertificate;
            if (stream.LocalCertificate != null)
            {
                Console.WriteLine("Local cert was issued to {0} and is valid from {1} until {2}.",
                    localCertificate.Subject,
                    localCertificate.GetEffectiveDateString(),
                    localCertificate.GetExpirationDateString());
            }
            else
            {
                Console.WriteLine("Local certificate is null.");
            }
            // Display the properties of the client's certificate.
            X509Certificate remoteCertificate = stream.RemoteCertificate;
            if (stream.RemoteCertificate != null)
            {
                Console.WriteLine("Remote cert was issued to {0} and is valid from {1} until {2}.",
                    remoteCertificate.Subject,
                    remoteCertificate.GetEffectiveDateString(),
                    remoteCertificate.GetExpirationDateString());
            }
            else
            {
                Console.WriteLine("Remote certificate is null.");
            }
        }



        private static bool ShowCertInfo()
        {
            Console.WriteLine("Show certificate info?  Y/N ");
            string r = Console.ReadLine();
            if (r == "Y" || r == "y")
                return true;

            return false;
        }
    }
}


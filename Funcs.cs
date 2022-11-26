using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;



namespace SslTcp
{
    class Funcs
    {
        public static X509Certificate GetCurrentUserCertificates()
        {
            try
            {
                List<X509Certificate> certificates = new List<X509Certificate>();
                X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);//тянется только из хранилища текущего юзера!
                store.Open(OpenFlags.OpenExistingOnly);
                foreach (X509Certificate cert in store.Certificates)
                {
                    certificates.Add(cert);
                }
                return certificates[0];
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }
        }
        public static void ShowInfo(X509Certificate cert)
        {
            Console.WriteLine($" hostName: {cert.GetName()}\n" +
                              $"centrName: {cert.GetIssuerName()}\n");//если самоподписной - одинаковые
        }

        public static string GetDataFromSSl(SslStream sslStream)
        {
            try
            {
                byte[] buffer = new byte[2048];
                StringBuilder messageData = new StringBuilder();
                int bytes = -1;
                do
                {
                    bytes = sslStream.Read(buffer, 0, buffer.Length);

                    Decoder decoder = Encoding.UTF8.GetDecoder();
                    char[] chars = new char[decoder.GetCharCount(buffer, 0, bytes)];
                    decoder.GetChars(buffer, 0, bytes, chars, 0);
                    messageData.Append(chars);

                    if (messageData.ToString().IndexOf("<EOF>") != -1)
                    {
                        break;
                    }
                } while (bytes != 0);
                messageData.Remove(messageData.Length - 5, 5);//вырезаем <eof>


                return messageData.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return "Exception_!";
            }
        }
    }



    public struct SessionData
    {
        public string login { get; set; }
        public string pasw { get; set; }
        public string cryptKey { get; set; }

        public  void Set()
        {
            Console.WriteLine("login:");
            login = Console.ReadLine();
            Console.WriteLine("pasw:");
            pasw = Console.ReadLine();
            Console.WriteLine("key:");
            cryptKey = Console.ReadLine();
        }
    }

}

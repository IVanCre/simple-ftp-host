using System;
using System.Security.Cryptography.X509Certificates;




namespace CustomFTP
{
    class Program
    {
        static void Main(string[] args)
        {
            int sslPortServer = 835;
            int startVariant;
            bool flagStartServer = false;

            while (flagStartServer==false)
            {
                UniversalIO.Print("Выберите вариант работы:\n" +
                    " 0 - Запуск сервера\n" +
                    " 1 - Добавление нового пользователя\n" +
                    " 2 - Удаление пользователя \n" +
                    " 3 - Просмотр всех пользователей");
                startVariant = Convert.ToInt32(UniversalIO.EnterMessage());


                switch (startVariant)
                {
                    case 0:
                        {
                            UniversalIO.Print("\n Выбран сервер:");
                            X509Certificate certificate = CertFuncs.GetCurrentUserCertificates();



                            if (certificate != null)//если нет сертификата - работа бессмысленна
                            {
                                CertFuncs.ShowInfo(certificate);

                                FileFuncs.CreateUsersMainFolder();
                                flagStartServer = true;//работать теперь будет только как сервак

                                SslTcpServer server = SslTcpServer.GetInstance();
                                server.RunServer(certificate, sslPortServer);
                            }
                            else
                            {
                                UniversalIO.Print(" Сертификат не найден. Сервер будет закрыт... ");
                            }
                            break;
                        }
                    case 1:
                        {
                            UniversalIO.Print("\n Выбрано 'Добавление пользователя':");
                            UniversalIO.Print("Введите имя нового Пользователя:");
                            string login = UniversalIO.EnterMessage();
                            UniversalIO.Print("Введите пароль нового Пользователя");
                            string pasw = UniversalIO.EnterMessage();
                            UserDataInDB.AddNewUser(login, pasw);
                            break;
                        }
                    case 2:
                        {
                            UniversalIO.Print("\n Выбрано 'Удаление пользователя':");
                            UniversalIO.Print("Введите имя удаляемого Пользователя:");
                            string login = UniversalIO.EnterMessage();
                            UserDataInDB.DeleteUser(login);
                            break;
                        }
                    case 3:
                        {
                            UniversalIO.Print("\n Выбрано 'Просмотр пользователей':");
                            UserDataInDB.PrintAllUsersData();
                            break;
                        }
                }
                UniversalIO.Print("\n");
            }
            UniversalIO.EnterMessage(); //удерживаем основной поток от закрытия(ввиду того что прослушка идет как асинхронная и возврат в основной поток

        }
    }
}


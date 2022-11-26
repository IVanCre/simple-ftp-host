using System;
using System.IO;
using System.Reflection;

namespace CustomFTP
{
    class FileFuncs
    {
        private static readonly object syncObject = new object();

        public static void Log(string message)
        {
            string logFileName = AbsolutFilePath(@"LogFiles\log.txt");

            lock (syncObject)
            {
                if(File.GetLastWriteTime(logFileName).Date < DateTime.Now.Date)
                {
                    File.Create(logFileName);//типа перезаписи, хранение данных только за последние сутки
                }
                string s = "\r\n" + DateTime.Now.ToString() + "   " + message;
                File.AppendAllText(logFileName, s);
            }
        }

        public static string AbsolutFilePath(string fileName)//возвращает абсолютный путь к файлу, неважно откуда идет вызов
        {
            string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return Path.Combine(directory, fileName);
        }

        public static void CreateFile(Stream ms, string login)
        {
            try
            {
                if (Directory.Exists($"users\\{ login}")) //если вдруг папка юзера была удалена в процессе работы
                    Directory.CreateDirectory($"users\\{login}");

                string fullPathToSave = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + $@"\users\\{login}\Compressed.zip";


                string clearDateTime = ClearDateString(DateTime.Now.ToString());
                string finalFilename = fullPathToSave.Insert(fullPathToSave.Length - 4, clearDateTime);//формируем имя файла вместе с понятной датой

                using (var fileStream = File.Create(finalFilename))
                {
                    ms.CopyTo(fileStream);
                }

                Log($"New archive uploaded");
                UniversalIO.Print($" New archive uploaded !\n");
            }
            catch (Exception e)
            {
                UniversalIO.Print("Ошибка создания архива из входящего потока");
                Log($"Error create archive \n {e.Message}");

            }
        }

        private static string ClearDateString(string input)
        {
            string incorrectSymbols= @"/ :.";
            for(int c=0; c< input.Length;c++)
            {
                if(incorrectSymbols.IndexOf(input[c])>-1)
                {
                    input = input.Replace(input[c],'_');
                }
            }
            return "_"+input;
        }
        public static void CreateUsersMainFolder()
        {
            Directory.CreateDirectory("users");
        }
    }

    
}

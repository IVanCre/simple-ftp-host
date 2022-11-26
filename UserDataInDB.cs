using System;
using System.IO;
using System.Collections.Generic;

namespace CustomFTP
{
    class UserDataInDB
    {
        static string usersDB = FileFuncs.AbsolutFilePath("servDB.bin");//заменить все это на реальную БД, а не позориться!


        public static string SearchUserInDB(string login, string pasw)
        {
            if (File.Exists(usersDB))
            {
                try
                {
                    List<string> allUsersData= new List<string>();
                    using (BinaryReader reader = new BinaryReader(File.Open(usersDB, FileMode.Open)))
                    {
                        while (reader.BaseStream.Position != reader.BaseStream.Length)
                        {
                            allUsersData.Add(reader.ReadString());
                        }
                    }

                    foreach (string s in allUsersData)
                    {
                        string[] words = s.Split(new char[] { '|' });
                        if (words[0] == login)
                            return words[1];//есть совпадение по логину, вернем пароль от этого логина
                    }
                    return null;

                }
                catch (Exception e)
                {
                    UniversalIO.Print("Ошибка чтения данных во внутренней БД");
                    FileFuncs.Log(e.Message);
                    return null;
                }
            }
            else
            {
                UniversalIO.Print("DB with user's data not found!");
                FileFuncs.Log("File 'userDB.bin' not found!");
                return null;
            }
        }


        public static void AddNewUser(string login, string pasw)
        {
            try
            {
                if (File.Exists(usersDB))
                {
                    Directory.CreateDirectory("users/" + login);

                    using (BinaryWriter writer = new BinaryWriter(File.Open(usersDB, FileMode.Open)))
                    {
                        writer.Seek(0,SeekOrigin.End);
                        writer.Write(login + "|" + pasw + "\n");
                        UniversalIO.Print($"Пользователь {login} успешно добавлен");
                    }
                }
                else
                {
                    UniversalIO.Print("Внутреняя БД не найдена\n");
                    FileFuncs.Log("Cant add new user - DB not found!\n");
                }
            }
            catch (Exception ex)
            {
                UniversalIO.Print("Ошибка добавления нового пользователя!\n");
                FileFuncs.Log($"Cant save new user!\n {ex}");
            }
        }
        public static void DeleteUser(string login)
        {
            try
            {
                if (File.Exists(usersDB))
                {
                    bool success = false;
                    List<string> allUsersData = new List<string>();
                    using (BinaryReader reader = new BinaryReader(File.Open(usersDB, FileMode.Open)))
                    {
                        while (reader.BaseStream.Position != reader.BaseStream.Length)
                        {
                            allUsersData.Add(reader.ReadString());
                        }
                    }

                    for (int i = 0; i < allUsersData.Count; i++)
                    {
                        if (allUsersData[i].Contains(login))
                        {
                            allUsersData.RemoveAt(i);
                            success = true;
                            break;
                        }
                    }
                    if (success)
                    {
                        using (BinaryWriter writer = new BinaryWriter(File.Open(usersDB, FileMode.Create)))
                        {
                            foreach (string s in allUsersData)
                            {
                                writer.Write(s);
                            }
                        }
                        UniversalIO.Print($"Пользователь {login} успешно удален");
                    }
                    else
                    {
                        UniversalIO.Print($"Пользователь {login} не найден в БД");
                    }
                }
                else
                {
                    UniversalIO.Print("Файл БД не найден");
                    FileFuncs.Log("Cant delete user - DB not found!\n");
                }
            }
            catch(Exception e)
            {
                UniversalIO.Print($"Ошибка удаления пользователя !");
                FileFuncs.Log($"Can't delete user -{e.Message}");
            }
        }
        public static void PrintAllUsersData()
        {
            if(File.Exists(usersDB))
            {
                try
                {
                    using (BinaryReader reader = new BinaryReader(File.Open(usersDB, FileMode.Open)))
                    {
                        while (reader.BaseStream.Position != reader.BaseStream.Length)
                        {
                            UniversalIO.Print(reader.ReadString());
                        }
                    }
                }
                catch(Exception e)
                {
                    UniversalIO.Print("Ошибка чтения файла БД");
                    FileFuncs.Log(e.Message);
                }
            }
            else
            {
                UniversalIO.Print("Файл БД не найден");
            }
        }
    }
}

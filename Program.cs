using System;
using System.Collections;
using System.IO;
using Spectre.Console;
using Microsoft.Data.Sqlite;

namespace ToDoList
{
    class Handler
    {
        private int _idCounter;
        public delegate void DescriptionTask();
        public delegate void RecordTask(string task);
        
        public DescriptionTask ExecutorShowTasks;
        public RecordTask ExecutorRecordTask;

        public void ShowTasks()
        {
            Console.WriteLine("id\tdescription task\tstatus");
            ExecutorShowTasks();
        }

        public void SetTask(string task)
        {
            ExecutorRecordTask(task);
        }

        public void TakeTask()
        {
            _idCounter++;
            
        }
    }
    
    class Database
    {
        private string _fileName = "todolist.txt";
        
        public void Initial()
        {
            Boolean fileExistsStatus = File.Exists(_fileName);
            if (!fileExistsStatus)
            {
                File.Create(_fileName);
            }
        }

        public void Write(string data)
        {
            using (StreamWriter writer = new StreamWriter(_fileName, true))
            {
                writer.WriteLine(data);
            }
        }

        public void Read()
        {
            using (StreamReader sr = new StreamReader(_fileName))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    Console.WriteLine(line);
                }
            }
        }
    }
    class Menu
    {
        public Dictionary<string, char> CommandKey = new Dictionary<string, char>
        {
            {"exit", 'q'},
            {"view", 'v'},
            {"write", 'w'},
            {"completed", 'c'},
        };
        public void Loading()
        {
            Console.WriteLine("*** TODO List lazy ***");
        }

        public void AvailabelOptions()
        {
            Console.WriteLine("Select an action:\n" +
                                $"'{CommandKey["exit"]}' - exit\n" +
                                $"'{CommandKey["view"]}' - view to-do list\n" +
                                $"'{CommandKey["write"]}' - write task\n" +
                                $"'{CommandKey["completed"]}' - completed task");
        }
    }
    
    internal class Program
    {
        static void Main(string[] args)
        {
            AnsiConsole.Markup("[underline red]Hello[/] World!");
            Boolean isWorks = true;
            Database database = new Database();
            Menu menu = new Menu();
            Handler handler = new Handler();
            handler.ExecutorShowTasks = database.Read;
            handler.ExecutorRecordTask = database.Write;
            database.Initial();
            menu.Loading();
            
            while (isWorks)
            {   
                menu.AvailabelOptions();
                ConsoleKeyInfo answerKey = Console.ReadKey(intercept: true);
                
                if (Convert.ToChar(answerKey.KeyChar) == menu.CommandKey["exit"])
                {
                    Console.WriteLine("Exit Branch!");
                    isWorks = false;
                }
                else if (Convert.ToChar(answerKey.KeyChar) == menu.CommandKey["view"])
                {
                    handler.ShowTasks();
                }
                else if (Convert.ToChar(answerKey.KeyChar) == menu.CommandKey["write"])
                {
                    handler.TakeTask();
                    handler.SetTask("buy tea");
                }
                else if (Convert.ToChar(answerKey.KeyChar) == menu.CommandKey["completed"])
                {
                    Console.WriteLine("Completed Branch!");
                }
            }
        }
    }
}
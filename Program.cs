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
        private string _connectString = "Data Source=todolazy.db";

        public void Initial()
        {
            using (SqliteConnection connection = new SqliteConnection(_connectString))
            {
                try
                {
                    connection.Open();
                    SqliteCommand command = connection.CreateCommand();
                    command.CommandText = @"CREATE TABLE IF NOT EXISTS TodoListLazy (
                                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                            Task TEXT NOT NULL,
                                            Status INTEGER NOT NULL)";
                    command.ExecuteNonQuery();
                }
                catch (Exception exception)
                {
                    // TODO: Handler exception
                    Console.WriteLine(exception);
                }
            }
        }
        
        public void Write(String data, Boolean status)
        {
            using (SqliteConnection connection = new SqliteConnection(_connectString))
            {
                try
                {

                    connection.Open();
                    SqliteCommand command = connection.CreateCommand();
                    command.CommandText = @"INSERT INTO TodoListLazy (Task, Status) VALUES ($Task, $Status)";
                    command.Parameters.AddWithValue("$Task", data);
                    command.Parameters.AddWithValue("$Status", Convert.ToByte(status));
                    command.ExecuteNonQuery();
                }
                catch (Exception exception)
                {
                    // TODO: Handler exception
                    Console.WriteLine(exception);
                }
            }
        }
        //
        // public void Read()
        // {
        //     using (StreamReader sr = new StreamReader(_fileName))
        //     {
        //         string line;
        //         while ((line = sr.ReadLine()) != null)
        //         {
        //             Console.WriteLine(line);
        //         }
        //     }
        // }
    }
    class Menu
    {
        public Dictionary<string, char> CommandKey = new Dictionary<string, char>
        {
            {"exit", 'q'},
            {"view", 'v'},
            {"write", 'w'},
            {"update", 'u'},
            {"delete", 'd'},
            {"completed", 'c'},
            {"yes", 'y'},
            {"no", 'n'},
        };
        
        private Dictionary<string, string> _colors = new Dictionary<string, string>
        {
            {"accent", "deeppink1_1"},
            {"minor", "lightskyblue3_1"},
            {"dialog", "navajowhite1"},
        };

        private Dictionary<string, int> _sizes = new Dictionary<string, int>
        {
            { "contentWidth", Console.WindowWidth - 6 },
            {"pixelWidth", 2},
            {"imageMaxWidth", 16}
        };
        public void Intro()
        {
            Table mainContent = new Table();
            String path = Path.Combine(AppContext.BaseDirectory, "assets", "lazy.png");
            CanvasImage image = new CanvasImage(path);
            Table imageBox = new Table();
            
            mainContent.Border = TableBorder.Square;
            mainContent.BorderStyle = Style.Parse(_colors["minor"]);
            mainContent.Width(_sizes["contentWidth"]);
            mainContent.AddColumn($"[bold {_colors["accent"]}] ✦✦ TODO LIST LAZY ✦✦ [/]").Centered();
            mainContent.Columns[0].Centered();
            mainContent.Columns[0].NoWrap();
            mainContent.AddRow("");
            mainContent.AddRow($"[{_colors["minor"]}] Housekeeping assistant [/]"); 
            image.PixelWidth(_sizes["pixelWidth"]);
            image.MaxWidth(_sizes["imageMaxWidth"]);
            imageBox.Border = TableBorder.None;
            image.BicubicResampler();
            imageBox.AddColumn("").Centered();
            mainContent.AddRow(imageBox.AddRow(image));
            mainContent.AddRow($"[{_colors["dialog"]}] Want to get started?[/] " +
                               $"[{_colors["dialog"]}]([/]" +
                               $"[{_colors["accent"]}]{Convert.ToString(CommandKey["no"]).ToUpper()}[/]" +
                               $"[{_colors["dialog"]}])[/]" +
                               $"[{_colors["dialog"]}]/[/]" +
                               $"[{_colors["dialog"]}]([/]" +
                               $"[{_colors["accent"]}]{Convert.ToString(CommandKey["yes"]).ToUpper()}[/]" +
                               $"[{_colors["dialog"]}])[/]");
            AnsiConsole.Write(mainContent);
        }

        public void Outro()
        {
            Console.Clear();
            AnsiConsole.Markup($"[{_colors["accent"]}]Good Bay ♥[/]\n");
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
            ConsoleKeyInfo answerKey;
            Boolean started = true;
            Boolean isWork = false;
            Database database = new Database();
            Menu menu = new Menu();
            Handler handler = new Handler();
            database.Initial();
            // handler.ExecutorShowTasks = database.Read;
            // handler.ExecutorRecordTask = database.Write;
            menu.Intro();
            
            while (started)
            {
                answerKey = Console.ReadKey(intercept: true);
                if (Convert.ToChar(answerKey.KeyChar) == menu.CommandKey["no"])
                {
                    started = false;
                    menu.Outro();
                    
                } 
                else if (Convert.ToChar(answerKey.KeyChar) == menu.CommandKey["yes"])
                {
                    isWork = true;
                    started = !isWork;
                }
            }
            
            while (isWork)
            {
                // TODO: Work screen for tasks
                // menu.AvailabelOptions();
                answerKey = Console.ReadKey(intercept: true);
                
                if (Convert.ToChar(answerKey.KeyChar) == menu.CommandKey["exit"])
                {
                    isWork = false;
                    menu.Outro();
                }
                else if (Convert.ToChar(answerKey.KeyChar) == menu.CommandKey["view"])
                {
                    handler.ShowTasks();
                }
                else if (Convert.ToChar(answerKey.KeyChar) == menu.CommandKey["write"])
                {
                    // handler.TakeTask();
                    // handler.SetTask("buy tea");
                    database.Write("buy tea", false);
                }
                else if (Convert.ToChar(answerKey.KeyChar) == menu.CommandKey["completed"])
                {
                    Console.WriteLine("Completed Branch!");
                }
            }
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Spectre.Console;
using Microsoft.Data.Sqlite;
using System.Text;

namespace ToDoList
{
    public static class Settings
    {
        private static string[] _taskColumnNames = new[] {"ID", "Task", "Status"};
        
        private static Dictionary<string, int> _sizes = new Dictionary<string, int>
        {
            {"contentWidth", Console.WindowWidth - 6},
            {"pixelWidth", 2},
            {"imageMaxWidth", 16},
        };
        
        private static Dictionary<string, string> _colors = new Dictionary<string, string>
        {
            {"accent", "deeppink1_1"},
            {"minor", "lightskyblue3_1"},
            {"dialog", "navajowhite1"},
            {"backlight", "turquoise2"},
            {"successful", "seagreen3"},
            {"failure", "deeppink3"},
        };
        
        private static Dictionary<string, char> _commandKey = new Dictionary<string, char>
        {
            {"write", 'w'},
            {"view", 'v'},
            {"update", 'u'},
            {"completed", 'c'},
            {"delete", 'd'},
            {"quit", 'q'},
            {"yes", 'y'},
            {"no", 'n'},
        };

        private static Dictionary<string, string> _taskStatusDescription = new Dictionary<string, string>
        {
            {"positive", "ready"},
            {"negative", "not ready"},
        }; 

        public static string[] GetColumnNames()
        {
            return _taskColumnNames;
        }

        public static Dictionary<string, int> GetSizes()
        {
            return _sizes;
        }

        public static Dictionary<string, string> GetColors()
        {
            return _colors;
        }

        public static Dictionary<string, char> GetCommandKey()
        {
            return _commandKey;
        }

        public static Dictionary<string, string> GetTaskStatusDescription()
        {
            return _taskStatusDescription;
        }
    }
    
    class Handler
    {
        public delegate List<Dictionary<string, string>> AmbassadorReader();
        public AmbassadorReader TaskReader;
        private string[] _taskColumNames = Settings.GetColumnNames();
        private Dictionary<string, string> _taskStatusDescription = Settings.GetTaskStatusDescription();

        public List<Dictionary<string, string>> ReadData()
        {
            List<Dictionary<string, string>> tasks = TaskReader();
            tasks.ForEach(dict =>
            {
                foreach (var item in dict)
                {
                    if (item.Key == _taskColumNames[2].ToLower())
                    {
                        dict[item.Key] = Convert.ToByte(item.Value) == 0 
                            ? $"{_taskStatusDescription["negative"]}" 
                            : $"{_taskStatusDescription["positive"]}";
                    }
                }
            });
            return tasks;
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
        
        public void Write(string data, bool status)
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
        
        public List<Dictionary<string, string>> Read()
        {
            List<Dictionary<string, string>> result = new List<Dictionary<string, string>>();
            
            using (SqliteConnection connection = new SqliteConnection(_connectString))
            {
                try
                {
                    connection.Open();
                    SqliteCommand command = connection.CreateCommand();
                    command.CommandText = @"SELECT Id, Task, Status FROM TodoListLazy";
                    SqliteDataReader reader = command.ExecuteReader();
                    
                    while (reader.Read())
                    {
                        Dictionary<string, string> currentTask = new Dictionary<string, string>
                        {
                            {"id", reader.GetString(0)},
                            {"task",  reader.GetString(1)},
                            {"status", reader.GetString(2)}
                        };
                        result.Add(currentTask);
                    }

                    return result;
                }
                catch (Exception exception)
                {
                    // TODO: Handler exception
                    Console.WriteLine(exception);
                }
            }
            
            return result;
        }
    }
    class Menu
    {
        public Dictionary<string, char> CommandKey = Settings.GetCommandKey();
        private Dictionary<string, string> _colors = Settings.GetColors();
        private Dictionary<string, int> _sizes = Settings.GetSizes();
        private string[] _taskColumnNames = Settings.GetColumnNames();
        private Dictionary<string, string> _taskStatusDescription = Settings.GetTaskStatusDescription();

        public void Intro()
        {
            Table mainContent = new Table();
            string path = Path.Combine(AppContext.BaseDirectory, "assets", "lazy.png");
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

        public void Notepad(List<Dictionary<string, string>> suiteTask)
        {
            Console.Clear();
            Table mainContent = new Table();
            Table options = new Table();
            Table tasks = new Table();
            StringBuilder optionString = new StringBuilder();
            int nextLineCounter = 0;
            
            mainContent.Border = TableBorder.Square;
            mainContent.BorderStyle = Style.Parse(_colors["minor"]);
            mainContent.Width(_sizes["contentWidth"]);
            mainContent.AddColumn($"[bold {_colors["accent"]}] ✦✦ MENU ✦✦ [/]").Centered();
            mainContent.Columns[0].Centered();
            mainContent.Columns[0].NoWrap();
            options.Border = TableBorder.None;
            options.Width(_sizes["contentWidth"]);
            
            foreach (KeyValuePair<string, char> item in CommandKey)
            {
                if (item.Value != CommandKey["yes"] && item.Value != CommandKey["no"])
                {
                    string end = " ";
                    if (nextLineCounter == 2)
                    {
                        end = "\n";
                    }
                    optionString.AppendFormat($"[{_colors["dialog"]}]([/]" +
                                              $"[{_colors["accent"]}]{item.Value}[/]" +
                                              $"[{_colors["dialog"]}])[/]" +
                                              $"[{_colors["dialog"]}]-{item.Key}[/]" +
                                              $"{end}");
                    ++nextLineCounter;
                }
            }
            
            options.AddColumn(Convert.ToString(optionString));
            options.Columns[0].Centered();
            
            if (Console.WindowWidth < 50)
            {
                options.Columns[0].LeftAligned();
            }
            mainContent.AddRow(options);
            
            tasks.Width(_sizes["contentWidth"]);
            
            Array.ForEach(
                _taskColumnNames, name => tasks.AddColumn($"[{_colors["accent"]}]{name}[/]", 
                    column => column.Centered())
            );
            
            tasks.Columns[1].Width(_sizes["contentWidth"] / 2);
            tasks.Caption($"[{_colors["minor"]}]♥ TODO list Lazy ♥[/]");
            tasks.BorderStyle = Style.Parse(_colors["minor"]);
            
            suiteTask.ForEach(dict =>
            {
                List<Table> tablesDescription = new List<Table>();
                foreach (var item in dict)
                {
                    Table tablePart = new Table();
                    tablePart.Border = TableBorder.None;
                    string taskLine = $"[{_colors["backlight"]}]{item.Value }[/]";

                    if (item.Key == _taskColumnNames[2].ToLower())
                    {
                        taskLine = item.Value == _taskStatusDescription["negative"]
                            ? $"[{_colors["failure"]}]{item.Value}[/]"
                            : $"[{_colors["successful"]}]{item.Value}[/]";
                    }
                    
                    if (item.Key == _taskColumnNames[1].ToLower())
                    {
                        tablePart.AddColumn(taskLine).LeftAligned();
                    }
                    else
                    {
                        tablePart.AddColumn(taskLine).Centered();
                    }
                    tablesDescription.Add(tablePart);
                }
                tasks.AddRow(tablesDescription);
            });

            mainContent.AddRow(tasks);
            AnsiConsole.Write(mainContent);
        }
    }
    
    internal class Program
    {
        static void Main(string[] args)
        {
            ConsoleKeyInfo answerKey;
            bool started = true;
            bool isWork = false;
            Database database = new Database();
            Menu menu = new Menu();
            Handler handler = new Handler();
            handler.TaskReader = database.Read;
            database.Initial();
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
                List<Dictionary<string, string>> suiteTask = handler.ReadData();
                menu.Notepad(suiteTask);
                answerKey = Console.ReadKey(intercept: true);
                
                if (Convert.ToChar(answerKey.KeyChar) == menu.CommandKey["quit"])
                {
                    isWork = false;
                    menu.Outro();
                }
                else if (Convert.ToChar(answerKey.KeyChar) == menu.CommandKey["view"])
                {
                    
                }
                else if (Convert.ToChar(answerKey.KeyChar) == menu.CommandKey["write"])
                {
                    database.Write("buy keyboard", false);
                }
                else if (Convert.ToChar(answerKey.KeyChar) == menu.CommandKey["completed"])
                {
                    Console.WriteLine("Completed Branch!");
                }
            }
        }
    }
}
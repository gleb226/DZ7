using System.Diagnostics;

namespace ParentApp;

internal static class Program
{
    public static int Main(string[] args)
    {
        if (args.Length > 0)
        {
            return RunNonInteractive(args);
        }

        return RunInteractive();
    }

    private static int RunInteractive()
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("Module 2 - Processes");
            Console.WriteLine("1) Task 1 - Start child and wait (show exit code)");
            Console.WriteLine("2) Task 2 - Start child; wait or kill");
            Console.WriteLine("3) Task 3 - Start child with calc args");
            Console.WriteLine("4) Task 4 - Start child with search args");
            Console.WriteLine("0) Exit");
            Console.Write("Choose: ");

            var choice = Console.ReadLine()?.Trim();
            Console.WriteLine();

            switch (choice)
            {
                case "1":
                    RunTask1();
                    break;
                case "2":
                    RunTask2Interactive();
                    break;
                case "3":
                    RunTask3Interactive();
                    break;
                case "4":
                    RunTask4Interactive();
                    break;
                case "0":
                    return 0;
                default:
                    Console.WriteLine("Unknown choice.");
                    break;
            }
        }
    }

    private static int RunNonInteractive(string[] args)
    {
        var mode = args[0].Trim().ToLowerInvariant();

        return mode switch
        {
            "task1" => RunTask1(),
            "task2" => RunTask2(args.Skip(1).ToArray()),
            "task3" => RunTask3(args.Skip(1).ToArray()),
            "task4" => RunTask4(args.Skip(1).ToArray()),
            _ => PrintUsage()
        };
    }

    private static int PrintUsage()
    {
        Console.WriteLine("ParentApp usage:");
        Console.WriteLine("  ParentApp");
        Console.WriteLine("  ParentApp task1");
        Console.WriteLine("  ParentApp task2 wait|kill");
        Console.WriteLine("  ParentApp task3 <a> <b> <op>");
        Console.WriteLine("  ParentApp task4 <filePath> <word>");
        return 1;
    }

    private static int RunTask1()
    {
        Console.WriteLine("Task 1: starting child process and waiting for it to exit...");
        using var process = StartChild("exitcode", "7", "500");
        process.WaitForExit();
        Console.WriteLine($"Child exit code: {process.ExitCode}");
        return 0;
    }

    private static void RunTask2Interactive()
    {
        Console.Write("Action (wait/kill): ");
        var action = Console.ReadLine()?.Trim() ?? "";
        RunTask2([action]);
    }

    private static int RunTask2(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Task 2 usage: task2 wait|kill");
            return 1;
        }

        var action = args[0].Trim().ToLowerInvariant();
        if (action is not ("wait" or "kill"))
        {
            Console.WriteLine("Action must be 'wait' or 'kill'.");
            return 1;
        }

        Console.WriteLine("Task 2: starting child process...");
        using var process = StartChild("exitcode", "0", "15000");

        if (action == "wait")
        {
            Console.WriteLine("Waiting for child to exit...");
            process.WaitForExit();
            Console.WriteLine($"Child exit code: {process.ExitCode}");
            return 0;
        }

        Console.WriteLine("Killing child process...");
        Thread.Sleep(1000);
        process.Kill(entireProcessTree: true);
        process.WaitForExit();
        Console.WriteLine($"Child exit code after kill: {process.ExitCode}");
        return 0;
    }

    private static void RunTask3Interactive()
    {
        Console.Write("First number: ");
        var a = Console.ReadLine()?.Trim() ?? "";
        Console.Write("Second number: ");
        var b = Console.ReadLine()?.Trim() ?? "";
        Console.Write("Operation (+ - * /): ");
        var op = Console.ReadLine()?.Trim() ?? "";
        RunTask3([a, b, op]);
    }

    private static int RunTask3(string[] args)
    {
        if (args.Length != 3)
        {
            Console.WriteLine("Task 3 usage: task3 <a> <b> <op>");
            return 1;
        }

        Console.WriteLine("Task 3: starting child process with calc arguments...");
        using var process = StartChild("calc", args[0], args[1], args[2]);
        process.WaitForExit();
        Console.WriteLine($"Child exit code: {process.ExitCode}");
        return 0;
    }

    private static void RunTask4Interactive()
    {
        Console.Write("File path: ");
        var filePath = Console.ReadLine()?.Trim() ?? "";
        Console.Write("Word: ");
        var word = Console.ReadLine()?.Trim() ?? "";
        RunTask4([filePath, word]);
    }

    private static int RunTask4(string[] args)
    {
        if (args.Length != 2)
        {
            Console.WriteLine("Task 4 usage: task4 <filePath> <word>");
            return 1;
        }

        Console.WriteLine("Task 4: starting child process with search arguments...");
        using var process = StartChild("search", args[0], args[1]);
        process.WaitForExit();
        Console.WriteLine($"Child exit code: {process.ExitCode}");
        return 0;
    }

    private static Process StartChild(params string[] childArgs)
    {
        var (fileName, arguments) = ResolveChildCommand(childArgs);

        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = false
        };

        var process = new Process { StartInfo = startInfo };
        if (!process.Start())
            throw new InvalidOperationException("Failed to start child process.");

        return process;
    }

    private static (string fileName, string arguments) ResolveChildCommand(string[] childArgs)
    {
        var baseDir = AppContext.BaseDirectory;
        var configuration = baseDir.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .FirstOrDefault(s => s.Equals("Debug", StringComparison.OrdinalIgnoreCase) || s.Equals("Release", StringComparison.OrdinalIgnoreCase))
            ?? "Debug";

        var root = FindSolutionRoot(baseDir) ?? Directory.GetCurrentDirectory();
        var tfm = "net8.0";

        var exePath = Path.Combine(root, "src", "ChildApp", "bin", configuration, tfm, "ChildApp.exe");
        if (File.Exists(exePath))
            return (exePath, JoinArgs(childArgs));

        var dllPath = Path.Combine(root, "src", "ChildApp", "bin", configuration, tfm, "ChildApp.dll");
        if (File.Exists(dllPath))
            return ("dotnet", JoinArgs([dllPath, .. childArgs]));

        throw new FileNotFoundException("ChildApp output not found. Build the solution first.");
    }

    private static string JoinArgs(IEnumerable<string> args)
    {
        return string.Join(" ", args.Select(QuoteIfNeeded));
    }

    private static string QuoteIfNeeded(string value)
    {
        if (value.Length == 0)
            return "\"\"";

        var needsQuotes = value.Any(char.IsWhiteSpace) || value.Contains('"');
        if (!needsQuotes)
            return value;

        return "\"" + value.Replace("\"", "\\\"", StringComparison.Ordinal) + "\"";
    }

    private static string? FindSolutionRoot(string startPath)
    {
        var dir = new DirectoryInfo(startPath);
        for (var i = 0; i < 10 && dir is not null; i++)
        {
            var sln = Path.Combine(dir.FullName, "ProcessesModule2.sln");
            if (File.Exists(sln))
                return dir.FullName;

            dir = dir.Parent;
        }

        return null;
    }
}

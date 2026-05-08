using System.Globalization;

namespace ChildApp;

internal static class Program
{
    public static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            PrintUsage();
            return 1;
        }

        var command = args[0].Trim().ToLowerInvariant();
        var commandArgs = args.Skip(1).ToArray();

        try
        {
            return command switch
            {
                "exitcode" => RunExitCode(commandArgs),
                "calc" => RunCalc(commandArgs),
                "search" => RunSearch(commandArgs),
                _ => UnknownCommand(command)
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 3;
        }
    }

    private static int UnknownCommand(string command)
    {
        Console.Error.WriteLine($"Unknown command: {command}");
        PrintUsage();
        return 1;
    }

    private static void PrintUsage()
    {
        Console.WriteLine("ChildApp commands:");
        Console.WriteLine("  exitcode <code> [delayMs]");
        Console.WriteLine("  calc <a> <b> <op>");
        Console.WriteLine("  search <filePath> <word>");
    }

    private static int RunExitCode(string[] args)
    {
        if (args.Length is < 1 or > 2)
        {
            Console.Error.WriteLine("Usage: exitcode <code> [delayMs]");
            return 1;
        }

        if (!int.TryParse(args[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var code))
        {
            Console.Error.WriteLine("Invalid exit code.");
            return 1;
        }

        var delayMs = 0;
        if (args.Length == 2)
        {
            if (!int.TryParse(args[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out delayMs) || delayMs < 0)
            {
                Console.Error.WriteLine("Invalid delayMs.");
                return 1;
            }
        }

        if (delayMs > 0)
            Thread.Sleep(delayMs);

        return code;
    }

    private static int RunCalc(string[] args)
    {
        if (args.Length != 3)
        {
            Console.Error.WriteLine("Usage: calc <a> <b> <op>");
            return 1;
        }

        Console.WriteLine($"Arg1: {args[0]}");
        Console.WriteLine($"Arg2: {args[1]}");
        Console.WriteLine($"Op: {args[2]}");

        if (!double.TryParse(args[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var a) ||
            !double.TryParse(args[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var b))
        {
            Console.Error.WriteLine("Invalid numbers.");
            return 1;
        }

        var op = args[2].Trim();
        var result = op switch
        {
            "+" => a + b,
            "-" => a - b,
            "*" => a * b,
            "/" => b == 0 ? throw new DivideByZeroException("Division by zero.") : a / b,
            _ => throw new ArgumentException("Unsupported operation. Use one of: + - * /")
        };

        Console.WriteLine($"Result: {result.ToString(CultureInfo.InvariantCulture)}");
        return 0;
    }

    private static int RunSearch(string[] args)
    {
        if (args.Length != 2)
        {
            Console.Error.WriteLine("Usage: search <filePath> <word>");
            return 1;
        }

        var filePath = args[0];
        var word = args[1];

        if (string.IsNullOrWhiteSpace(word))
        {
            Console.Error.WriteLine("Search word must be non-empty.");
            return 1;
        }

        if (!File.Exists(filePath))
        {
            Console.Error.WriteLine("File not found.");
            return 1;
        }

        var text = File.ReadAllText(filePath);
        var count = CountOccurrences(text, word, StringComparison.OrdinalIgnoreCase);
        Console.WriteLine($"File: {filePath}");
        Console.WriteLine($"Word: {word}");
        Console.WriteLine($"Count: {count}");
        return 0;
    }

    private static int CountOccurrences(string text, string word, StringComparison comparison)
    {
        var count = 0;
        var index = 0;

        while (true)
        {
            index = text.IndexOf(word, index, comparison);
            if (index < 0)
                return count;

            count++;
            index += word.Length;
        }
    }
}

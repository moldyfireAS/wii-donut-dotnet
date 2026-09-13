using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Media;

class Program
{
    static string ExtractEmbeddedMp3()
    {
        string fileName = "Main Theme - Homebrew Browser.mp3";
        string tempPath = Path.Combine(Path.GetTempPath(), "donut", fileName);

        Directory.CreateDirectory(Path.GetDirectoryName(tempPath)!);

        if (File.Exists(tempPath))
            return tempPath;

        var assembly = Assembly.GetExecutingAssembly();
        string resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(name => name.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));

        if (resourceName == null)
            return string.Empty;

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
            return string.Empty;

        using var fileStream = File.Create(tempPath);
        stream.CopyTo(fileStream);

        return tempPath;
    }

    static void PlayMp3(string mp3Path)
    {
        if (string.IsNullOrWhiteSpace(mp3Path) || !File.Exists(mp3Path))
            return;

        try
        {
            SoundPlayer player = new SoundPlayer(mp3Path);
            player.Load();
            player.PlayLooping();
        }
        catch
        {
            // ignore audio errors
        }
    }

    static void Main()
    {
        string mp3Path = ExtractEmbeddedMp3();
        Thread musicThread = new Thread(() => PlayMp3(mp3Path));
        musicThread.IsBackground = true;
        musicThread.Start();

        Console.OutputEncoding = Encoding.UTF8;
        Console.CursorVisible = false;

        int width = Console.WindowWidth;
        int height = Console.WindowHeight;

        if (width < 20) width = 80;
        if (height < 10) height = 22;

        float A = 0;
        float B = 0;

        float speedA = 0.08f;
        float speedB = 0.03f;

        bool paused = false;

        const string blueColor = "\x1b[38;5;39m";
        const string purpleColor = "\x1b[38;5;135m";
        const string resetColor = "\x1b[0m";

        StringBuilder frameBuffer = new StringBuilder();
        string numLuminanceChars = ".,-~:;=!*#$@";

        Console.WriteLine("Controls: [+] faster | [-] slower | [R] reset | [Space] pause");

        while (true)
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.R)
                {
                    A = 0;
                    B = 0;
                }
                else if (key == ConsoleKey.OemPlus || key == ConsoleKey.Add)
                {
                    speedA *= 1.2f;
                    speedB *= 1.2f;
                }
                else if (key == ConsoleKey.OemMinus || key == ConsoleKey.Subtract)
                {
                    speedA *= 0.8f;
                    speedB *= 0.8f;
                }
                else if (key == ConsoleKey.Spacebar)
                {
                    paused = !paused;
                }
            }

            if (!paused)
            {
                A += speedA;
                B += speedB;
            }

            if (Console.WindowWidth != width || Console.WindowHeight != height)
{
    width = Console.WindowWidth;
    height = Console.WindowHeight;
    Console.Clear();
}


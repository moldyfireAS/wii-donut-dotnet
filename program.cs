using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

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

        Process.Start(new ProcessStartInfo
        {
            FileName = mp3Path,
            UseShellExecute = true
        });
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

        const string blueColor = "\x1b[38;5;39m";
        const string purpleColor = "\x1b[38;5;135m";
        const string resetColor = "\x1b[0m";

        StringBuilder frameBuffer = new StringBuilder();

        string numLuminanceChars = ".,-~:;=!*#$@";

        while (true)
        {
            if (Console.WindowWidth != width || Console.WindowHeight != height)
            {
                width = Console.WindowWidth;
                height = Console.WindowHeight;
                Console.Clear();
            }

            char[] outputBuffer = new char[width * height];
            float[] zBuffer = new float[width * height];

            Array.Fill(outputBuffer, ' ');
            Array.Fill(zBuffer, 0f);

            float centerX = width / 2f;
            float centerY = height / 2f;
            float scaleX = width * 0.375f; 
            float scaleY = height * 0.68f; 

            for (float theta = 0; theta < 6.28f; theta += 0.07f)
            {
                for (float phi = 0; phi < 6.28f; phi += 0.02f)
                {
                    float sinTheta = MathF.Sin(theta);
                    float cosTheta = MathF.Cos(theta);
                    float sinPhi = MathF.Sin(phi);
                    float cosPhi = MathF.Cos(phi);

                    float sinA = MathF.Sin(A);
                    float cosA = MathF.Cos(A);
                    float sinB = MathF.Sin(B);
                    float cosB = MathF.Cos(B);

                    float circleX = cosTheta + 2; 
                    float circleY = sinTheta;

                    float oneOverZ = 1 / (sinPhi * circleX * sinA + circleY * cosA + 5);
                    float t = sinPhi * circleX * cosA - circleY * sinA;

                    int x = (int)(centerX + scaleX * oneOverZ * (cosPhi * circleX * cosB - t * sinB));
                    int y = (int)(centerY + scaleY * oneOverZ * (cosPhi * circleX * sinB + t * cosB));

                    int bufferIndex = x + width * y;

                    float luminance = 8 * ((circleY * sinA - sinPhi * cosTheta * cosA) * cosB 
                                      - sinPhi * cosTheta * sinA 
                                      - circleY * cosA 
                                      - cosPhi * cosTheta * sinB);

                    if (y >= 0 && y < height && x >= 0 && x < width && oneOverZ > zBuffer[bufferIndex])
                    {
                        zBuffer[bufferIndex] = oneOverZ;
                        int luminanceIndex = (int)luminance;
                        outputBuffer[bufferIndex] = numLuminanceChars[luminanceIndex > 0 ? (luminanceIndex < 12 ? luminanceIndex : 11) : 0];
                    }
                }
            }

            frameBuffer.Clear();
            string activeColor = "";

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (y == height - 1 && x == width - 1) break;

                    int idx = x + width * y;
                    char ch = outputBuffer[idx];

                    if (ch == ' ')
                    {
                        frameBuffer.Append(' ');
                    }
                    else
                    {
                        string targetColor = (x % 2 == 0) ? blueColor : purpleColor;

                        if (activeColor != targetColor)
                        {
                            frameBuffer.Append(targetColor);
                            activeColor = targetColor;
                        }
                        frameBuffer.Append(ch);
                    }
                }
                if (y < height - 1)
                {
                    frameBuffer.Append('\n');
                }
            }
            frameBuffer.Append(resetColor);

            Console.SetCursorPosition(0, 0);
            Console.Write(frameBuffer.ToString());

            A += 0.08f;
            B += 0.03f;
            Thread.Sleep(30);
        }
    }
}


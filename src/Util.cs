namespace Sisk.Core
{
    internal static class Util
    {
        static EventWaitHandle logWaitHandle = new EventWaitHandle(true, EventResetMode.AutoReset);

        internal static async void WriteColorfulLine(string linePattern, bool enableColors)
        {
            await Task.Run(() =>
            {
                logWaitHandle.WaitOne();
                char[] chars = linePattern.ToCharArray();
                bool skipNext = false;
                for (int i = 0; i < chars.Length; i++)
                {
                    char c = chars[i];
                    if (skipNext)
                    {
                        skipNext = false;
                        continue;
                    }
                    if (c == '&')
                    {
                        char color = chars[i + 1];
                        ConsoleColor consoleColor = color switch
                        {
                            '1' => ConsoleColor.DarkBlue,
                            '9' => ConsoleColor.Blue,
                            '2' => ConsoleColor.DarkGreen,
                            'a' => ConsoleColor.Green,
                            '3' => ConsoleColor.DarkCyan,
                            'b' => ConsoleColor.Cyan,
                            '4' => ConsoleColor.DarkRed,
                            'c' => ConsoleColor.Red,
                            '5' => ConsoleColor.DarkMagenta,
                            'd' => ConsoleColor.Magenta,
                            '6' => ConsoleColor.DarkYellow,
                            'e' => ConsoleColor.Yellow,
                            '7' => ConsoleColor.Gray,
                            '8' => ConsoleColor.DarkGray,
                            '0' => ConsoleColor.Black,
                            'f' => ConsoleColor.White,
                            _ => ConsoleColor.Gray
                        };
                        if (enableColors)
                        {
                            Console.ForegroundColor = consoleColor;
                        }
                        skipNext = true;
                        continue;
                    }
                    Console.Write(c);
                }
                Console.WriteLine();
                logWaitHandle.Set();
            });
        }
    }
}

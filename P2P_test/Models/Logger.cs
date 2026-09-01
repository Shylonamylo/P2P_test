using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace P2P_test.Models;

public static class Logger
{
    private static readonly string FileName = $"{DateTimeOffset.Now.ToString($"yyyy_MM_dd-hh_mm_ss")}" + ".log";

    private static readonly List<string> buffer = new();

    static Logger()
    {
        var _worker = Task.Run(Worker);
        
        if (!File.Exists($"logs/{FileName}"))
        {
            Directory.CreateDirectory($"logs");
        }
    }

    public static void Log(string message)
    {
        buffer.Add(message);
    }

    private static void Worker()
    {
        while (true)
        {
            if (buffer.Count > 0)
            {
                File.AppendAllText($"logs/{FileName}", $"[{DateTimeOffset.Now:T}]: {buffer[0]}{Environment.NewLine}");
                buffer.RemoveAt(0);
            }
            else
            {
                while (buffer.Count==0)
                {
                    Thread.Sleep(100);
                }
            }
        }
    }
}
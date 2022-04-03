#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;

using UdpSockets.ArgsParsing;

namespace UdpSockets
{
    class Program
    {
        private static int Main(string[] args)
        {
            ParsingResult parameters;
            Parser parser = new();

            try
            {
                parameters = parser.Parse(args);
            }
            catch (ParsingFailedException)
            {
                return -1;
            }

            Memory<byte> message = MakeMessage(parameters.DatagramSize);
            IPEndPoint receiverEndPoint = new(parameters.Ip, parameters.Port);

            InterlockedInt64 bytesSent = new();
            InterlockedInt64 tasksCompletedAsync = new();

            using var source = CancelKeyCancellationTokenSource();

            PacketsSender sender = new(
                message,
                receiverEndPoint,
                bytesSent,
                tasksCompletedAsync,
                source
            );

            source.CancelAfter(parameters.Duration);
            Stopwatch stopwatch = Stopwatch.StartNew();

            if (parameters.ThreadsCount == 1)
            {
                sender.SendUsingOneThread();
            }
            else
            {
                sender.SendUsingThreads(parameters.ThreadsCount);
            }

            stopwatch.Stop();

            double mib = bytesSent.Get() / 1024 / 1024;
            double s = stopwatch.Elapsed.TotalSeconds;

            Console.WriteLine($"MiB/s: {mib / s}");

            Console.WriteLine(
                "I/O operations that completed async: {0}",
                tasksCompletedAsync.Get()
            );

            return 0;
        }

        private static Memory<byte> MakeMessage(int sizeInBytes)
        {
            //Use letters only to avoid sending ASCII characters
            //with special meaning, such as EOF, which may affect behaviour
            //of other programs (e.g. netcat)
            List<byte> letters = new();

            for (char c = 'A'; c <= 'z'; ++c)
            {
                letters.Add((byte)c);
            }

            // Taking advantage of pre-pinned memory here using the
            // .NET5 POH (pinned object heap).
            byte[] message = GC.AllocateArray<byte>(sizeInBytes, true);

            for (int i = 0; i < message.Length; ++i)
            {
                message[i] = letters[i % letters.Count];
            }

            return message.AsMemory();
        }

        private static CancellationTokenSource CancelKeyCancellationTokenSource()
        {
            CancellationTokenSource source = new();

            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                source.Cancel();
            };

            return source;
        }
    }
}

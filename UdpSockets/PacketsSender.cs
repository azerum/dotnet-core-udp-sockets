#nullable enable

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace UdpSockets
{
    public class PacketsSender
    {
        private readonly Memory<byte> message;
        private readonly IPEndPoint receiverEndPoint;
        private readonly InterlockedInt64 bytesSent;
        private readonly InterlockedInt64 tasksCompletedAsync;
        private readonly CancellationTokenSource source;

        public PacketsSender(
            Memory<byte> message,
            IPEndPoint receiverEndPoint,
            InterlockedInt64 bytesSent,
            InterlockedInt64 tasksCompletedAsync,
            CancellationTokenSource source
        )
        {
            this.message = message;
            this.receiverEndPoint = receiverEndPoint;
            this.bytesSent = bytesSent;
            this.tasksCompletedAsync = tasksCompletedAsync;
            this.source = source;
        }

        public void SendUsingOneThread()
        {
            DoSend();
        }

        public void SendUsingThreads(int threadsCount)
        {
            List<Thread> threads = new(threadsCount - 1);

            for (int i = 0; i < threads.Capacity; ++i)
            {
                threads.Add(new(DoSend));
            }

            foreach (Thread t in threads)
            {
                t.Start();
            }

            DoSend();

            foreach (Thread t in threads)
            {
                t.Join();
            }
        }

        private void DoSend()
        {
            Socket socket = new(
                AddressFamily.InterNetwork,
                SocketType.Dgram,
                ProtocolType.Udp
            );

            SocketAsyncEventArgs e = new();

            e.RemoteEndPoint = receiverEndPoint;
            e.SetBuffer(message);
            e.Completed += OnCompleted;

            while (!source.Token.IsCancellationRequested)
            {
                if (socket.SendToAsync(e))
                {
                    tasksCompletedAsync.Add(1);
                }
                else
                {
                    OnCompleted(socket, e);
                }
            }

            void OnCompleted(object? sender, SocketAsyncEventArgs e)
            {
                if (e.SocketError != SocketError.Success)
                {
                    source.Cancel();
                    Console.WriteLine($"Socket error: {e.SocketError}");

                    return;
                }

                bytesSent.Add(e.BytesTransferred);
            }
        }
    }
}

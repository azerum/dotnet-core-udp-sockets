using System;
using System.Net;

namespace UdpSockets.ArgsParsing
{
    public record ParsingResult(
        IPAddress Ip,
        int Port,
        int ThreadsCount,
        TimeSpan Duration,
        int DatagramSize
    );
}

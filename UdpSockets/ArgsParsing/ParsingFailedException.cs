#nullable enable

using System;

namespace UdpSockets.ArgsParsing
{
    public class ParsingFailedException : Exception
    {
        public ParsingFailedException() { }
        public ParsingFailedException(string message) : base(message) { }
    }
}

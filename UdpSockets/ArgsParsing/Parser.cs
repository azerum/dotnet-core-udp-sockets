#nullable enable

using System;
using System.Collections.Generic;
using System.Net;
using DocoptNet;

namespace UdpSockets.ArgsParsing
{
    public class Parser
    {
        private const string Usage =
@"UDP sockets perfomance test

Usage:
    command --ip=<target-ip> --port=<target-port>
    [--t=<threads-count> --duration=<seconds> --dgram-size=<bytes>]

";

        private readonly Docopt docopt = new();

        /// <exception cref="ParsingFailedException"></exception>
        public ParsingResult Parse(string[] args)
        {
            IDictionary<string, ValueObject> dictionary;

            try
            {
                dictionary = docopt.Apply(Usage, args)!;
            }
            catch (DocoptBaseException e)
            {
                Console.WriteLine(e.Message);
                throw new ParsingFailedException();
            }

            object? maybeIp = dictionary["--ip"].Value;

            if (
                maybeIp == null
                || !IPAddress.TryParse(maybeIp.ToString(), out IPAddress? ip)
            )
            {
                Console.WriteLine("--ip should be a valid IP address");
                throw new ParsingFailedException();
            }

            object? maybePort = dictionary["--port"].Value;

            if (
                maybePort == null
                || !int.TryParse(maybePort.ToString(), out int port)
                || port < 0
                || port > 65535
            )
            {
                Console.WriteLine(
                    "--port should be an integer from 0 to 65535"
                );

                throw new ParsingFailedException();
            }

            int threadsCount = 1;
            object? maybeThreadsCount = dictionary["--t"].Value;

            if (maybeThreadsCount == null)
            {
                Console.WriteLine($"Note: using --t={threadsCount}");
            }
            else
            {
                threadsCount = ParsePositiveIntegerOrThrow(
                    maybeThreadsCount.ToString()!,
                    "--t"
                );
            }

            int durationInS = 5;
            object? maybeDuration = dictionary["--duration"].Value;

            if (maybeDuration == null)
            {
                Console.WriteLine($"Note: using --duration={durationInS} (s)");
            }
            else
            {
                durationInS = ParsePositiveIntegerOrThrow(
                    maybeDuration.ToString()!,
                    "--duration"
                );
            }

            int datagramSize = 1000;
            object? maybeDatagramSize = dictionary["--dgram-size"].Value;

            if (maybeDatagramSize == null)
            {
                Console.WriteLine($"Note: using --dgram-size={datagramSize} (bytes)");
            }
            else
            {
                datagramSize = ParsePositiveIntegerOrThrow(
                    maybeDatagramSize.ToString()!,
                    "--dgram-size"
                );
            }

            return new ParsingResult(
                ip,
                port,
                threadsCount,
                TimeSpan.FromSeconds(durationInS),
                datagramSize
            );
        }

        private static int ParsePositiveIntegerOrThrow(string s, string argName)
        {
            if (
                !int.TryParse(s, out int result)
                || result <= 0
            )
            {
                Console.WriteLine($"{argName} should be an integer > 0");
                throw new ParsingFailedException();
            }

            return result;
        }
    }
}

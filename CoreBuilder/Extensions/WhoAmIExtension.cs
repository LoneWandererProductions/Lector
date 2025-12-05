/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder.Extensions
 * FILE:        WhoAmIExtension.cs
 * PURPOSE:     Extension for WhoAmI to return individual parameters like "ip" or "hostname". Specially designed for scripting.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Linq;
using System.Net.NetworkInformation;
using Weaver.Interfaces;
using Weaver.Messages;

namespace CoreBuilder.Extensions
{
    /// <inheritdoc />
    /// <summary>
    /// Extension for WhoAmI command to fetch individual parameters
    /// </summary>
    public sealed class WhoAmIExtension : ICommandExtension
    {
        /// <inheritdoc />
        public string Name => "Who";

        /// <inheritdoc />
        public string Description => "Returns specific information of WhoAmI command (e.g., ip, hostname, username)";

        /// <inheritdoc />
        public string Namespace => "System";

        /// <inheritdoc />
        public CommandResult Invoke(ICommand command, string[] args, Func<string[], CommandResult> executor)
        {
            if (args.Length == 0)
            {
                return CommandResult.Fail("No parameter specified. Example: Who(ip)");
            }

            string requested = args[0].ToLowerInvariant();

            try
            {
                var hostname = Environment.MachineName;
                var username = Environment.UserName;
                var domain = Environment.UserDomainName;

                var ips = NetworkInterface
                    .GetAllNetworkInterfaces()
                    .Where(n => n.OperationalStatus == OperationalStatus.Up)
                    .SelectMany(n => n.GetIPProperties().UnicastAddresses)
                    .Where(a => a.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    .Select(a => a.Address.ToString())
                    .Distinct()
                    .ToList();

                string ipsJoined = ips.Any() ? string.Join(", ", ips) : "None";

                return requested switch
                {
                    "hostname" => CommandResult.Ok(message: hostname),
                    "username" => CommandResult.Ok(message: username),
                    "domain" => CommandResult.Ok(message: domain),
                    "ip" => CommandResult.Ok(message: ipsJoined),
                    "os" => CommandResult.Ok(message: Environment.OSVersion.ToString()),
                    "64bitos" => CommandResult.Ok(message: Environment.Is64BitOperatingSystem.ToString()),
                    "64bitprocess" => CommandResult.Ok(message: Environment.Is64BitProcess.ToString()),
                    "processorcount" => CommandResult.Ok(message: Environment.ProcessorCount.ToString()),
                    "clrversion" => CommandResult.Ok(message: Environment.Version.ToString()),
                    _ => CommandResult.Fail($"Unknown parameter '{requested}'.")
                };
            }
            catch (Exception ex)
            {
                return CommandResult.Fail($"WhoAmIExtension failed: {ex.Message}");
            }
        }
    }
}

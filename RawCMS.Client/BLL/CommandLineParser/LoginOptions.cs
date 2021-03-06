//******************************************************************************
// <copyright file="license.md" company="RawCMS project  (https://github.com/arduosoft/RawCMS)">
// Copyright (c) 2019 RawCMS project  (https://github.com/arduosoft/RawCMS)
// RawCMS project is released under GPL3 terms, see LICENSE file on repository root at  https://github.com/arduosoft/RawCMS .
// </copyright>
// <author>Daniele Fontani, Emanuele Bucarelli, Francesco Mina'</author>
// <autogenerated>true</autogenerated>
//******************************************************************************
using CommandLine;

namespace RawCMS.Client.BLL.CommandLineParser
{
    [Verb("login", HelpText = "Perform login. Type login for more help.")]
    public class LoginOptions
    {
        [Option('v', "verbose", Default = false, HelpText = "Prints all messages to standard output.")]
        public bool Verbose { get; set; }

        [Option('u', "username", Required = true, HelpText = "Username")]
        public string Username { get; set; }

        [Option('p', "password", Required = true, HelpText = "Password")]
        public string Password { get; set; }

        [Option('i', "client-id", Required = true, HelpText = "Client id")]
        public string ClientId { get; set; }

        [Option('t', "client-secret", Required = true, HelpText = "Client secret")]
        public string ClientSecret { get; set; }

        [Option('s', "server-url", Required = true, HelpText = "Server URL")]
        public string ServerUrl { get; set; }
    }
}
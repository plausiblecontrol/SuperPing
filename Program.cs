using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace sping {
  class Program {
    static void Main(string[] args) {
      List<string> addresses = new List<string>();
      List<string> errors = new List<string>();
      List<Task<PingReply>> pingTasks = new List<Task<PingReply>>();
      string line;
      Console.WriteLine("Ping 12/9/14" + System.Environment.NewLine);
      Console.WriteLine("Reading your host file...");
      using (StreamReader reader = new StreamReader("C:\\Windows\\System32\\drivers\\etc\\hosts")) {
        while ((line = reader.ReadLine()) != null) {
          try {
            if (line != "") {
              if (line.Substring(0, 1) != "#") {
                addresses.Add(line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)[1]);
              }
            }
          } catch {
            errors.Add(line);
          }
        }
        Console.WriteLine("Preparing the ping test..."+System.Environment.NewLine);
        foreach (var address in addresses) {
          pingTasks.Add(PingAsync(address));
        }
        Task.WaitAll(pingTasks.ToArray());

        //Now you can iterate over your list of pingTasks
        int i = 0;
        foreach (var pingTask in pingTasks) {
          if (pingTask.Result != null) {
            try {
              string s = pingTask.Result.Status.ToString();
              string ts = pingTask.Result.RoundtripTime + " " + pingTask.Result.Options.ToString();
              Console.WriteLine("Ping success for " + addresses[i]);// + " " + pingTask.Result.Status.ToString() + " " + pingTask.Result.RoundtripTime + " " + pingTask.Result.Options.ToString());//Result.RoundtripTime +" for "+addresses[i]);
            } catch {
              errors.Add("Ping failure for " + addresses[i]);
            }
          } else {
            errors.Add("Ping failure for " + addresses[i]);
          }
          i++;
        }
        foreach (string er in errors) {
          Console.WriteLine(er);
        }
        Console.WriteLine(System.Environment.NewLine);
        Console.WriteLine("Finished, press any key..");
        if (args.Length == 0)
            Console.ReadKey();
      }
    }

    static Task<PingReply> PingAsync(string address) {
      var tcs = new TaskCompletionSource<PingReply>();
      Ping ping = new Ping();
      ping.PingCompleted += (obj, sender) => {
        tcs.SetResult(sender.Reply);
      };
      ping.SendAsync(address, new object());
      return tcs.Task;
    }
  }
}

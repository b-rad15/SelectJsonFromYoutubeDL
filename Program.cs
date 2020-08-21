using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SelectJsonFromYoutubeDL
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Process searchProcess = new Process
            {
                StartInfo =
                {
                    FileName = args[0],
                    Arguments = $"\"ytsearch{args[2]}:{args[1]}\" -j -i",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                },
                EnableRaisingEvents = true,
                //I think these are added only after Process Started
                //PriorityClass = ProcessPriorityClass.RealTime,
            };

            // searchProcess.PriorityBoostEnabled = true;

            Console.WriteLine($"yt command: {searchProcess.StartInfo.FileName} {searchProcess.StartInfo.Arguments}");
            
            searchProcess.OutputDataReceived += (sender, e) =>
            {

                if (e.Data == null)
                {
                    return;
                }

                var trimmedLine = e.Data.Trim();

                JObject videoSearchJson = JsonConvert.DeserializeObject(trimmedLine) as JObject;
                TimeSpan duration = TimeSpan.FromSeconds(double.Parse(videoSearchJson["duration"].ToString()));
                JObject parsedJson = new JObject
                {
                    ["uploader_id"] = videoSearchJson["uploader_id"],
                    ["description"] = videoSearchJson["description"]/*?.ToString().Split('\n')[0].Remove(128)*/,
                    ["thumbnail"] = videoSearchJson["thumbnail"],
                    ["title"] = videoSearchJson["title"],
                    ["webpage_url"] = videoSearchJson["webpage_url"],
                    ["duration"] = duration.Hours > 0
                        ? $"{duration.Hours}:{duration.Minutes:D2}:{duration.Seconds:D2}"
                        : $"{duration.Minutes}:{duration.Seconds:D2}"
                };
                // searchResults.Add(new YTResult
                // {
                //     author = videoSearchJson["uploader_id"].ToString(),
                //     description = videoSearchJson["description"].ToString(),
                //     duration = duration.Hours > 0
                //         ? $"{duration.Hours}:{duration.Minutes}:{duration.Seconds}"
                //         : $"{duration.Minutes}:{duration.Seconds}",
                //     thumbnailURL = videoSearchJson["thumbnail"].ToString(),
                //     title = videoSearchJson["title"].ToString(),
                //     URL = videoSearchJson["webpage_url"].ToString()
                // });

                Console.WriteLine(parsedJson.ToString(Formatting.None));
            };

            searchProcess.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data == null)
                {
                    return;
                }
                Console.Error.WriteLine(e.Data);
            };
            searchProcess.Start();
            searchProcess.BeginOutputReadLine();
            searchProcess.BeginErrorReadLine();
            searchProcess.WaitForExit();
            // System.Threading.Thread.Sleep(1000);
            Console.WriteLine("yt command exited");
        }
    }
}

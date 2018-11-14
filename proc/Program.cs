using System;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;

namespace proc
{
    class Program
    {
        static void Main(string[] args)
        {
                string json = "";
                var delimiter = "\t";
                var nl = Environment.NewLine;
                string fileHeader = "Phish"+delimiter+"PhishText"+nl;

                StringBuilder sb = new StringBuilder();
                //sb.Append(fileHeader);

                using (StreamReader r = new StreamReader("../online-valid.json"))
                {
                    json = r.ReadToEnd();
                }

                List<Phish> phishes  = JsonConvert.DeserializeObject<List<Phish>>(json);
                string phishUrl = "";

                foreach(Phish p in phishes){

                    if(p.url.Substring(0,5)=="https"){
                        phishUrl = p.url.Substring(8);
                        Console.WriteLine(phishUrl);
                        sb.Append("1"+delimiter+phishUrl+nl);
                    }else{
                        phishUrl = p.url.Substring(7);
                        Console.WriteLine(phishUrl);
                        sb.Append("1"+delimiter+phishUrl+nl);
                    }
                }

                Console.WriteLine("Total phishes:");
                Console.WriteLine(phishes.Count);
/*
                List<string> al = new List<string>();
                List<Domain> alexa200k = new List<Domain>();

                using(var reader = new StreamReader("../top200k.csv"))
                {

                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var values = line.Split(',');
                        //Console.WriteLine(values[1].Substring(0,values[1].IndexOf(".")));
                        al.Add(values[1]);
                        var length = values[1].Length - (values[1].IndexOf("."));
                        var n = values[1].Substring(0,values[1].IndexOf("."));
                        var t = values[1].Substring(values[1].IndexOf("."),length);
                        //Console.WriteLine(n+" "+t);

                        alexa200k.Add(new Domain(n,t));

                    }
                }

                Console.WriteLine("Total Alexa:");
                Console.WriteLine(alexa200k.Count);

*/
                using(var reader = new StreamReader("../top500.csv"))
                {
                    int i = 0;

                    while (!reader.EndOfStream)
                    //while (i < 500)
                    {
                        var line = reader.ReadLine();
                        var values = line.Split(',');

                        sb.Append("0"+delimiter+values[1]+nl);

                        int index = values[1].IndexOf("o");
                        if(index >=0){
                            sb.Append("1"+delimiter+ReplaceFirst(values[1],"o","0")+nl);
                        }
                        int index2 = values[1].IndexOf("a");
                        if(index2 >=0){
                            sb.Append("1"+delimiter+values[1].Replace("a","\u03B1")+nl);
                        }
                        //Console.WriteLine(values[1].Substring(0,values[1].IndexOf(".")));
                        /*
                        al.Add(values[1]);
                        var length = values[1].Length - (values[1].IndexOf("."));
                        var n = values[1].Substring(0,values[1].IndexOf("."));
                        var t = values[1].Substring(values[1].IndexOf("."),length);*/
                        //Console.WriteLine(n+" "+t);
                        var bashStr = "./curl.sh http://"+values[1];
                        String result = bashStr.Bash();
                        string[] array = result.Split(Environment.NewLine.ToCharArray(),StringSplitOptions.RemoveEmptyEntries);

                        foreach(String item in array){

                                    if(item.Length <= 1){
                                    }else if(item.Substring(0,2)=="//"){
                                        sb.Append("0"+delimiter+item.Substring(2)+nl);
                                    }else{
                                        sb.Append("0"+delimiter+values[1]+item+nl);
                                    }
                                    //Console.WriteLine(item);
                        }

                        Console.WriteLine(i);
                        i++;
                    }
                }

            var strFilePath = "Data/output.tsv";

            // Create the File


            using (FileStream fs = File.Create(strFilePath))
            {
                Byte[] info = new UTF8Encoding(true).GetBytes(sb.ToString());

                // Add some information to the file.
                fs.Write(info, 0, info.Length);
            }

            string ReplaceFirst(string text, string search, string replace)
            {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
            }

        }

        public int GetNthIndex(string s, char t, int n)
        {
            int count = 0;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == t)
                {
                    count++;
                    if (count == n)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }




    }
        public class Phish
        {
            public string phish_id;
            public string url;
            public string phish_detail_url;
            public string submission_time;
            public string verified;
            public string verification_time;
            public string online;
            public List<Detail> details;
            public string target;

        }

        public class Detail
        {
            public string ip_address;
            public string cidr_block;
            public string announcing_network;
            public string rir;
            public string country;
            public string detail_time;

        }

        public class Domain{
            public Domain(string n, string t){
                tld = t;
                name = n;
            }
            public string tld;
            public string name;
        }


        public static class ShellHelper
        {
            public static string Bash(this string cmd)
            {
                var escapedArgs = cmd.Replace("\"", "\\\"");

                var process = new Process()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "/bin/bash",
                        Arguments = $"-c \"{escapedArgs}\"",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };
                process.Start();
                string result = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                return result;
            }
        }



}

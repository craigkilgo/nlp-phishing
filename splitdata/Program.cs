using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;

namespace Data
{
    class Program
    {
        static void Main(string[] args)
        {

            Random randm = new Random();
            var nl = Environment.NewLine;
            
            //int rand_month = randm.Next(1,13);
            StringBuilder training = new StringBuilder();
            StringBuilder testing = new StringBuilder();
            training.Append("Phish");
            training.Append("\t");
            training.Append("UrlText");
            training.Append(nl);

            testing.Append("Phish");
            testing.Append("\t");
            testing.Append("UrlText");
            testing.Append(nl);
            
                using(var reader = new StreamReader("output.tsv"))
                {

                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        //var values = line.Split('\n');
                        //Console.WriteLine(line);
                        //Console.WriteLine("-------");
                        int random = randm.Next(1,10);
                        if(random < 4){
                            testing.Append(line);
                            testing.Append(nl);
                        }else{
                            training.Append(line);
                            training.Append(nl);
                        }



                    }
                }
            String trainingPath = "training-data.tsv";
            String testingPath = "testing-data.tsv";

            using (FileStream fs = File.Create(trainingPath))
            {
                Byte[] info = new UTF8Encoding(true).GetBytes(training.ToString());

                // Add some information to the file.
                fs.Write(info, 0, info.Length);
            }

            using (FileStream fs = File.Create(testingPath))
            {
                Byte[] info = new UTF8Encoding(true).GetBytes(testing.ToString());

                // Add some information to the file.
                fs.Write(info, 0, info.Length);
            }

        }
    }
}

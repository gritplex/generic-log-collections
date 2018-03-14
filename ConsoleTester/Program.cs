using LogCollections;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Linq;
using static System.Text.Encoding;

namespace ConsoleTester
{
    class Program
    {
        static void Main(string[] args)
        {
            int cnt = 10000;
            int mod = 1000;

            if (args.Length > 0 && int.TryParse(args[0], out int usercnt))
                cnt = usercnt;

            if (args.Length > 1 && int.TryParse(args[1], out int usermod))
                cnt = mod;

            var rnd = new Random();
            var set = new LogSortedSet<Data>(
                "test",
                100,
                d => (d.Id),
                d => UTF8.GetBytes(JsonConvert.SerializeObject(d)),
                bArr => JsonConvert.DeserializeObject<Data>(UTF8.GetString(bArr)));

            var data = new Data[cnt];

            for (int i = 0; i < cnt; i++)
            {
                var d = new Data
                {
                    DateTime = DateTime.Now,
                    Id = new Guid(
                        BitConverter.GetBytes(rnd.Next() % 5000)
                        .Concat(BitConverter.GetBytes(2))
                        .Concat(BitConverter.GetBytes(2))
                        .Concat(BitConverter.GetBytes(2)).ToArray()),
                    Num = i,
                    Name = rnd.Next().ToString()
                };

                data[i] = d;                
            }

            var w = new Stopwatch();

            w.Start();
            for (int i = 0; i < cnt; i++)
            {                
                set.Add(data[i]);
                set.Remove(data[i]);
            }
            w.Stop();
            Console.WriteLine($"WRITE: {w.ElapsedMilliseconds}ms  -> {(double)w.ElapsedMilliseconds / cnt}ms/op  -> {cnt / TimeSpan.FromMilliseconds(w.ElapsedMilliseconds).TotalSeconds}ops/sec");

            //var log = new BinaryLog("test", sizeof(byte) * 1024 * 1024 * 5);

            //w.Start();
            //foreach (var entry in data)
            //{
            //    log.Append(entry);
            //}
            //w.Stop();

            Guid.NewGuid().ToByteArray();

            //var read = new List<LogEntry>(cnt);
            //w.Restart();
            //foreach (var entry in log)
            //{
            //    read.Add(entry);
            //}
            //w.Stop();
            //Console.WriteLine($"READ: {w.ElapsedMilliseconds}ms  -> {(double)w.ElapsedMilliseconds / cnt}ms/op");

            //w.Restart();
            //set.Compact();
            //w.Stop();
            //Console.WriteLine($"COMPACT: {w.ElapsedMilliseconds}ms");

            //for (int i = 0; i < data.Length; i++)
            //{
            //    Debug.Assert(data[i].Id == read[i].Id);
            //    Debug.Assert(data[i].Key == read[i].Key);
            //    for (int j = 0; j < data[i].Value.Length; j++)
            //    {
            //        Debug.Assert(data[i].Value[j] == read[i].Value[j]);
            //    }
            //}

            //Console.WriteLine("All Checks passed!");
        }
    }

    public class Data : IComparable<Data>
    {
        public DateTime DateTime { get; set; }
        public string Name { get; set; }
        public Guid Id { get; set; }
        public int Num { get; set; }

        public int CompareTo(Data other) => Num.CompareTo(other.Num);
    }
}

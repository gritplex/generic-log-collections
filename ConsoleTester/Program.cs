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
                mod = usermod;

            var rnd = new Random();
            var set = new LogSet<Data>(
                ".",
                "test",
                100,
                d => (rnd.Next() % 10),
                d => (d.Id),
                d => UTF8.GetBytes(JsonConvert.SerializeObject(d)),
                bArr => JsonConvert.DeserializeObject<Data>(UTF8.GetString(bArr)),
                maxFileSize: sizeof(byte) * 2048 * 1000);

            var data = new Data[cnt];

            for (int i = 0; i < cnt; i++)
            {
                var d = new Data
                {
                    DateTime = DateTime.Now,
                    Id = new Guid(
                        BitConverter.GetBytes(rnd.Next() % mod)
                        .Concat(BitConverter.GetBytes(2))
                        .Concat(BitConverter.GetBytes(2))
                        .Concat(BitConverter.GetBytes(2)).ToArray()),
                    //Id = Guid.NewGuid(),
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
                //set.Remove(data[i]);
            }
            w.Stop();
            //set = null;
            //GC.WaitForPendingFinalizers();
            //GC.Collect();
            Console.WriteLine($"WRITE: {w.ElapsedMilliseconds}ms  -> {(double)w.ElapsedMilliseconds / cnt}ms/op  -> {cnt / TimeSpan.FromMilliseconds(w.ElapsedMilliseconds).TotalSeconds}ops/sec");

            set.Compact(false);

            w.Restart();
            set = new LogSet<Data>(
                ".",
                "test",
                100,
                d => (rnd.Next() % 10),
                d => (d.Id),
                d => UTF8.GetBytes(JsonConvert.SerializeObject(d)),
                bArr => JsonConvert.DeserializeObject<Data>(UTF8.GetString(bArr)),
                true);
            w.Stop();
            //foreach (var item in set)
            //{
            //    Console.WriteLine($"{item.DateTime} ; {item.Name} ; {item.Num} ; {item.Id}");
            //}            
            Console.WriteLine($"Read {set.Count} in {w.ElapsedMilliseconds}ms  -> {(double)w.ElapsedMilliseconds / set.Count}ms/op  -> {set.Count / TimeSpan.FromMilliseconds(w.ElapsedMilliseconds).TotalSeconds}ops/sec");
        }
    }

    public class Data : IComparable<Data>, IEquatable<Data>
    {
        public DateTime DateTime { get; set; }
        public string Name { get; set; }
        public Guid Id { get; set; }
        public int Num { get; set; }

        public int CompareTo(Data other) => Id.CompareTo(other.Id);
        public bool Equals(Data other) => Id.Equals(other.Id);

        public override bool Equals(object obj) => Id.Equals(Id);
        public override int GetHashCode() => Id.GetHashCode();
    }


}

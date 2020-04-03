using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace CCC2019SS
{
    #region Helpers

    public static class StringExtension
    {
        public static int AsInt(this String str)
        {
            return Convert.ToInt32(str);
        }
    }

    public static class EnumerableExtension
    {
        public static T PickRandom<T>(this IEnumerable<T> source)
        {
            return source.PickRandom(1).Single();
        }

        public static IEnumerable<T> PickRandom<T>(this IEnumerable<T> source, int count)
        {
            return source.Shuffle().Take(count);
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return source.OrderBy(x => Guid.NewGuid());
        }
    }

    public class Helpers
    {
        public static Random rand = new Random();

        public static T DeepClone<T>(T obj)
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;

                return (T)formatter.Deserialize(ms);
            }
        }
    }

    #endregion

    [Serializable]
    public class Quest
    {
        public int StartX { get; set; }
        public int StartY { get; set; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public int FinishX { get; set; }
        public int FinishY { get; set; }
        public int Direction { get; set; }

        public List<Command> Commands { get; set; }

        public Quest()
        {
            this.Commands = new List<Command>();
        }

        public void MoveToEnd()
        {
            this.PositionX = this.StartX;
            this.PositionY = this.StartY;
            foreach (var c in this.Commands)
            {
                if (c.Type == "F")
                {
                    if (Direction == 0)
                    {
                        this.PositionX += c.NTimes;
                    }
                    else if (Direction == 1)
                    {
                        this.PositionY += c.NTimes;
                    }
                    else if (Direction == 2)
                    {
                        this.PositionX -= c.NTimes;
                    }
                    else if (Direction == 3)
                    {
                        this.PositionY -= c.NTimes;
                    }
                }
                else if (c.Type =="T")
                {
                    for (int i = 0; i < c.NTimes; i++)
                    {
                        this.Direction = (this.Direction + 1) % 4;
                    }
                }
            }
            this.FinishX = this.PositionX;
            this.FinishY = this.PositionY;
        }

    }

    [Serializable]
    public class Command
    {
        public string Type { get; set; }
        public int NTimes { get; set; }

    }

    class Program
    {
        static void Main(string[] args)
        {
            var filenames = Enumerable.Range(1, 5).Select(p => "..\\..\\data\\level1_" + p + ".in").ToList();
            List<String> outputText = new List<string>();
            foreach (var filename in filenames)
            {
                Console.WriteLine(filename);
                string[] lines = System.IO.File.ReadAllLines(filename);
                int[] props = lines[0].Split(' ').Select(p => Convert.ToInt32(p)).ToArray();
                var quest = new Quest() { StartX = props[0], StartY = props[1] };
                var data = lines[1].Split(' ');
                for (int i = 0; i < data.Length; i+=2)
                {
                    quest.Commands.Add(new Command() { Type = data[i], NTimes = data[i + 1].AsInt() });
                }

                quest.MoveToEnd();



                //String s = String.Join(",", quest.Commandss.Select(p => "").ToList());
                String s = quest.FinishX + " " + quest.FinishY;
                Console.WriteLine(s);
                System.IO.File.WriteAllText(filename + ".out", s);
                outputText.Add(s);
            }
            System.IO.File.WriteAllLines("..\\..\\data\\level1.out", outputText);
            Console.ReadKey();
        }
    }
}


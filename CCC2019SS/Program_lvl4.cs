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
        public static double AsDouble(this String str)
        {
            return double.Parse(str.Replace(".", ","));
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
        public int WX { get; set; }
        public int WY { get; set; }
        public int StartX { get; set; }
        public int StartY { get; set; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public int FinishX { get; set; }
        public int FinishY { get; set; }
        public int Direction { get; set; }
        public int NAliens { get; set; }
        public int NQueries { get; set; }
        public int Time { get; set; }
        public double Speed { get; set; }

        public List<Command> Commands { get; set; }
        public List<Point> VisitedPoints { get; set; }
        public List<Alien> Aliens { get; set; }
        public List<Query> Queries { get; set; }

        public Quest()
        {
            this.Time = 0;
            this.Commands = new List<Command>();
            this.VisitedPoints = new List<Point>();
            this.Aliens = new List<Alien>();
            this.Queries = new List<Query>();
        }

        public void MoveToEnd()
        {
            this.PositionX = this.StartX;
            this.PositionY = this.StartY;
            this.VisitedPoints.Add(new Point() { X = this.PositionX, Y = this.PositionY });
            foreach (var c in this.Commands)
            {
                if (c.Type == "F")
                {
                    if (Direction == 0)
                    {
                        for (int i = 0; i < c.NTimes; i++)
                        {
                            this.PositionX++;
                            this.VisitedPoints.Add(new Point() { X = this.PositionX, Y = this.PositionY });
                        }
                    }
                    else if (Direction == 1)
                    {
                        for (int i = 0; i < c.NTimes; i++)
                        {
                            this.PositionY++;
                            this.VisitedPoints.Add(new Point() { X = this.PositionX, Y = this.PositionY });
                        }
                    }
                    else if (Direction == 2)
                    {
                        for (int i = 0; i < c.NTimes; i++)
                        {
                            this.PositionX--;
                            this.VisitedPoints.Add(new Point() { X = this.PositionX, Y = this.PositionY });
                        }
                    }
                    else if (Direction == 3)
                    {
                        for (int i = 0; i < c.NTimes; i++)
                        {
                            this.PositionY--;
                            this.VisitedPoints.Add(new Point() { X = this.PositionX, Y = this.PositionY });
                        }
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

        public void DoQueries()
        {
            foreach(var q in this.Queries)
            {
                var alien = this.Aliens.Where(p => p.Id == q.AlienId).SingleOrDefault();
                double timeToGo = q.Tick - alien.SpawnTime;
                int timeIndex = (int)System.Math.Floor(timeToGo * this.Speed);
                var finishPoint = this.VisitedPoints[timeIndex];
                q.ResultX = finishPoint.X;
                q.ResultY = finishPoint.Y;
            }
        }
    }

    [Serializable]
    public class Point
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Point()
        {

        }
    }

    [Serializable]
    public class Command
    {
        public string Type { get; set; }
        public int NTimes { get; set; }

    }

    [Serializable]
    public class Alien
    {
        public int Id { get; set; }
        public int SpawnTime { get; set; }

    }

    [Serializable]
    public class Query
    {
        public int AlienId { get; set; }
        public int Tick { get; set; }
        public int ResultX { get; set; }
        public int ResultY { get; set; }

    }

    class Program
    {
        static void Main(string[] args)
        {
            var filenames = Enumerable.Range(1, 5).Select(p => "..\\..\\data\\level3_" + p + ".in").ToList();
            List<String> outputText = new List<string>();
            foreach (var filename in filenames)
            {
                Console.WriteLine(filename);
                string[] lines = System.IO.File.ReadAllLines(filename);
                int[] props = lines[0].Split(' ').Select(p => Convert.ToInt32(p)).ToArray();
                int[] props2 = lines[1].Split(' ').Select(p => Convert.ToInt32(p)).ToArray();
                var quest = new Quest() { WX = props[0], WY = props[1], StartX = props2[0], StartY = props2[1] };
                var data = lines[2].Split(' ');
                for (int i = 0; i < data.Length; i+=2)
                {
                    quest.Commands.Add(new Command() { Type = data[i], NTimes = data[i + 1].AsInt() });
                }
                quest.Speed = lines[3].AsDouble();
                quest.NAliens = lines[4].AsInt();
                for (int i = 0; i < quest.NAliens; i++)
                {
                    var dataAlien = lines[i + 5].Split(' ');
                    quest.Aliens.Add(new Alien() { Id = i, SpawnTime = dataAlien[0].AsInt() });
                }
                quest.NQueries = lines[5 + quest.NAliens].AsInt();
                for (int i = 0; i < quest.NQueries; i++)
                {
                    var dataQuery = lines[6 + quest.NAliens + i].Split(' ');
                    quest.Queries.Add(new Query() { Tick = dataQuery[0].AsInt(), AlienId = dataQuery[1].AsInt() });
                }

                quest.MoveToEnd();

                quest.DoQueries();


                var s = quest.Queries.Select(p => p.Tick + " " + p.AlienId + " " + p.ResultX + " " + p.ResultY);
                //var s = quest.VisitedPoints.Select(p => p.X + " " + p.Y);
                //String s = String.Join(",", quest.Commandss.Select(p => "").ToList());
                //Console.WriteLine(s);
                //System.IO.File.WriteAllText(filename + ".out", s);
                System.IO.File.WriteAllLines(filename + ".out", s);
                //outputText.Add(s);
            }
            //System.IO.File.WriteAllLines("..\\..\\data\\level1.out", outputText);
            Console.ReadKey();
        }
    }
}


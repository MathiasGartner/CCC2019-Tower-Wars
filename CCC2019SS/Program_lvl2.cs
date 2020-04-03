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
        public double Health { get; set; }
        public int NTowers { get; set; }
        public int Range { get; set; }
        public double Damage { get; set; }
        public bool Won { get; set; }
        public int WinOrLooseTime { get; set; }
        public int BaseX { get; set; }
        public int BaseY { get; set; }

        public List<Command> Commands { get; set; }
        public List<Point> VisitedPoints { get; set; }
        public List<Alien> Aliens { get; set; }
        public List<Query> Queries { get; set; }
        public List<Tower> Towers { get; set; }

        public Quest()
        {
            this.Time = 0;
            this.Commands = new List<Command>();
            this.VisitedPoints = new List<Point>();
            this.Aliens = new List<Alien>();
            this.Queries = new List<Query>();
            this.Towers = new List<Tower>();
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
                else if (c.Type == "T")
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
            foreach (var q in this.Queries)
            {
                var alien = this.Aliens.Where(p => p.Id == q.AlienId).SingleOrDefault();
                double timeToGo = q.Tick - alien.SpawnTime;
                int timeIndex = (int)System.Math.Floor(timeToGo * this.Speed);
                var finishPoint = this.VisitedPoints[timeIndex];
                q.ResultX = finishPoint.X;
                q.ResultY = finishPoint.Y;
            }
        }

        public void GetAlienPosition()
        {

        }

        public void Fight()
        {
            var lastPoint = this.VisitedPoints.Last();
            var lastlastPoint = this.VisitedPoints[this.VisitedPoints.Count - 2];
            this.BaseX = lastPoint.X + lastPoint.X - lastlastPoint.X;
            this.BaseY = lastPoint.Y + lastPoint.Y - lastlastPoint.Y;

            foreach(var t in this.Towers)
            {
                
            }
            foreach(var a in this.Aliens)
            {
                if (a.SpawnTime == 0)
                {
                    a.IsSpawned = true;
                    a.X = this.VisitedPoints[0].X;
                    a.Y = this.VisitedPoints[0].Y;
                }
            }

            int i = 0;
            while(true)
            {
                i++;
                foreach (var a in this.Aliens.Where(p => !p.IsDead && p.IsSpawned))
                {
                    double timeToGo = i - a.SpawnTime;
                    int timeIndex = (int)System.Math.Floor(timeToGo * this.Speed);
                    if (timeIndex >= this.VisitedPoints.Count)
                    {
                        //INFO: is in base
                        this.Won = false;
                        this.WinOrLooseTime = i;
                        return;
                    }
                    var finishPoint = this.VisitedPoints[timeIndex];
                    //TODO: calc dblX
                    a.X = finishPoint.X;
                    a.Y = finishPoint.Y;
                }

                foreach (var a in this.Aliens.Where(p => !p.IsDead && p.IsSpawned))
                {
                    if (a.X == this.BaseX && a.Y == this.BaseY)
                    {
                        this.Won = false;
                        this.WinOrLooseTime = i;
                        return;
                    }
                }

                foreach (var a in this.Aliens.Where(p => !p.IsDead && !p.IsSpawned))
                {
                    if (a.SpawnTime == i)
                    {
                        a.IsSpawned = true;
                        a.X = this.VisitedPoints[0].X;
                        a.Y = this.VisitedPoints[0].Y;
                    }
                }

                foreach (var t in this.Towers)
                {
                    if (t.Status == "L")
                    {
                        var alien = this.Aliens.Where(p => p.Id == t.AlienId).SingleOrDefault();
                        if (alien.IsInRange(t.X, t.Y, this.Range) && !alien.IsDead)
                        {

                        }
                        else
                        {
                            foreach(var a in this.Aliens.Where(p => !p.IsDead && p.IsSpawned))
                            {
                                a.CalcDistanceToTower(t);
                            }
                            var nextAlien = this.Aliens.Where(p => !p.IsDead && p.IsSpawned).OrderBy(p => p.towerDistance).ThenBy(p => p.Id).FirstOrDefault();
                            //var nextAlien = this.Aliens.Aggregate((i1, i2) => i1.DistanceToTower(t.X, t.Y) < i2.DistanceToTower(t.X, t.Y) ? i1 : i2);
                            //var nextAlien = this.Aliens.Min(p => p.DistanceToTower(t.X, t.Y));
                            //var minDistance = this.Aliens.Min(p => p.DistanceToTower(t.X, t.Y));
                            //var nextAlien = this.Aliens.Where(p => p.DistanceToTower(t.X, t.Y) == minDistance).First();
                            if (nextAlien != null && nextAlien.DistanceToTower(t.X, t.Y) <= this.Range)
                            {
                                t.AlienId = nextAlien.Id;
                            }
                            else
                            {
                                t.Status = "S";
                            }
                        }
                    }
                    if (t.Status == "S")
                    {
                        foreach (var a in this.Aliens.Where(p => !p.IsDead && p.IsSpawned))
                        {
                            a.CalcDistanceToTower(t);
                        }
                        var nextAlien = this.Aliens.Where(p => !p.IsDead && p.IsSpawned).OrderBy(p => p.towerDistance).ThenBy(p => p.Id).FirstOrDefault();
                        //var nextAlien = this.Aliens.Min(p => p.DistanceToTower(t.X, t.Y));
                        if (nextAlien != null && nextAlien.DistanceToTower(t.X, t.Y) <= this.Range)
                        {
                            t.AlienId = nextAlien.Id;
                            t.Status = "L";
                        }
                    }
                }

                //shoot
                foreach (var t in this.Towers.Where(p => p.Status == "L"))
                {
                    var alien = this.Aliens.Where(p => p.Id == t.AlienId).SingleOrDefault();
                    alien.RemainingHealth -= this.Damage;
                }

                //check for dead
                foreach(var a in this.Aliens.Where(p => p.IsSpawned && !p.IsDead && p.RemainingHealth <= 0).ToList())
                {
                    a.IsDead = true;
                }

                if (this.Aliens.Where(p => p.IsDead).Count() == this.NAliens)
                {
                    this.Won = true;
                    this.WinOrLooseTime = i;
                    return;
                }
            }
        }
    }

    [Serializable]
    public class Tower
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string Status { get; set; } //"L", "S"
        public int AlienId { get; set; }

        public Tower()
        {
            this.Status = "S";
            this.AlienId = -1;
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
        public double RemainingHealth { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public bool IsDead { get; set; }
        public bool IsSpawned { get; set; }


        public double towerDistance { get; set; }

        public Alien()
        {
            this.IsDead = false;
            this.IsSpawned = false;
        }

        public void CalcDistanceToTower(Tower t)
        {
            towerDistance = DistanceToTower(t.X, t.Y);
        }

        public bool IsInRange(int tX, int tY, int range)
        {
            int diffX = tX - this.X;
            int diffY = tY - this.Y;
            if (Math.Sqrt(diffX * diffX + diffY * diffY) <= range)
            {
                return true;
            }
            return false;
        }

        public double DistanceToTower(int tX, int tY)
        {
            int diffX = tX - this.X;
            int diffY = tY - this.Y;
            return Math.Sqrt(diffX * diffX + diffY * diffY);
        }
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
            var filenames = Enumerable.Range(4, 5).Select(p => "..\\..\\data\\level4_" + p + ".in").ToList();
            List<String> outputText = new List<string>();
            foreach (var filename in filenames)
            {
                Console.WriteLine(filename);
                string[] lines = System.IO.File.ReadAllLines(filename);
                int[] props = lines[0].Split(' ').Select(p => Convert.ToInt32(p)).ToArray();
                int[] props2 = lines[1].Split(' ').Select(p => Convert.ToInt32(p)).ToArray();
                var quest = new Quest() { WX = props[0], WY = props[1], StartX = props2[0], StartY = props2[1] };

                var data = lines[2].Split(' ');
                for (int i = 0; i < data.Length; i += 2)
                {
                    quest.Commands.Add(new Command() { Type = data[i], NTimes = data[i + 1].AsInt() });
                }

                var dataAliensProps = lines[3].Split(' ');
                quest.Health = dataAliensProps[0].AsDouble();
                quest.Speed = dataAliensProps[1].AsDouble();

                quest.NAliens = lines[4].AsInt();
                for (int i = 0; i < quest.NAliens; i++)
                {
                    var dataAlien = lines[i + 5].Split(' ');
                    quest.Aliens.Add(new Alien() { Id = i, SpawnTime = dataAlien[0].AsInt(), RemainingHealth = quest.Health });
                }

                var dataTowers = lines[5 + quest.NAliens].Split(' ');
                quest.Damage = dataTowers[0].AsDouble();
                quest.Range = dataTowers[1].AsInt();
                quest.TowerCost = dataTowers[2].AsInt();



                //quest.NTowers = lines[6 + quest.NAliens].AsInt();
                //
                //for (int i = 0; i < quest.NTowers; i++)
                //{
                //    var dataTower = lines[7 + quest.NAliens + i].Split(' ');
                //    quest.Towers.Add(new Tower() { X = dataTower[0].AsInt(), Y = dataTower[1].AsInt() });
                //}

                //quest.NQueries = lines[5 + quest.NAliens].AsInt();
                //for (int i = 0; i < quest.NQueries; i++)
                //{
                //    var dataQuery = lines[6 + quest.NAliens + i].Split(' ');
                //    quest.Queries.Add(new Query() { Tick = dataQuery[0].AsInt(), AlienId = dataQuery[1].AsInt() });
                //}

                quest.MoveToEnd();

                quest.Fight();

                //quest.DoQueries();


                //var s = quest.Queries.Select(p => p.Tick + " " + p.AlienId + " " + p.ResultX + " " + p.ResultY);
                //var s = quest.VisitedPoints.Select(p => p.X + " " + p.Y);
                //String s = String.Join(",", quest.Commandss.Select(p => "").ToList());
                //Console.WriteLine(s);
                //System.IO.File.WriteAllText(filename + ".out", s);
                string[] s = { quest.WinOrLooseTime.ToString(), quest.Won ? "WIN" : "LOSS" };
                System.IO.File.WriteAllLines(filename + ".out", s);
                //outputText.Add(s);
            }
            //System.IO.File.WriteAllLines("..\\..\\data\\level1.out", outputText);
            Console.ReadKey();
        }
    }
}


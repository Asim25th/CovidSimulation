using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia;
using System.Threading.Tasks;

namespace CovidSimulation
{
    public class Person // Данные каждого человека
    {
        public int Id { get; set; }
        public double DaysInfected { get; set; }
        public Point Coordinates { get; set; }
        public double SpeedX { get; set; }
        public double SpeedY { get; set; }
        public CovidStatus Status { get; set; }

        private Random Random { get; } = new Random();

        public Person()
        {
            Status = CovidStatus.Susceptible;
            GenerateRandomSpeed();
        }

        private void GenerateRandomSpeed()
        {
            SpeedX = Random.NextDouble() * 2 - 1;
            SpeedY = Random.NextDouble() * 2 - 1;
        }

        public void Move(double canvasWidth, double canvasHeight)
        {
            Coordinates = new Point(Coordinates.X + SpeedX, Coordinates.Y + SpeedY);

            if (Coordinates.X < 0 || Coordinates.X > canvasWidth)
            {
                SpeedX *= -1;
            }
            if (Coordinates.Y < 0 || Coordinates.Y > canvasHeight)
            {
                SpeedY *= -1;
            }
        }
    }
}
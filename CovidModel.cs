using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CovidSimulation
{
    public class CovidModel
    {
        public List<Person> People { get; set; } = new List<Person>();
        public Random Random { get; set; } = new Random();

        public double InfectChance {  get; set; }
        public double InfectRadius {  get; set; }

        public int PopulationSize { get; set; }

        public double CanvasWidth { get; set; } // Параметры окна, в котором будет создаваться популяция
        public double CanvasHeight { get; set; }
      
        public void CreatePopulation() // Создание популяции, которая будет передаваться в Canvas
        {
            People.Clear();

            for (int i = 0; i < PopulationSize; i++)
            {
                double xCoordinate = Random.NextDouble() * CanvasWidth;
                double yCoordinate = Random.NextDouble() * CanvasHeight;
                Point startCoordinates = new Point(xCoordinate, yCoordinate); // Рандомная позиция человека

                Person person = new Person()
                {
                    Id = i,
                    Coordinates = new Point(
                        startCoordinates.X,
                        startCoordinates.Y
                        ),
                    DaysInfected = 0
                };

                People.Add(person);
            }
        }

        public void CheckPersonInfectionTime(List<Person> people)
        {
            foreach (var person in people.Where(p => p.Status == CovidStatus.Infected))
            {
                if (person.DaysInfected >= 21)
                {
                    if (Random.NextDouble() <= 0.12)
                    {
                        person.Status = CovidStatus.Dead;
                    }
                    else
                    {
                        person.Status = CovidStatus.Recovered;
                    }
                }
                else
                {
                    person.DaysInfected += 1; // Увеличение количества дней заражения
                }
            }
        }

        public void Start()
        {
            People[0].Status = CovidStatus.Infected;
        }

        public double Distance(Person person1, Person person2)
        {
            return Math.Sqrt(
                Math.Pow(person1.Coordinates.X - person2.Coordinates.X, 2) + 
                Math.Pow(person1.Coordinates.Y - person2.Coordinates.Y, 2)
                );
        }

        public void CheckInfections(List<Person> people) // Проверка столкновений и заражение людей
        {
            foreach (var infectedPerson in people.Where(p => p.Status == CovidStatus.Infected))
            {
                foreach (var susceptiblePerson in people.Where(p => p.Status == CovidStatus.Susceptible))
                {
                    double distance = Distance(infectedPerson, susceptiblePerson);
                    if (distance <= InfectRadius)
                    {
                        // Заражение с вероятностью
                        if (Random.NextDouble() <= InfectChance)
                        {
                            susceptiblePerson.Status = CovidStatus.Infected;
                        }
                    }
                }
            }
        }
    }
}
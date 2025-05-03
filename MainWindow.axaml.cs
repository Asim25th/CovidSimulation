using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace CovidSimulation
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private CovidModel _covidModel;

        private List<Person> _people = new List<Person>();

        private DispatcherTimer _timer; // Обновление движения
        private DispatcherTimer _dayTimer; // Обновление дней и соответствующих статусов заражения

        // Списки для привязки к графику
        public ObservableCollection<int> SusceptibleSpisok { get; set; } = new ObservableCollection<int>();
        public ObservableCollection<int> InfectedSpisok { get; set; } = new ObservableCollection<int>();
        public ObservableCollection<int> RecoveredSpisok { get; set; } = new ObservableCollection<int>();
        public ObservableCollection<int> DeadSpisok { get; set; } = new ObservableCollection<int>();

        private double _r;
        public double R // Значения из covidModel
        {
            get => _r;
            set
            {
                _r = value;
                OnPropertyChanged(nameof(R));
            }
        }

        private double _days;
        public double Days 
        {
            get => _days;
            set
            {
                _days = value;
                OnPropertyChanged(nameof(Days));
            }
        }

        private double _activecases;
        public double ActiveCases
        {
            get => _activecases;
            set
            {
                _activecases = value;
                OnPropertyChanged(nameof(ActiveCases));
            }
        }

        public ISeries[] Series { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            _covidModel = new CovidModel();

            // Создание популяции после нажатия на кнопку Начать
            _covidModel.CanvasWidth = SimulationPole.Width; 
            _covidModel.CanvasHeight = SimulationPole.Height;

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(30);
            _timer.Tick += Movement_Tick;

            _dayTimer = new DispatcherTimer();
            _dayTimer.Interval = TimeSpan.FromMilliseconds(300);
            _dayTimer.Tick += DayTimer_Tick;

            // Инициализация коллекции ObservableCollection
            SusceptibleSpisok = new ObservableCollection<int>();
            InfectedSpisok = new ObservableCollection<int>();
            RecoveredSpisok = new ObservableCollection<int>();
            DeadSpisok = new ObservableCollection<int>();

            Series = new ISeries[]
            {
                new StackedAreaSeries<int>
                {
                    Name = "Suspectible",
                    Fill = new SolidColorPaint(SKColors.LightBlue),
                    Values = SusceptibleSpisok
                },
                new StackedAreaSeries<int>
                {
                    Name = "Infected",
                    Fill = new SolidColorPaint(SKColors.Red),
                    Values = InfectedSpisok
                },
                new StackedAreaSeries<int>
                {
                    Name = "Recovered",
                    Fill = new SolidColorPaint(SKColors.Gray),
                    Values = RecoveredSpisok
                },
                new StackedAreaSeries<int>
                {
                    Name = "Dead",
                    Fill = new SolidColorPaint(SKColors.Purple),
                    Values = DeadSpisok
                }
            };

            DataContext = this;
        }

        private void DayTimer_Tick(object sender, EventArgs e)
        {
            Days++;
            _covidModel.CheckPersonInfectionTime(_people);
        }

        private void Movement_Tick(object sender, EventArgs e)
        {
            // Обновление позиций людей в основной зоне
            foreach (var person in _people)
            {
                person.Move(_covidModel.CanvasWidth, _covidModel.CanvasHeight);
            }
            EllipseMethods.UpdateCanvasPosition(_people);
            _covidModel.CheckInfections(_people);
            

            // Перерисовка холста основной зоны
            for (int i = 0; i < _people.Count; i++)
            {
                var person = _people[i];
                var personEllipse = EllipseMethods._personEllipses[i];

                Canvas.SetLeft(personEllipse, person.Coordinates.X - EllipseMethods.ellipseSize / 2);
                Canvas.SetTop(personEllipse, person.Coordinates.Y - EllipseMethods.ellipseSize / 2);

                // Обновление цвета в зависимости от статуса
                if (person.Status == CovidStatus.Infected)
                {
                    personEllipse.Fill = Brushes.Red;

                    // Создаем и добавляем радиус, если его нет
                    var infectionRadiusEllipse = EllipseMethods._infectionEllipses.FirstOrDefault(e => (int)e.Tag == person.Id);
                    if (infectionRadiusEllipse == null)
                    {
                        EllipseMethods.CreateInfectionRadiusEllipse(person, _covidModel.InfectRadius);
                        infectionRadiusEllipse = EllipseMethods._infectionEllipses.Last(); // Получение созданного радиуса
                        SimulationPole.Children.Add(infectionRadiusEllipse);
                    }

                    // Обновление положения радиуса
                    EllipseMethods.UpdateInfectionRadiusEllipse(person, _covidModel.InfectRadius);
                }
                else if (person.Status == CovidStatus.Recovered)
                {
                    personEllipse.Fill = Brushes.Gray;
                    RemoveInfectionRadiusEllipse(person);
                }
                else if (person.Status == CovidStatus.Dead)
                {
                    personEllipse.Fill = Brushes.Purple;
                    RemoveInfectionRadiusEllipse(person);
                }
                else
                {
                    personEllipse.Fill = Brushes.LightBlue;
                }
            }

            // Обновление количества зараженных (active cases)
            ActiveCases = _people.Count(p => p.Status == CovidStatus.Infected);

            // Обновление количества людей в каждой группе
            var susceptible = _people.Count(p => p.Status == CovidStatus.Susceptible);
            var infected = _people.Count(p => p.Status == CovidStatus.Infected) ;
            var recovered = _people.Count(p => p.Status == CovidStatus.Recovered);
            var dead = _people.Count(p => p.Status == CovidStatus.Dead);

            // Добавление новых данных в ObservableCollection
            SusceptibleSpisok.Add(susceptible);
            InfectedSpisok.Add(infected);
            RecoveredSpisok.Add(recovered);
            DeadSpisok.Add(dead);
        }

        public void SetParameters(int populationSize, double r, double infectChance)
        {
            _covidModel.PopulationSize = populationSize;
            _covidModel.InfectRadius = r;
            _covidModel.InfectChance = infectChance;
        }

        private async void StartSimulation_ButtonClick(object sender, RoutedEventArgs e)
        {
            EnterParameters enter = new EnterParameters(this);
            var result = await enter.ShowDialog<bool>(this);
            if (result)
            {
                R = _covidModel.InfectRadius;
            }

            _covidModel.CreatePopulation();
            _people = _covidModel.People.ToList();

            EllipseMethods.CreateEllipsesForPeople(_people);

            foreach (var ellipse in EllipseMethods._personEllipses)
            {
                SimulationPole.Children.Add(ellipse);
            }

            foreach (var ellipse in EllipseMethods._infectionEllipses)
            {
                SimulationPole.Children.Add(ellipse);
            }

            _covidModel.Start();
            _timer.Start();
            _dayTimer.Start();
        }

        private void StopSimulation_ButtonClick(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            _dayTimer.Stop();

            Days = 0;
            EllipseMethods._infectionEllipses.Clear();
            EllipseMethods._personEllipses.Clear();

            _covidModel.InfectChance = 0;
            _covidModel.InfectRadius = 0;

            _covidModel.People.Clear();
            _people.Clear();

            SimulationPole.Children.Clear();

            R = 0;
            ActiveCases = 0;

            SusceptibleSpisok.Clear();
            InfectedSpisok.Clear();
            RecoveredSpisok.Clear();
            DeadSpisok.Clear();
        }

        private void RemoveInfectionRadiusEllipse(Person person) // Удаление эллипса радиуса из Canvas и из списка
        {
            var removeRadiusEllipse = EllipseMethods._infectionEllipses.FirstOrDefault(e => (int)e.Tag == person.Id);
            if (removeRadiusEllipse != null)
            {
                SimulationPole.Children.Remove(removeRadiusEllipse);
                EllipseMethods._infectionEllipses.Remove(removeRadiusEllipse);
            }
        }
    }
}
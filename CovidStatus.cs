using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CovidSimulation
{
    public enum CovidStatus
    {
        Susceptible, // Уязвимые
        Infected, // Зараженные
        Recovered, // Выздоровевшие
        Dead // Умершие
    }
}
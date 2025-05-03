using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;

namespace CovidSimulation;

public partial class EnterParameters : Window
{
    private int _populationSize;
    private double _r;
    private double _infectChance;

    private MainWindow _mainWindow;
    public EnterParameters()
    {
        InitializeComponent();
    }

    public EnterParameters(MainWindow mainWindow)
    {
        InitializeComponent();
        _mainWindow = mainWindow;
    }

    private void Confirm_ButtonClick(object sender, RoutedEventArgs e)
    {
        if (_infectChance > 0 && !string.IsNullOrEmpty(populationTextBox.Text) && !string.IsNullOrEmpty(infectionRadiusTextBox.Text))
        {
            _populationSize = Convert.ToInt32(populationTextBox.Text);
            _r = Convert.ToDouble(infectionRadiusTextBox.Text);

            _mainWindow.SetParameters(_populationSize, _r, _infectChance);
            this.Close(true);
        } 
    }

    private void SelectInfectionChance_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ComboBox combo = sender as ComboBox;
        if (combo != null)
        {
            ComboBoxItem item = combo.SelectedItem as ComboBoxItem;
            if (item != null)
            {
                string chance = item.Content.ToString();
                _infectChance = Convert.ToDouble(chance) / 100;
            }
        }
    }
}
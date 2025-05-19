using System.Windows;
using System.Windows.Controls;

namespace Forza_DB_Editor
{
    public partial class EngineSwapModal : Window
    {
        public List<Car> CarList { get; set; }
        public Car SelectedCar { get; set; }
        public List<EngineSwap> AllEngineSwaps { get; set; }
        private List<EngineSwap> uniqueEngines;
        private bool isProgrammaticallySettingText = false;
        public EngineSwapModal()
        {
            InitializeComponent();
            Loaded += EngineSwapModal_Loaded;
        }


        private void EngineSwapModal_Loaded(object sender, RoutedEventArgs e)
        {
            CarComboBox.ItemsSource = CarList;
            foreach (var car in CarList)
            {
                System.Diagnostics.Debug.WriteLine($"[CarList] Car ID: {car.Id}, Name: {car.FullName}");
            }

            if (SelectedCar != null)
            {
                // Find and assign the same reference from the list
                var match = CarList.FirstOrDefault(c => c.Id == SelectedCar?.Id);

                if (match != null)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        CarComboBox.SelectedItem = match;
                    }), System.Windows.Threading.DispatcherPriority.Loaded);
                    System.Diagnostics.Debug.WriteLine($"[Match Found] Assigned Car ID: {match.Id}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[No Match] Could not find selected car in CarList");
                }
            }

            System.Diagnostics.Debug.WriteLine($"[Modal Load] SelectedCar ID: {SelectedCar?.Id}, Name: {SelectedCar?.FullName}");


            try
            {
                CarComboBox.ItemsSource = CarList;
                CarComboBox.SelectedItem = SelectedCar;

                uniqueEngines = AllEngineSwaps
                    .GroupBy(e => e.EngineID)
                    .Select(g => g.First())
                    .OrderBy(e => e.EngineName)
                    .ToList();

                EngineListBox.ItemsSource = uniqueEngines;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during modal load: {ex.Message}", "Modal Load Error");
            }
        }


        private void EngineSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isProgrammaticallySettingText || uniqueEngines == null)
                return;

            string search = EngineSearchBox.Text.Trim();

            var filtered = uniqueEngines
                .Where(e => !string.IsNullOrEmpty(e.EngineName) &&
                            e.EngineName.Contains(search, StringComparison.OrdinalIgnoreCase))
                .ToList();

            EngineListBox.ItemsSource = filtered;
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            // Placeholder logic
            MessageBox.Show("Submit clicked — SQL functionality coming soon.");
            this.DialogResult = true; // or just: this.Close();
        }

        private void EngineListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EngineListBox.SelectedItem is EngineSwap selectedEngine)
            {
                isProgrammaticallySettingText = true;
                EngineSearchBox.Text = selectedEngine.EngineName;
                isProgrammaticallySettingText = false;
            }
        }


    }
}
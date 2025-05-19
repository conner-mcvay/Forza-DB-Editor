using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
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
        public SQLiteConnection Connection { get; set; }
        public int? InsertedCarID { get; private set; }
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
            if (CarComboBox.SelectedItem is not Car selectedCar ||
                EngineListBox.SelectedItem is not EngineSwap selectedEngine)
            {
                MessageBox.Show("Please select both a car and an engine.", "Missing Info", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(PriceTextBox.Text.Trim(), out int price))
            {
                MessageBox.Show("Please enter a valid price.", "Invalid Price", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (Connection.State != ConnectionState.Open)
                    Connection.Open();

                // use `Connection` instead of creating a new one
                string getIdSql = File.ReadAllText("Queries/GetNextEngineSwapID.sql");
                using var getIdCmd = new SQLiteCommand(getIdSql, Connection);
                getIdCmd.Parameters.AddWithValue("@CarID", selectedCar.Id);
                var upgradeEngineId = Convert.ToInt32(getIdCmd.ExecuteScalar());
                // get level
                string getLevelSql = File.ReadAllText("Queries/GetNextEngineLevel.sql");

                using var getLevelCmd = new SQLiteCommand(getLevelSql, Connection);
                getLevelCmd.Parameters.AddWithValue("@CarID", selectedCar.Id);

                int nextLevel = Convert.ToInt32(getLevelCmd.ExecuteScalar());

                // 2. Insert new engine swap
                string insertSql = File.ReadAllText("Queries/CreateEngineSwap.sql");
                using var insertCmd = new SQLiteCommand(insertSql, Connection);
                insertCmd.Parameters.AddWithValue("@UpgradeEngineID", upgradeEngineId);
                insertCmd.Parameters.AddWithValue("@CarID", selectedCar.Id);
                insertCmd.Parameters.AddWithValue("@Level", nextLevel);
                insertCmd.Parameters.AddWithValue("@EngineID", selectedEngine.EngineID);
                insertCmd.Parameters.AddWithValue("@IsStock", 0);
                insertCmd.Parameters.AddWithValue("@ManufacturerID", 0); // update if needed
                insertCmd.Parameters.AddWithValue("@Price", price);

                insertCmd.ExecuteNonQuery();

                MessageBox.Show("Engine swap successfully added!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);


                InsertedCarID = selectedCar.Id;
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to add engine swap: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
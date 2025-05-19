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
    public partial class MainWindow : Window
    {
        private AppConfig config;
        private string currentFilePath;
        private SQLiteConnection currentConnection;
        
        private List<Car> carList = new();
        private List<EngineSwap> allEngineSwaps = new();
        private bool carsLoaded = false;
        private bool engineSwapsLoaded = false;
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        public MainWindow()
        {
            InitializeComponent();
            config = AppConfig.Load();

            /*
            MessageBox.Show("Forza DB Editor will back up your selected database file before making any edits, but you are " +
                "STRONGLY ENCOURAGED to create an additional backup on your own before making any edits. Invalid db files WILL BREAK " +
                "THE GAME.", "Caution", MessageBoxButton.OK, MessageBoxImage.Warning);
            */

            if (string.IsNullOrEmpty(config.LastFilePath) || !File.Exists(config.LastFilePath))
            {
                PromptToOpenFile();
            }
            else
            {
                OpenDatabase(config.LastFilePath); // no popup
            }

            // Try to load saved file
            if (string.IsNullOrEmpty(config.LastFilePath) || !File.Exists(config.LastFilePath))
            {
                PromptToOpenFile();
            }
            else
            {
                OpenDatabase(config.LastFilePath);
            }
        }

        private void LoadCars(SQLiteConnection conn)
        {
            carList.Clear();

            string query = LoadSqlQuery("Queries/GetCars.sql");

            using var cmd = new SQLiteCommand(query, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                carList.Add(new Car
                {
                    Id = reader.GetInt32(0),
                    Year = reader.GetInt32(1),
                    Make = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Model = reader.GetString(3),
                    FullName = reader.GetString(4),
                    FrontWheelDiameterIN = reader.GetInt32(5),
                    FrontTireAspect = reader.GetInt32(6),
                    ModelFrontTrackOuter = reader.IsDBNull(7) ? 0.0 : reader.GetDouble(7),
                    RearWheelDiameterIN = reader.GetInt32(8),
                    RearTireAspect = reader.GetInt32(9),
                    ModelRearTrackOuter = reader.IsDBNull(10) ? 0.0 : reader.GetDouble(10),
                });
            }

            CarListBox.ItemsSource = carList.OrderBy(c => c.FullName).ToList();
        }

        private void PromptToOpenFile()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "SQLite files (*.slt)|*.slt|All files (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                OpenDatabase(dialog.FileName, showMessage: true);
            }
        }

        private void OpenDatabase(string filePath, bool showMessage = false)
        {
            try
            {
                currentConnection = new SQLiteConnection($"Data Source={filePath};Version=3;");
                currentConnection.Open();

                using var cmd = new SQLiteCommand("SELECT MIN(Id) FROM Data_Car;", currentConnection);
                var result = cmd.ExecuteScalar();

                currentFilePath = filePath;
                config.LastFilePath = filePath;
                config.Save();

                this.Title = $"Forza DB Editor - {filePath}";
                if (showMessage)
                {
                    MessageBox.Show("Database successfully opened", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening database: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                PromptToOpenFile();
            }
        }

        private void SelectFile_Click(object sender, RoutedEventArgs e)
        {
            PromptToOpenFile();
        }

        private void MainTabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (MainTabControl.SelectedItem == CarTab && !carsLoaded && currentConnection != null)
            {
                LoadCars(currentConnection);
                LoadEngineSwaps(currentConnection);
                carsLoaded = true;
            }
        }

        private void CarListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CarListBox.SelectedItem is Car selectedCar)
            {
                YearText.Text = selectedCar.Year.ToString();
                MakeText.Text = selectedCar.Make.ToString();
                ModelText.Text = selectedCar.Model.ToString();
                FrontWheelDiameterText.Text = selectedCar.FrontWheelDiameterIN.ToString();
                FrontTireAspectText.Text = selectedCar.FrontTireAspect.ToString();
                RearWheelDiameterText.Text = selectedCar.RearWheelDiameterIN.ToString();
                RearTireAspectText.Text = selectedCar.RearTireAspect.ToString();
                ModelFrontTrackOuter.Text = selectedCar.ModelFrontTrackOuter.ToString();
                ModelRearTrackOuter.Text = selectedCar.ModelRearTrackOuter.ToString();

                // Hide engine swaps until the button is clicked
                EngineSwapsPanel.Visibility = Visibility.Collapsed;
                ViewEngineSwapsButton.Content = "View/Edit Engine Swaps";
            }
        }

        private void CarSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CarListBox == null || carList == null) return;

            string search = CarSearchBox.Text.Trim();

            var filtered = carList
                .Where(c => !string.IsNullOrEmpty(c.FullName) &&
                            c.FullName.Contains(search, StringComparison.OrdinalIgnoreCase))
                .OrderBy(c => c.FullName)
                .ToList();

            CarListBox.ItemsSource = filtered;
        }

        private string LoadSqlQuery(string relativePath)
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string fullPath = Path.Combine(basePath, relativePath);
            return File.ReadAllText(fullPath);
        }

        private void LoadEngineSwaps(SQLiteConnection conn)
        {
            allEngineSwaps.Clear();

            string query = LoadSqlQuery("Queries/GetEngineSwaps.sql");

            using var cmd = new SQLiteCommand(query, conn);
            using var reader = cmd.ExecuteReader();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                System.Diagnostics.Debug.WriteLine($"[{i}] {reader.GetName(i)}");
            }

            while (reader.Read())
            {
                allEngineSwaps.Add(new EngineSwap
                {
                    UpgradeEngineID = reader.GetInt32(0),
                    CarID = reader.GetInt32(1),
                    CarName = reader.GetString(2),
                    EngineID = reader.IsDBNull(3) ? 0 : Convert.ToInt32(reader["EngineID"]),
                    EngineName = reader.GetString(4),
                    Level = reader.GetInt32(5),
                    IsStock = reader.GetBoolean(6),
                    Price = reader.GetInt32(7)
                });
            }

            System.Diagnostics.Debug.WriteLine($"Loaded {allEngineSwaps.Count} engine swaps.");
            engineSwapsLoaded = true;
        }

        public void RefreshEngineSwapsForCar(int carId)
        {
            var filteredSwaps = allEngineSwaps
                .Where(es => es.CarID == carId)
                .ToList();

            EngineSwapsGrid.ItemsSource = filteredSwaps;
            EngineSwapsGrid.Items.Refresh();
        }

        private void ViewEngineSwaps_Click(object sender, RoutedEventArgs e)
        {
            if (CarListBox.SelectedItem is not Car selectedCar)
            {
                MessageBox.Show("Please select a car first.", "No Car Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Toggle collapse
            if (EngineSwapsPanel.Visibility == Visibility.Visible)
            {
                EngineSwapsPanel.Visibility = Visibility.Collapsed;
                ViewEngineSwapsButton.Content = "View/Edit Engine Swaps"; // <-- set collapsed label
                return;
            }

            // Load once
            if (!engineSwapsLoaded)
            {
                try
                {
                    LoadEngineSwaps(currentConnection);
                    engineSwapsLoaded = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading engine swaps: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            // Refresh and show
            RefreshEngineSwapsForCar(selectedCar.Id);
            EngineSwapsPanel.Visibility = Visibility.Visible;
            ViewEngineSwapsButton.Content = "Hide Engine Swaps"; // <-- set expanded label
        }

        private void AddEngineSwap_Click(object sender, RoutedEventArgs e)
        {
            var selectedCar = (Car)CarListBox.SelectedItem;
            var selectedCarInList = carList.FirstOrDefault(c => c.Id == selectedCar.Id);

            var modal = new EngineSwapModal
            {
                Owner = this,
                CarList = carList,
                SelectedCar = selectedCarInList,
                AllEngineSwaps = allEngineSwaps,
                Connection = currentConnection
            };

            bool? result = modal.ShowDialog();

            if (result == true && modal.InsertedCarID.HasValue)
            {
                LoadEngineSwaps(currentConnection);
                RefreshEngineSwapsForCar(modal.InsertedCarID.Value);
                EngineSwapsPanel.Visibility = Visibility.Visible;
                ViewEngineSwapsButton.Content = "Hide Engine Swaps";
            }
        }

        private void DebugForceShow_Click(object sender, RoutedEventArgs e)
        {
            EngineSwapsPanel.Visibility = Visibility.Visible;

            EngineSwapsGrid.ItemsSource = new List<object>
    {
        new { EngineName = "Test Engine", Level = 3, IsStock = false, Price = 5000 }
    };

            EngineSwapsGrid.Items.Refresh();
        }
    }
}

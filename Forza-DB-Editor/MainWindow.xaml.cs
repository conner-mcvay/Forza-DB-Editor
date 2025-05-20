using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using static Forza_DB_Editor.TurboUpgradeModal;

namespace Forza_DB_Editor
{
    public partial class MainWindow : Window
    {
        bool dev = Environment.GetEnvironmentVariable("env") == "dev"; 
        //----------------//
        //  constructors  // 
        //----------------//
        private AppConfig config;
        private SQLiteConnection currentConnection;
        
        private List<Car> carList = new();
        private List<EngineSwap> allEngineSwaps = new();
        private List<Engine> engineList = new();
        private List<Turbo> singleTurboUpgrades = new();
        
        private bool carsLoaded = false;
        private bool engineSwapsLoaded = false;
        private bool singleTurboLoaded = false;

        public MainWindow()
        {
            InitializeComponent();
            config = AppConfig.Load();

            if (!dev)
            {
                MessageBox.Show("Forza DB Editor will back up your selected database file before making any edits, but you are " +
                    "STRONGLY ENCOURAGED to create an additional backup on your own before making any edits. Invalid db files WILL BREAK " +
                    "THE GAME AND POSSIBLY CORRUPT YOUR PROFILE.", "Caution", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            if (string.IsNullOrEmpty(config.LastFilePath) || !File.Exists(config.LastFilePath))
            {
                PromptToOpenFile();
            }
            else
            {
                OpenDatabase(config.LastFilePath); // no popup
                BackupDatabaseFile(config.LastFilePath); // no popup
                
            }

            // Try to load saved file
            if (string.IsNullOrEmpty(config.LastFilePath) || !File.Exists(config.LastFilePath))
            {
                PromptToOpenFile();
            }
            else
            {
                OpenDatabase(config.LastFilePath);
                BackupDatabaseFile(config.LastFilePath); // no popup

            }
        }

        //------------------// 
        //  util functions  //
        //------------------//
        private void PromptToOpenFile()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "SQLite files (*.slt)|*.slt|All files (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                BackupDatabaseFile(dialog.FileName);
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

        private void BackupDatabaseFile(string originalPath)
        {
            try
            {
                string exeDir = AppDomain.CurrentDomain.BaseDirectory;
                string backupDir = Path.Combine(exeDir, "backup");

                if (!Directory.Exists(backupDir))
                    Directory.CreateDirectory(backupDir);

                string fileName = Path.GetFileNameWithoutExtension(originalPath);
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string destPath = Path.Combine(backupDir, $"{fileName}_{timestamp}.slt");

                if (!dev)
                {
                    File.Copy(originalPath, destPath, overwrite: true);
                }
                System.Diagnostics.Debug.WriteLine($"Backup created at: {destPath}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to create backup: {ex.Message}", "Backup Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SelectFile_Click(object sender, RoutedEventArgs e)
        {
            PromptToOpenFile();
        }

        private string LoadSqlQuery(string relativePath)
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string fullPath = Path.Combine(basePath, relativePath);
            return File.ReadAllText(fullPath);
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        //---------------------//
        //  Car tab functions  //
        //---------------------//

        private void LoadCars(SQLiteConnection conn)
        {
            carList.Clear();

            string query = LoadSqlQuery("Queries/Car_GetCars.sql");

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


        private void MainTabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (MainTabControl.SelectedItem == CarTab && !carsLoaded && currentConnection != null)
            {
                LoadCars(currentConnection);
                LoadEngineSwaps(currentConnection);
                carsLoaded = true;
            }

            if (EngineTab.IsSelected && engineList.Count == 0)
            {
                LoadEngines(currentConnection);
                EngineListBox.ItemsSource = engineList.OrderBy(c => c.EngineName).ToList();
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


        private void LoadEngineSwaps(SQLiteConnection conn)
        {
            allEngineSwaps.Clear();

            string query = LoadSqlQuery("Queries/Car_GetEngineSwaps.sql");

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

        //-------------------------//
        //  Engines tab functions  // 
        //-------------------------//

        private void LoadEngines(SQLiteConnection conn)
        {
            engineList.Clear();

            string query = LoadSqlQuery("Queries/Engine_GetEngines.sql"); 

            using var cmd = new SQLiteCommand(query, conn);
            using var reader = cmd.ExecuteReader();

            var seen = new HashSet<int>();

            while (reader.Read())
            {
                int engineId = reader.GetInt32(0);
                string engineName = reader.GetString(1);

                if (!seen.Contains(engineId))
                {
                    engineList.Add(new Engine
                    {
                        EngineID = engineId,
                        EngineName = engineName
                    });

                    seen.Add(engineId);
                }
            }
        }

        private void EngineSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string search = EngineSearchBox.Text.Trim();

            var filtered = engineList
                .Where(e => e.EngineName.Contains(search, StringComparison.OrdinalIgnoreCase))
                .OrderBy(e => e.EngineName)
                .ToList();

            EngineListBox.ItemsSource = filtered;
        }

        private void EngineListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Collapse turbo view if open
            if (SingleTurboPanel.Visibility == Visibility.Visible)
            {
                SingleTurboPanel.Visibility = Visibility.Collapsed;
                ViewSingleTurboButton.Content = "View/Edit Single Turbo Upgrades";
            }

            if (EngineListBox.SelectedItem is not Engine selected)
                return;

            EngineIDText.Text = selected.EngineID.ToString();
            EngineNameText.Text = selected.EngineName;
        }

        private void LoadSingleTurboUpgrades(int engineId)
        {
            singleTurboUpgrades.Clear();

            string sql = LoadSqlQuery("Queries/Engine_SingleTurbo_GetSingleTurboUpgrades.sql");

            using var cmd = new SQLiteCommand(sql, currentConnection);
            cmd.Parameters.AddWithValue("@EngineID", engineId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                singleTurboUpgrades.Add(new Turbo
                {
                    EngineID = engineId,
                    ManufacturerID = reader.GetInt32(1),
                    Level = reader.GetInt32(2),
                    Price = reader.GetInt32(3),
                    MinScale = reader.GetDouble(4),
                    PowerMinScale = reader.GetDouble(5),
                    MaxScale = reader.GetDouble(6),
                    PowerMaxScale = reader.GetDouble(7),
                    RobScale = reader.GetDouble(8)
                });
            }

            SingleTurboGrid.ItemsSource = null;
            SingleTurboGrid.Items.Clear(); // optional, defensive
            SingleTurboGrid.ItemsSource = singleTurboUpgrades;
            SingleTurboGrid.Items.Refresh();
        }

        private void ViewSingleTurbo_Click(object sender, RoutedEventArgs e)
        {
            if (EngineListBox.SelectedItem is not Engine selectedEngine)
            {
                MessageBox.Show("Please select an engine first.", "No Engine Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Toggle logic
            if (SingleTurboPanel.Visibility == Visibility.Visible)
            {
                SingleTurboPanel.Visibility = Visibility.Collapsed;
                AddSingleTurboButton.Visibility = Visibility.Collapsed;
                ViewSingleTurboButton.Content = "View/Edit Single Turbo Upgrades";
                return;
            }

            // Load and show
            LoadSingleTurboUpgrades(selectedEngine.EngineID);
            SingleTurboPanel.Visibility = Visibility.Visible;
            AddSingleTurboButton.Visibility = Visibility.Visible;
            ViewSingleTurboButton.Content = "Hide Single Turbo Upgrades";
        }

        private void AddSingleTurbo_Click(object sender, RoutedEventArgs e)
        {
            if (EngineListBox.SelectedItem is not Engine selectedEngine)
                return;

            var modal = new TurboUpgradeModal
            {
                Owner = this,
                Connection = currentConnection,
                EngineList = engineList,
                SelectedEngine = (Engine)EngineListBox.SelectedItem,
                Mode = TurboUpgradeType.Single // or Twin
            };

            bool? result = modal.ShowDialog();

            if (result == true && modal.InsertSucceeded)
            {
                LoadSingleTurboUpgrades(modal.SelectedEngine.EngineID);
            }
        }
        
    }
}

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
    /// <summary>
    /// Interaction logic for TurboUpgradeModal.xaml
    /// </summary>
    public partial class TurboUpgradeModal : Window
    {
        public List<Engine> EngineList { get; set; }
        public Engine SelectedEngine { get; set; }
        public SQLiteConnection Connection { get; set; }

        public bool InsertSucceeded { get; private set; } = false;
        public TurboUpgradeModal()
        {
            InitializeComponent();
        }

        public enum TurboUpgradeType
        {
            Single,
            Twin
        }

        public TurboUpgradeType Mode { get; set; }

        public int GetNextTurboId(string tableName, SQLiteConnection conn)
        {
            string sql = $"SELECT COALESCE(MAX(Id), 0) + 1 FROM {tableName}";
            using var cmd = new SQLiteCommand(sql, conn);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public string GetNextLevelQuery => Mode switch
        {
            TurboUpgradeType.Single => "Queries/Engine_SingleTurbo_GetNextLevel.sql",
            TurboUpgradeType.Twin => "Queries/Engine_TwinTurbo_GetNextLevel.sql",
            _ => throw new InvalidOperationException("Unsupported turbo mode")
        };

        public string InsertQuery => Mode switch
        {
            TurboUpgradeType.Single => "Queries/Engine_SingleTurbo_CreateNewSingleTurboUpgrade.sql",
            TurboUpgradeType.Twin => "Queries/Engine_TwinTurbo_CreateNewTwinTurboUpgrade.sql",
            _ => throw new InvalidOperationException("Unsupported turbo mode")
        };

        public string UpdateQuery => Mode switch
        {
            TurboUpgradeType.Single => "Queries/Engine_SingleTurbo_UpdateExistingSingleTurboUpgrade.sql",
            TurboUpgradeType.Twin => "Queries/Engine_TwinTurbo_UpdateExistingTwinTurboUpgrade.sql",
            _ => throw new InvalidOperationException("Unsupported turbo mode")
        };

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            EngineComboBox.ItemsSource = EngineList;

            if (SelectedEngine != null)
            {
                var match = EngineList.FirstOrDefault(e => e.EngineID == SelectedEngine.EngineID);
                if (match != null)
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        EngineComboBox.SelectedItem = match;
                    }), System.Windows.Threading.DispatcherPriority.Loaded); ;
            }
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            if (EngineComboBox.SelectedItem is not Engine engine)
                return;

            double minScale, powerMin, maxScale, powerMax, robScale = 0;
            int price = 0;


            try
            {
                minScale = double.Parse(MinScaleBox.Text);
                powerMin = double.Parse(PowerMinScaleBox.Text);
                maxScale = double.Parse(MaxScaleBox.Text);
                powerMax = double.Parse(PowerMaxScaleBox.Text);
                robScale = double.Parse(RobScaleBox.Text);
                price = int.Parse(PriceBox.Text);
            }
            catch (Exception)
            {
                MessageBox.Show("Failed to parse inputs, all inputs should be decimal numbers.", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            

            string tableName = Mode switch
            {
                TurboUpgradeType.Single => "List_UpgradeEngineTurboSingle",
                TurboUpgradeType.Twin => "List_UpgradeEngineTurboTwin",
                _ => throw new InvalidOperationException("Unsupported turbo type")
            };

            int nextId = GetNextTurboId(tableName, Connection);

            // Get next Level and ManufacturerID
            string nextQuery = File.ReadAllText(GetNextLevelQuery);
            using var cmd = new SQLiteCommand(nextQuery, Connection);
            cmd.Parameters.AddWithValue("@EngineID", engine.EngineID);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
            {
                MessageBox.Show("Failed to get next level.");
                return;
            }

            string selectedLevelText = ((ComboBoxItem)LevelComboBox.SelectedItem).Content.ToString();
            int selectedLevel = selectedLevelText switch
            {
                "Street" => 1,
                "Sport" => 2,
                "Race" => 3,
                _ => throw new InvalidOperationException("Unknown turbo level")
            };
            int manufacturerId = 0;

            bool rowExists = false;
            string checkSql = @"
                SELECT 1 FROM List_UpgradeEngineTurboSingle
                WHERE EngineID = @EngineID AND Level = @Level
                LIMIT 1";

            using (var checkCmd = new SQLiteCommand(checkSql, Connection))
            {
                checkCmd.Parameters.AddWithValue("@EngineID", engine.EngineID);
                checkCmd.Parameters.AddWithValue("@Level", selectedLevel);

                using var checkReader = checkCmd.ExecuteReader();
                rowExists = checkReader.Read(); // returns true if any row found
            }

            if (rowExists) // row found, update existing turbo for given Level
            {
                string updateSql = File.ReadAllText(UpdateQuery);
                using var updateCmd = new SQLiteCommand(updateSql, Connection);
                updateCmd.Parameters.AddWithValue("@EngineID", engine.EngineID);
                updateCmd.Parameters.AddWithValue("@Level", selectedLevel);
                updateCmd.Parameters.AddWithValue("@Price", price);
                updateCmd.Parameters.AddWithValue("@MaxScale", maxScale);
                updateCmd.Parameters.AddWithValue("@PowerMaxScale", powerMax);
                updateCmd.Parameters.AddWithValue("@MinScale", minScale);
                updateCmd.Parameters.AddWithValue("@PowerMinScale", powerMin);
                updateCmd.Parameters.AddWithValue("@RobScale", robScale);

                updateCmd.ExecuteNonQuery();
            }
            else // Insert new turbo
            {                
                string insertSql = File.ReadAllText(InsertQuery);
                using var insertCmd = new SQLiteCommand(insertSql, Connection);
                insertCmd.Parameters.AddWithValue("@Id", nextId);
                insertCmd.Parameters.AddWithValue("@EngineID", engine.EngineID);
                insertCmd.Parameters.AddWithValue("@NextLevel", selectedLevel);
                insertCmd.Parameters.AddWithValue("@ManufacturerId", manufacturerId);
                insertCmd.Parameters.AddWithValue("@Price", price);
                insertCmd.Parameters.AddWithValue("@MaxScale", maxScale);
                insertCmd.Parameters.AddWithValue("@PowerMaxScale", powerMax);
                insertCmd.Parameters.AddWithValue("@MinScale", minScale);
                insertCmd.Parameters.AddWithValue("@PowerMinScale", powerMin);
                insertCmd.Parameters.AddWithValue("@RobScale", robScale);

                insertCmd.ExecuteNonQuery();
            }

            InsertSucceeded = true;
            this.DialogResult = true;

            MessageBox.Show("Turbo upgrade added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            this.Close();
        }
    }
}

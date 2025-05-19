using System.IO;
using System.Text.Json;

public class AppConfig
{
    public string LastFilePath { get; set; }

    private static readonly string configPath = "config.json";

    public static AppConfig Load()
    {
        if (File.Exists(configPath))
        {
            string json = File.ReadAllText(configPath);
            return JsonSerializer.Deserialize<AppConfig>(json);
        }
        return new AppConfig();
    }

    public void Save()
    {
        string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(configPath, json);
    }
}
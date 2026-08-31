using System;
using System.IO;
using System.Text.Json;

namespace P2P_test.Models.Models;

public class Settings
{
    public bool DeveloperMode { get; set; }

    public Settings()
    {
    }

    public bool LoadSettings()
    {
        try
        {
            Settings deserialized = JsonSerializer.Deserialize<Settings>(File.ReadAllText("appsettings.json"));
            
            DeveloperMode = deserialized.DeveloperMode;

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            DeveloperMode = false;
            
            File.WriteAllText("appsettings.json", JsonSerializer.Serialize(this));

            return false;
        }
    }
    
    public bool SaveSettings()
    {
        try
        {
            File.WriteAllText("appsettings.json", JsonSerializer.Serialize(this));
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }
    
}
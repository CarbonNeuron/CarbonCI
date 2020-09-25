﻿using Config.Net;
namespace CarbonCI
{
    public static class Settings
    {
        //Global persistent settings that are saved to a json file. Limited Types
        public static IPersistentSettings PSettings = new ConfigurationBuilder<IPersistentSettings>().UseJsonFile("settings.json").Build();
    }


    public interface IPersistentSettings
    {
        //Types allowed: bool, double, int, long, string, TimeSpan, DateTime, Uri, Guid
        //DateTime is always converted to UTC
        [Option(Alias = "SuperSecretStuff", DefaultValue = "Test")]
        string PValue { get; set; }
    }
}
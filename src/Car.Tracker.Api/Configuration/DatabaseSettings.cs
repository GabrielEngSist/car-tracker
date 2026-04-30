using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Car.Tracker.Api.Configuration;

public class DatabaseSettings
{
    public const string SectionName = "CarTracker";
    public string? Host {get;set;}
    public string? Database {get; set;}
    public string? Username {get; set;}
    public string? Password {get; set;}
    public string? Port {get; set;}
    public string? Schema {get; set;}
}

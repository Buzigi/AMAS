using System;
using Microsoft.Extensions.Configuration;

namespace MedSched.Api.Converters;

public static class ConnectionStringConverter
{
    public static string ConvertDatabaseUrlToConnectionString(string databaseUrl)
    {
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':');
        var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        var defaultPort = configuration.GetValue<int>("DefaultPostgresPort");
        var port = uri.IsDefaultPort ? defaultPort : uri.Port;
        return $"Host={uri.Host};Port={port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
    }
}

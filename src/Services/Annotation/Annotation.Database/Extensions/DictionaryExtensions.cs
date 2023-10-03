using PreciPoint.Ims.Services.Annotation.Application.Configuration;
using System;
using System.Collections;

namespace PreciPoint.Ims.Services.Annotation.Database.Extensions;

internal static class DictionaryExtensions
{
    internal static PostgreSqlConfig ToPostgreSqlConfig(this IDictionary environmentVariables)
    {
        return new PostgreSqlConfig
        {
            Database = environmentVariables.EnvironmentVariableToString("POSTGRES_DB"),
            Host = environmentVariables.EnvironmentVariableToString("POSTGRES_SERVER"),
            Username = environmentVariables.EnvironmentVariableToString("POSTGRES_USER"),
            Password = environmentVariables.EnvironmentVariableToString("POSTGRES_PASSWORD"),
            Port = environmentVariables.EnvironmentVariableToInt32("PGPORT")
        };
    }

    private static string EnvironmentVariableToString(this IDictionary environmentVariables, string key)
    {
        if (environmentVariables.Contains(key))
        {
            return environmentVariables[key].ToString();
        }

        throw new ArgumentException(
            $"Can't extract string - we miss the required key '{key}' in environment variables.");
    }

    private static int EnvironmentVariableToInt32(this IDictionary environmentVariables, string key)
    {
        if (environmentVariables.Contains(key))
        {
            return Convert.ToInt32(environmentVariables[key]);
        }

        throw new ArgumentException(
            $"Can't extract int - we miss the required key '{key}' in environment variables.");
    }
}
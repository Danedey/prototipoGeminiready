using System;
using Microsoft.Maui.ApplicationModel;

namespace prototipoGeminiready.Services;

public static class AppSettings
{
    public static string CassandraHost =>
        Environment.GetEnvironmentVariable("CASSANDRA_HOST")
        ?? (DeviceInfo.Current.Platform == DevicePlatform.Android ? "10.0.2.2" : "127.0.0.1");

    public static int CassandraPort =>
        int.TryParse(Environment.GetEnvironmentVariable("CASSANDRA_PORT"), out var port)
            ? port
            : 9042;

    public static string CassandraKeyspace =>
        Environment.GetEnvironmentVariable("CASSANDRA_KEYSPACE") ?? "chatbot_fiscal";

    public static string GeminiApiKey =>
        Environment.GetEnvironmentVariable("GEMINI_API_KEY")
        ?? throw new InvalidOperationException(
            "Falta la variable de entorno GEMINI_API_KEY. Configúrala antes de usar Gemini.");

    public static string GeminiEndpoint =>
        $"https://generativelanguage.googleapis.com/v1beta/models/gemini-3.6-flash:generateContent?key={GeminiApiKey}";
}

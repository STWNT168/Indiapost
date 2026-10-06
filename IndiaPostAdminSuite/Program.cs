using System.Text.Json;
using System.Text.Json.Serialization;

namespace IndiaPostAdminSuite;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}

public record AdminRecord(
    string Id,
    string Date,
    string Reference,
    string Subject,
    string Office,
    string Responsible,
    string Status,
    string NextAction,
    string DueDate,
    string Remarks);

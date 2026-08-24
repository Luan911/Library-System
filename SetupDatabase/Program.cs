using Microsoft.Data.SqlClient;
using System.Text.RegularExpressions;

const string server = @"(localdb)\MSSQLLocalDB";
const string masterConnection = $"Server={server};Integrated Security=True;TrustServerCertificate=True;";

string scriptPath = args.Length > 0
    ? args[0]
    : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Khayelitsha_Community_Library_DB.sql"));

if (!File.Exists(scriptPath))
{
    Console.WriteLine($"SQL script not found: {scriptPath}");
    return 1;
}

Console.WriteLine($"Using SQL script: {scriptPath}");
Console.WriteLine($"Target server: {server}");
Console.WriteLine();

string script = File.ReadAllText(scriptPath);
var batches = Regex.Split(script, @"^\s*GO\s*(\r?\n|$)", RegexOptions.Multiline | RegexOptions.IgnoreCase)
    .Select(b => b.Trim())
    .Where(b => !string.IsNullOrWhiteSpace(b))
    .ToList();

using var connection = new SqlConnection(masterConnection);
connection.Open();

int executed = 0;
foreach (var batch in batches)
{
    if (string.IsNullOrWhiteSpace(batch))
        continue;

    using var command = new SqlCommand(batch, connection) { CommandTimeout = 120 };
    try
    {
        command.ExecuteNonQuery();
        executed++;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error in batch {executed + 1}:");
        Console.WriteLine(ex.Message);
        Console.WriteLine();
        Console.WriteLine("Batch preview:");
        Console.WriteLine(batch.Length > 300 ? batch[..300] + "..." : batch);
        return 1;
    }
}

Console.WriteLine($"Database setup complete. Executed {executed} batch(es).");

using var verify = new SqlConnection($"Server={server};Database=KhayelitshaLibraryDB;Integrated Security=True;TrustServerCertificate=True;");
verify.Open();
using var countCmd = new SqlCommand("SELECT COUNT(*) FROM Member", verify);
var memberCount = countCmd.ExecuteScalar();
Console.WriteLine($"Verified: KhayelitshaLibraryDB is ready ({memberCount} members loaded).");
return 0;

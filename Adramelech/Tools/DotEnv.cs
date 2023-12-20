namespace Adramelech.Tools;

public static class DotEnv
{
    public static void Load(string? filePath = null)
    {
        filePath ??= Path.Combine(Directory.GetCurrentDirectory(), ".env");

        if (!File.Exists(filePath))
            return;

        foreach (var line in File.ReadAllLines(filePath))
        {
            // Split just the first '='
            var parts = line.Split('=', 2);
            if (parts.Length != 2)
                continue;

            Format(ref parts[1]);

            Environment.SetEnvironmentVariable(parts[0], parts[1]);
        }
    }

    private static void Format(ref string value)
    {
        // If there is double quotes in the start and end of the value, remove them
        if (value.StartsWith('"') && value.EndsWith('"'))
            value = value[1..^1];
    }
}
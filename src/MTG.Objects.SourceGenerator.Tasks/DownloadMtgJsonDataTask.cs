using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace MTG.Objects.SourceGenerator.Tasks;

/// <summary>
/// MSBuild task that downloads MTGJson data files with ETag-based caching.
/// </summary>
public class DownloadMtgJsonDataTask : Task
{
    private const string EnumValuesUrl = "https://mtgjson.com/api/v5/EnumValues.json";
    private const string SetListUrl = "https://mtgjson.com/api/v5/SetList.json";
    private const int MaxRetries = 3;

    [Required]
    public string CacheDirectory { get; set; } = string.Empty;

    [Output]
    public string EnumValuesPath { get; set; } = string.Empty;

    [Output]
    public string SetListPath { get; set; } = string.Empty;

    public override bool Execute()
    {
        try
        {
            // Ensure cache directory exists
            if (!Directory.Exists(CacheDirectory))
            {
                Directory.CreateDirectory(CacheDirectory);
                Log.LogMessage(MessageImportance.High, $"Created cache directory: {CacheDirectory}");
            }

            // Download EnumValues.json
            EnumValuesPath = EnsureDataFileAsync("EnumValues.json", EnumValuesUrl).GetAwaiter().GetResult();

            // Download SetList.json
            SetListPath = EnsureDataFileAsync("SetList.json", SetListUrl).GetAwaiter().GetResult();

            Log.LogMessage(MessageImportance.High, "MTGJson data files are ready");
            return true;
        }
        catch (Exception ex)
        {
            Log.LogWarning($"Failed to download MTGJson data files: {ex.Message}");

            if (File.Exists(Path.Combine(CacheDirectory, "EnumValues.json")) &&
                File.Exists(Path.Combine(CacheDirectory, "SetList.json")))
            {
                // Set paths to cached files even if download failed
                EnumValuesPath = Path.Combine(CacheDirectory, "EnumValues.json");
                SetListPath = Path.Combine(CacheDirectory, "SetList.json");
                return true;
            }
            else
            {
                Log.LogError("Failed to download MTGJson data files and no cached versions exist.");
                return false;
            }
        }
    }

    private async System.Threading.Tasks.Task<string> EnsureDataFileAsync(string fileName, string url)
    {
        string filePath = Path.Combine(CacheDirectory, fileName);
        string etagPath = Path.Combine(CacheDirectory, $"{fileName}.etag");

        // Check if file exists and has a stored ETag
        if (File.Exists(filePath) && File.Exists(etagPath))
        {
            var storedETag = File.ReadAllText(etagPath).Trim();

            // Validate if the file is still current
            if (await IsFileCurrentAsync(url, storedETag, fileName))
            {
                Log.LogMessage(MessageImportance.High, $"{fileName} is up to date (ETag matched)");
                return filePath;
            }
        }

        // Download the file
        Log.LogMessage(MessageImportance.High, $"Downloading {fileName}...");
        await DownloadFileAsync(url, filePath, etagPath, fileName);
        Log.LogMessage(MessageImportance.High, $"{fileName} downloaded successfully");

        return filePath;
    }

    private async System.Threading.Tasks.Task<bool> IsFileCurrentAsync(string url, string storedETag, string fileName)
    {
        try
        {
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            var request = new HttpRequestMessage(HttpMethod.Head, url);
            request.Headers.Add("If-None-Match", storedETag);

            var response = await httpClient.SendAsync(request);

            // If 304 Not Modified, the file is still current
            if (response.StatusCode == System.Net.HttpStatusCode.NotModified)
            {
                return true;
            }

            // If we get a 200 with the same ETag, file is current
            if (response.IsSuccessStatusCode && response.Headers.ETag != null)
            {
                return response.Headers.ETag.Tag == storedETag;
            }

            return false;
        }
        catch (HttpRequestException ex)
        {
            Log.LogMessage(MessageImportance.High, $"ETag validation failed for {fileName}: {ex.Message}");

            // If the file exists and network check failed, assume it's current
            var filePath = Path.Combine(CacheDirectory, fileName);
            return File.Exists(filePath);
        }
        catch (System.Threading.Tasks.TaskCanceledException)
        {
            Log.LogMessage(MessageImportance.High, $"ETag check timed out for {fileName}");

            // If the file exists and check timed out, assume it's current
            var filePath = Path.Combine(CacheDirectory, fileName);
            return File.Exists(filePath);
        }
    }

    private async System.Threading.Tasks.Task DownloadFileAsync(string url, string filePath, string etagPath, string fileName)
    {
        try
        {
            await ExecuteWithRetryAsync(async () =>
            {
                using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                // Save the file content
                var content = await response.Content.ReadAsStringAsync();
                File.WriteAllText(filePath, content);

                // Save the ETag if present
                if (response.Headers.ETag != null)
                {
                    File.WriteAllText(etagPath, response.Headers.ETag.Tag);
                }
            }, fileName);
        }
        catch (Exception ex)
        {
            // If file exists, log warning and continue
            if (File.Exists(filePath))
            {
                Log.LogMessage(MessageImportance.High, $"Download failed for {fileName}, using cached version: {ex.Message}");
            }
            else
            {
                Log.LogError($"Failed to download {fileName} and no cached version exists: {ex.Message}");
                throw;
            }
        }
    }

    private async System.Threading.Tasks.Task ExecuteWithRetryAsync(Func<System.Threading.Tasks.Task> action, string fileName)
    {
        for (int attempt = 1; attempt <= MaxRetries; attempt++)
        {
            try
            {
                await action();
                return;
            }
            catch (Exception ex) when (attempt < MaxRetries && (ex is HttpRequestException || ex is System.Threading.Tasks.TaskCanceledException))
            {
                var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                Log.LogMessage(MessageImportance.High,
                    $"Retry {attempt} for {fileName} after {delay.TotalSeconds}s delay. Error: {ex.Message}");
                Thread.Sleep(delay);
            }
        }
    }
}

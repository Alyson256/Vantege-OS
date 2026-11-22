using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Bitshift.Models;
using Bitshift.Services.Abstractions;

namespace Bitshift.Services
{
    public sealed class UpdateCheckService : IUpdateCheckService, IDisposable
    {
        private const string Owner = "Alyson256";
        private const string Repo = "Bitshift";
        private const string AppVersion = "2.1.0";

        private readonly HttpClient _httpClient;

        public string CurrentVersion => AppVersion;
        public string RepositoryUrl => $"https://github.com/{Owner}/{Repo}";
        private string ReleasesUrl => $"{RepositoryUrl}/releases";

        public UpdateCheckService()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(8)
            };
            _httpClient.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue("Bitshift", AppVersion));
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        }

        public async Task<UpdateCheckResult> PingAsync(CancellationToken cancellationToken = default)
        {
            var sw = Stopwatch.StartNew();

            try
            {
                string releaseEndpoint = $"https://api.github.com/repos/{Owner}/{Repo}/releases/latest";
                using HttpResponseMessage releaseResponse = await _httpClient.GetAsync(releaseEndpoint, cancellationToken);

                if (releaseResponse.IsSuccessStatusCode)
                {
                    string json = await releaseResponse.Content.ReadAsStringAsync(cancellationToken);
                    sw.Stop();

                    using JsonDocument doc = JsonDocument.Parse(json);
                    string rawTag = doc.RootElement.TryGetProperty("tag_name", out JsonElement tagEl)
                        ? tagEl.GetString() ?? AppVersion
                        : AppVersion;
                    string htmlUrl = doc.RootElement.TryGetProperty("html_url", out JsonElement urlEl)
                        ? urlEl.GetString() ?? ReleasesUrl
                        : ReleasesUrl;

                    string latest = NormalizeVersion(rawTag);
                    bool hasUpdate = CompareSemver(latest, AppVersion) > 0;

                    return new UpdateCheckResult(
                        hasUpdate ? UpdateCheckStatus.UpdateAvailable : UpdateCheckStatus.UpToDate,
                        AppVersion,
                        latest,
                        htmlUrl,
                        Math.Max(1, sw.ElapsedMilliseconds),
                        null);
                }

                // Fallback to tags if repository has no published releases yet
                if (releaseResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    string tagsEndpoint = $"https://api.github.com/repos/{Owner}/{Repo}/tags?per_page=1";
                    using HttpResponseMessage tagsResponse = await _httpClient.GetAsync(tagsEndpoint, cancellationToken);

                    if (tagsResponse.IsSuccessStatusCode)
                    {
                        string tagsJson = await tagsResponse.Content.ReadAsStringAsync(cancellationToken);
                        sw.Stop();

                        using JsonDocument tagsDoc = JsonDocument.Parse(tagsJson);
                        if (tagsDoc.RootElement.ValueKind == JsonValueKind.Array &&
                            tagsDoc.RootElement.GetArrayLength() > 0)
                        {
                            JsonElement firstTag = tagsDoc.RootElement[0];
                            string rawTag = firstTag.TryGetProperty("name", out JsonElement nameEl)
                                ? nameEl.GetString() ?? AppVersion
                                : AppVersion;

                            string latest = NormalizeVersion(rawTag);
                            bool hasUpdate = CompareSemver(latest, AppVersion) > 0;
                            string tagUrl = $"{RepositoryUrl}/releases/tag/{Uri.EscapeDataString(rawTag)}";

                            return new UpdateCheckResult(
                                hasUpdate ? UpdateCheckStatus.UpdateAvailable : UpdateCheckStatus.UpToDate,
                                AppVersion,
                                latest,
                                tagUrl,
                                Math.Max(1, sw.ElapsedMilliseconds),
                                null);
                        }

                        return new UpdateCheckResult(
                            UpdateCheckStatus.UpToDate,
                            AppVersion,
                            AppVersion,
                            ReleasesUrl,
                            Math.Max(1, sw.ElapsedMilliseconds),
                            null);
                    }
                }

                sw.Stop();
                return new UpdateCheckResult(
                    UpdateCheckStatus.Error,
                    AppVersion,
                    AppVersion,
                    ReleasesUrl,
                    Math.Max(1, sw.ElapsedMilliseconds),
                    $"HTTP {(int)releaseResponse.StatusCode}");
            }
            catch (Exception ex)
            {
                sw.Stop();
                return new UpdateCheckResult(
                    UpdateCheckStatus.Error,
                    AppVersion,
                    AppVersion,
                    ReleasesUrl,
                    Math.Max(1, sw.ElapsedMilliseconds),
                    ex.Message);
            }
        }

        private static string NormalizeVersion(string rawTag)
        {
            string trimmed = rawTag.Trim();
            if (trimmed.StartsWith("v", StringComparison.OrdinalIgnoreCase))
            {
                trimmed = trimmed.Substring(1);
            }

            int dashIdx = trimmed.IndexOf('-');
            if (dashIdx >= 0)
            {
                trimmed = trimmed.Substring(0, dashIdx);
            }

            return string.IsNullOrWhiteSpace(trimmed) ? AppVersion : trimmed;
        }

        private static int CompareSemver(string a, string b)
        {
            string[] partsA = a.Split('.');
            string[] partsB = b.Split('.');
            int length = Math.Max(Math.Max(partsA.Length, partsB.Length), 3);

            for (int i = 0; i < length; i++)
            {
                int valA = i < partsA.Length && int.TryParse(partsA[i], out int pa) ? pa : 0;
                int valB = i < partsB.Length && int.TryParse(partsB[i], out int pb) ? pb : 0;

                if (valA > valB) return 1;
                if (valA < valB) return -1;
            }

            return 0;
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}

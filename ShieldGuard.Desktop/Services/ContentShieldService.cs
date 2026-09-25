using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ShieldGuard.Desktop.Services;

public class ContentShieldService
{
    private static readonly string HostsPath = @"C:\Windows\System32\drivers\etc\hosts";
    private static readonly string BlockedHostsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        "ShieldGuard", "blocked_hosts.txt");
    private const string BlockMarkerStart = "# === SHIELDGUARD START ===";
    private const string BlockMarkerEnd = "# === SHIELDGUARD END ===";

    // Built-in blocklist (top harmful categories)
    private static readonly string[] BuiltInBlockList = new[]
    {
        // Adult content
        "pornhub.com", "xvideos.com", "xnxx.com", "xhamster.com", "redtube.com",
        "youporn.com", "tube8.com", "spankbang.com", "eporner.com", "beeg.com",
        "tnaflix.com", "drtuber.com", "hardsextube.com", "sunporno.com", "hdtube.xxx",
        "fapvid.com", "netfapx.com", "onlyfans.com", "brazzers.com", "bang.com",
        // Gambling
        "bet365.com", "pokerstars.com", "888casino.com", "williamhill.com", "betway.com",
        "paddy power.com", "draftkings.com", "fanduel.com", "betfair.com", "unibet.com",
        // Malware/Scam
        "malwarebytes-free.com", "yourcomputerisinfected.com", "systemalert.co",
        // Drugs
        "erowid.org", "silk-road.to", "psychonaut.wiki",
    };

    public bool IsEnabled()
    {
        try
        {
            var content = File.ReadAllText(HostsPath);
            return content.Contains(BlockMarkerStart);
        }
        catch { return false; }
    }

    public async Task EnableAsync(IProgress<string>? progress = null)
    {
        progress?.Report("Loading block list...");
        var domains = await GetBlockListAsync(progress);

        progress?.Report("Applying protection to system...");
        ApplyToHosts(domains);
        progress?.Report("Content Shield enabled! ✓");
    }

    public void Disable()
    {
        try
        {
            var lines = File.ReadAllLines(HostsPath).ToList();
            int start = lines.FindIndex(l => l.Trim() == BlockMarkerStart);
            int end = lines.FindIndex(l => l.Trim() == BlockMarkerEnd);
            if (start >= 0 && end >= 0 && end > start)
            {
                lines.RemoveRange(start, end - start + 1);
                File.WriteAllLines(HostsPath, lines);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to disable shield: {ex.Message}");
        }
    }

    private async Task<List<string>> GetBlockListAsync(IProgress<string>? progress)
    {
        var domains = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        // Add built-in list
        foreach (var d in BuiltInBlockList)
            domains.Add(d);

        // Try to fetch updated lists from community sources
        var sources = new[]
        {
            "https://raw.githubusercontent.com/StevenBlack/hosts/master/alternates/fakenews-gambling-porn/hosts",
        };

        foreach (var url in sources)
        {
            try
            {
                progress?.Report($"Downloading blocklist...");
                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
                var content = await client.GetStringAsync(url);
                ParseHostsFile(content, domains);
                progress?.Report($"Loaded {domains.Count:N0} domains");
            }
            catch
            {
                progress?.Report("Using built-in list (offline mode)");
            }
        }

        return domains.ToList();
    }

    private static void ParseHostsFile(string content, HashSet<string> domains)
    {
        foreach (var line in content.Split('\n'))
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith('#') || string.IsNullOrWhiteSpace(trimmed)) continue;
            var parts = trimmed.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2 && (parts[0] == "0.0.0.0" || parts[0] == "127.0.0.1"))
            {
                var domain = parts[1].ToLowerInvariant();
                if (!domain.Contains("localhost") && domain.Contains('.'))
                    domains.Add(domain);
            }
        }
    }

    private static void ApplyToHosts(List<string> domains)
    {
        // Remove old ShieldGuard block
        var lines = File.ReadAllLines(HostsPath).ToList();
        int start = lines.FindIndex(l => l.Trim() == BlockMarkerStart);
        int end = lines.FindIndex(l => l.Trim() == BlockMarkerEnd);
        if (start >= 0 && end >= 0 && end > start)
            lines.RemoveRange(start, end - start + 1);

        // Add SafeSearch enforcement
        var safesearch = new List<string>
        {
            "# SafeSearch enforcement",
            "216.58.209.100 www.google.com",       // Force safe.google.com redirect (handled by DNS in production)
        };

        // Build block entries
        var block = new List<string> { "", BlockMarkerStart, $"# ShieldGuard Desktop — {domains.Count} domains blocked", $"# Updated: {DateTime.Now:yyyy-MM-dd HH:mm}" };
        foreach (var domain in domains.Take(50000))
        {
            block.Add($"0.0.0.0 {domain}");
            block.Add($"0.0.0.0 www.{domain}");
        }
        block.Add(BlockMarkerEnd);

        lines.AddRange(block);
        File.WriteAllLines(HostsPath, lines);
        
        // Flush DNS cache
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("ipconfig", "/flushdns")
        { WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden, UseShellExecute = true });
    }

    public int GetBlockedDomainCount()
    {
        try
        {
            var content = File.ReadAllText(HostsPath);
            return content.Split('\n').Count(l => l.TrimStart().StartsWith("0.0.0.0") && !l.Contains("# "));
        }
        catch { return 0; }
    }
}

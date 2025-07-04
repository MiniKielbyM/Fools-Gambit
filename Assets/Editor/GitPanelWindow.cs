
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using System.Diagnostics;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
public static class GitUtils
{
    public static bool IsGitRepository(string path = null)
    {
        // Use the root of the Unity project if no path is provided
        if (string.IsNullOrEmpty(path))
        {
            path = Directory.GetCurrentDirectory();
        }

        string gitFolderPath = Path.Combine(path, ".git");
        return Directory.Exists(gitFolderPath);
    }
    public static bool HasGitHubRemote(string path = null)
    {
        if (string.IsNullOrEmpty(path))
        {
            path = Directory.GetCurrentDirectory();
        }

        if (!Directory.Exists(Path.Combine(path, ".git")))
        {
            return false; // Not a Git repo
        }

        try
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = "remote -v", // shows remotes with URLs
                WorkingDirectory = path,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(startInfo))
            {
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                return output.Contains("github.com");
            }
        }
        catch
        {
            return false;
        }
    }
    public static async Task<string> GetGitHubUsername(string accessToken)
    {
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd("UnityGitPanel");
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            HttpResponseMessage response = await client.GetAsync("https://api.github.com/user");

            if (response.IsSuccessStatusCode)
            {
                string result = await response.Content.ReadAsStringAsync();
                var match = Regex.Match(result, "\"login\"\\s*:\\s*\"([^\"]+)\"");
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
            }
            else
            {
                UnityEngine.Debug.LogError($"❌ Failed to fetch username: {response.StatusCode}");
            }
        }
        return null;
    }
    private static void RunGitCommand(string args, string workingDirectory = null)
    {
        if (string.IsNullOrEmpty(workingDirectory))
        {
            workingDirectory = Directory.GetCurrentDirectory();
        }

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = args,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (Process process = Process.Start(startInfo))
        {
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (!string.IsNullOrEmpty(error))
            {
                UnityEngine.Debug.LogError($"❌ Git command failed: {error}");
            }
            else
            {
                UnityEngine.Debug.Log($"✅ Git command succeeded: {output}");
            }
        }
    }
    public static async Task<string> CreateRepo(string repoName, string accessToken)
    {
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd("UnityGitPanel");
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var json = $"{{\"name\": \"{repoName}\", \"private\": false}}";
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://api.github.com/user/repos", content);
            string result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                UnityEngine.Debug.LogError($"❌ Failed to create repo: {response.StatusCode} - {result}");
                return null;
            }

            string username = await GetGitHubUsername(accessToken);
            if (string.IsNullOrEmpty(username))
            {
                UnityEngine.Debug.LogError("❌ Could not fetch GitHub username.");
                return null;
            }

            string remoteUrl = $"https://github.com/{username}/{repoName}.git";
            string root = Directory.GetCurrentDirectory();
            string gitignorePath = Path.Combine(root, ".gitignore");

            // Ensure Unity patterns are present in .gitignore
            string[] unityPatterns = new string[]
            {
            "[Ll]ibrary/",
            "[Tt]emp/",
            "[Oo]bj/",
            "[Bb]uild/",
            "[Bb]uilds/",
            "[Ll]ogs/",
            "[Mm]emoryCaptures/",
            "sysinfo.txt",
            "*.userprefs",
            "*.csproj",
            "*.unityproj",
            "*.sln",
            "*.suo",
            "*.tmp",
            "*.user",
            "*.booproj",
            "*.pidb",
            "*.svd",
            "*.pdb",
            "*.mdb",
            "*.opendb",
            "*.VC.db",
            ".vscode/",
            ".idea/",
            ".DS_Store",
            "*.apk",
            "*.aab"
            };

            HashSet<string> existingLines = new HashSet<string>();
            if (File.Exists(gitignorePath))
            {
                var lines = File.ReadAllLines(gitignorePath);
                foreach (var line in lines)
                    existingLines.Add(line.Trim());
            }

            using (StreamWriter writer = new StreamWriter(gitignorePath, append: true))
            {
                foreach (var pattern in unityPatterns)
                {
                    if (!existingLines.Contains(pattern))
                    {
                        writer.WriteLine(pattern);
                    }
                }
            }

            // Initialize local git repo if it doesn't exist
            if (!Directory.Exists(Path.Combine(root, ".git")))
            {
                RunGitCommand("init", root);
            }

            RunGitCommand($"remote add origin {remoteUrl}", root);
            RunGitCommand("add .gitignore", root);
            RunGitCommand("commit -m \"Ensure .gitignore is present and updated\"", root);
            RunGitCommand("branch -M main", root);
            RunGitCommand("push -u origin main", root);

            return remoteUrl;
        }
    }

}
public class GitHubOAuthWindow : EditorWindow
{
    private const string clientId = "Ov23livN2pwLZuwTaP4Q";
    private const string clientSecret = "ee62345ec2c05b532b63f1a8bb823cc0bffa17e6";
    private const string redirectUri = "http://localhost:4567/callback/";
    public const string tokenKey = "GitHubAccessToken";
    public static string AccessToken => EditorPrefs.GetString(tokenKey, "");
    public static bool IsSignedIn() => !string.IsNullOrEmpty(AccessToken);
    [MenuItem("GitPane/Sign in with GitHub", false, 10)]
    public static void ShowWindow()
    {
        GitHubOAuthWindow window = GetWindow<GitHubOAuthWindow>("GitHub OAuth");
        window.InitiateOAuthFlow();
        window.Close();
    }
    private async void InitiateOAuthFlow()
    {
        string authUrl = $"https://github.com/login/oauth/authorize?client_id={clientId}&redirect_uri={redirectUri}&scope=repo";
        Application.OpenURL(authUrl);
        string code = await WaitForOAuthCode();

        if (!string.IsNullOrEmpty(code))
        {
            string token = await ExchangeCodeForToken(code);
            if (!string.IsNullOrEmpty(token))
            {
                EditorPrefs.SetString(tokenKey, token);
            }
        }
    }
    private async Task<string> WaitForOAuthCode()
    {
        HttpListener listener = new HttpListener();
        listener.Prefixes.Add(redirectUri);

        try
        {
            listener.Start();
            var context = await listener.GetContextAsync();
            var code = context.Request.QueryString["code"];
            byte[] buffer = Encoding.UTF8.GetBytes("<html><body><h2>GitHub login successful. You may return to Unity.</h2></body></html>");
            context.Response.ContentLength64 = buffer.Length;
            await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            context.Response.OutputStream.Close();
            return code;
        }
        finally
        {
            listener.Stop();
        }

    }
    private async Task<string> ExchangeCodeForToken(string code)
    {
        using (var client = new HttpClient())
        {
            var values = new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "code", code },
                { "redirect_uri", redirectUri }
            };

            var content = new FormUrlEncodedContent(values);
            var response = await client.PostAsync("https://github.com/login/oauth/access_token", content);
            var body = await response.Content.ReadAsStringAsync();
            var match = Regex.Match(body, @"access_token=([^&]+)");
            return match.Success ? match.Groups[1].Value : null;
        }
    }
    [MenuItem("GitPane/Sign in with GitHub", true, 10)]
    public static bool ValidateOAuth()
    {
        return !IsSignedIn();
    }
}

public class GithubSignOutWindow : EditorWindow
{
    [MenuItem("GitPane/Sign out from GitHub", false, 12)]
    public static void ShowWindow()
    {
        GithubSignOutWindow window = GetWindow<GithubSignOutWindow>("GitHub Sign Out");
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("Are you sure you want to sign out?", EditorStyles.boldLabel);
        if (GUILayout.Button("Sign Out"))
        {
            EditorPrefs.DeleteKey(GitHubOAuthWindow.tokenKey);
            Close();
        }
        if (GUILayout.Button("Cancel"))
        {
            Close();
        }
    }

    [MenuItem("GitPane/Sign out from GitHub", true, 12)]
    public static bool ValidateSignOut()
    {
        return GitHubOAuthWindow.IsSignedIn();
    }
}

public class GitPanelWindow : EditorWindow
{
    private bool remoteHasChanges = false;
    private bool localHasChanges = false;
    private async Task CheckRemoteChangesAsync()
    {
        await Task.Run(() =>
        {
            try
            {
                RunGitCommand("fetch origin");
                string result = RunGitCommand("rev-list HEAD..origin/main --count");
                remoteHasChanges = int.TryParse(result, out int count) && count > 0;
            }
            catch
            {
                remoteHasChanges = false;
            }
        });
        Repaint();
    }
    private async Task CheckLocalChangesAsync()
    {
        await Task.Run(() =>
        {
            try
            {
                RunGitCommand("fetch origin");
                string result = RunGitCommand("rev-list origin/main..HEAD --count");
                localHasChanges = int.TryParse(result, out int count) && count > 0;
            }
            catch
            {
                localHasChanges = false;
            }
        });
        Repaint();
    }
    private async void Update()
    {
        if (EditorApplication.timeSinceStartup - lastRefreshTime > refreshInterval)
        {
            RefreshGitStatus();
            Repaint();
            if (GitUtils.HasGitHubRemote())
            {
                await CheckRemoteChangesAsync();
                await CheckLocalChangesAsync();
            }
            else
            {
                remoteHasChanges = false;
                localHasChanges = false;
            }
            lastRefreshTime = EditorApplication.timeSinceStartup;
        }
    }

    private string commitMessage = "";
    private List<GitFileChange> fileChanges = new List<GitFileChange>();
    private double lastRefreshTime;
    private const double refreshInterval = 1.0; // seconds
    [MenuItem("GitPane/🐱 Git Panel")]
    public static void ShowWindow()
    {
        var window = GetWindow<GitPanelWindow>("Git Panel");
        window.minSize = new Vector2(300, 200);
    }
    private async void OnEnable()
    {
        if (!GitUtils.IsGitRepository())
        {
            bool create = EditorUtility.DisplayDialog(
                "Git Not Initialized",
                "This project is not a Git repository. Would you like to create one and push it to GitHub?",
                "Yes", "No"
            );
            if (create)
            {
                if (!GitHubOAuthWindow.IsSignedIn())
                {
                    EditorUtility.DisplayDialog("GitHub Not Signed In", "You must sign in with GitHub first.", "OK");
                    return;
                }
                string repoName = Path.GetFileName(Directory.GetCurrentDirectory());
                string accessToken = GitHubOAuthWindow.AccessToken;
                string url = await GitUtils.CreateRepo(repoName, accessToken);
                if (!string.IsNullOrEmpty(url))
                {
                    UnityEngine.Debug.Log("✅ Repo created and pushed to GitHub: " + url);
                }
            }
        }
        RefreshGitStatus();
    }
    private void OnGUI()
    {
        GUILayout.Label("Commit Message", EditorStyles.boldLabel);
        commitMessage = GUILayout.TextField(commitMessage);
        GUILayout.BeginHorizontal();
        EditorGUI.BeginDisabledGroup(!SceneManager.GetActiveScene().isDirty && fileChanges.Count == 0);
        if (GUILayout.Button("✓ Commit", GUILayout.Height(25)))
        {
            if (!string.IsNullOrWhiteSpace(commitMessage))
            {
                CommitChanges(commitMessage);
                commitMessage = "";
            }
            else
            {
                UnityEngine.Debug.LogWarning("⚠️ Commit message cannot be empty.");
            }
        }
        EditorGUI.EndDisabledGroup();
        EditorGUI.BeginDisabledGroup(!localHasChanges);
        if (GUILayout.Button("↑ Push  ", GUILayout.Height(25), GUILayout.Width(position.width * 0.25f)))
        {
            try
            {
                RunGitCommand("push -f");
                UnityEngine.Debug.Log("✅ Push successful.");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError("❌ Push failed: " + e.Message);
            }
            RefreshGitStatus();
        }
        EditorGUI.EndDisabledGroup();

        EditorGUI.BeginDisabledGroup(!remoteHasChanges);
        if (GUILayout.Button("↓ Pull  ", GUILayout.Height(25), GUILayout.Width(position.width * 0.25f)))
        {
            try
            {
                RunGitCommand("pull");
                EditorSceneManager.OpenScene(SceneManager.GetActiveScene().path);
                UnityEngine.Debug.Log("✅ Pull successful.");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError("❌ Pull failed: " + e.Message);
            }
            RefreshGitStatus();
        }
        EditorGUI.EndDisabledGroup();
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        GUILayout.Label("Changes", EditorStyles.boldLabel);
        if (fileChanges.Count == 0)
        {
            GUILayout.Label("No changes detected.");
        }
        foreach (var change in fileChanges)
        {
            GUILayout.BeginHorizontal();
            GUI.color = GetStatusColor(change.Status);
            GUILayout.Label(GetStatusSymbol(change.Status), GUILayout.Width(20));
            GUI.color = Color.white;
            GUILayout.Label(change.FilePath);
            GUILayout.EndHorizontal();
        }
    }
    private void RefreshGitStatus()
    {
        fileChanges = GetGitStatus();
    }
    private void CommitChanges(string message)
    {
        try
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.isDirty)
            {
                if (!EditorUtility.DisplayDialog(
                    "Unsaved Scene",
                    "The active scene has unsaved changes. Do you want to save it before committing?",
                    "Save and Commit", "Commit without Saving"))
                {
                    return; // User chose not to save
                }
                else
                {
                    EditorSceneManager.SaveScene(activeScene);
                }
            }
            RunGitCommand("add .");
            RunGitCommand("add .mp3");
            RunGitCommand("add .wav");
            RunGitCommand($"commit -m \"{message}\"");
            UnityEngine.Debug.Log("✅ Commit successful.");
            RefreshGitStatus();
            Repaint();
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError("❌ Commit failed: " + e.Message);
        }
    }
    private List<GitFileChange> GetGitStatus()
    {
        var changes = new List<GitFileChange>();
        var startInfo = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = "status --porcelain",
            WorkingDirectory = Directory.GetCurrentDirectory(),
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        using (var process = Process.Start(startInfo))
        {
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            foreach (var line in output.Split('\n'))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                string status = line.Substring(0, 2).Trim();
                string filePath = line.Substring(3).Trim();
                changes.Add(new GitFileChange { Status = status, FilePath = filePath });
            }
        }
        return changes;
    }

    private string RunGitCommand(string args)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = args,
            WorkingDirectory = Directory.GetCurrentDirectory(),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        using (var process = Process.Start(startInfo))
        {
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            return output;
        }
    }

    private string GetStatusSymbol(string status)
    {
        return status switch
        {
            "M" => "M",
            "A" => "A",
            "D" => "D",
            "R" => "R",
            "??" => "U",
            "!" => "!",
            _ => status
        };
    }
    private Color GetStatusColor(string status)
    {
        return status switch
        {
            "M" => Color.yellow,
            "A" => Color.green,
            "D" => Color.red,
            "R" => Color.cyan,
            "??" => Color.green,
            _ => Color.white
        };
    }

    private struct GitFileChange
    {
        public string Status;
        public string FilePath;
    }
}

#endif

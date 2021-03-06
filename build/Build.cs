using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[CheckBuildProjectConfigurations]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main() => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution] readonly Solution Solution;

    AbsolutePath OutputDirectory => RootDirectory / "output";

    AbsolutePath ArtifactDirectory => RootDirectory / "artifact";

    [Parameter()]
    public string WebAppUser;

    [Parameter()]
    public string WebAppPassword;

    [Parameter]
    public string WebAppName;

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoRestore());
        });

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            EnsureCleanDirectory(OutputDirectory);
            EnsureCleanDirectory(ArtifactDirectory);
        });

    Target Publish => _ => _
        .DependsOn(Compile)
        .DependsOn(Clean)
        .Executes(() =>
        {
            DotNetPublish(s => s
                .SetProject(Solution)
                .SetConfiguration(Configuration)
                .SetOutput(OutputDirectory)
                .EnableNoRestore()
                .SetNoBuild(true));
        });

    Target Zip => _ => _
        .DependsOn(Publish)
        .Executes(() =>
        {
            ZipFile.CreateFromDirectory(OutputDirectory, ArtifactDirectory / "deployment.zip");
        });

    Target Deploy => _ => _
        .DependsOn(Zip)
        .Requires(() => WebAppUser)
        .Requires(() => WebAppPassword)
        .Requires(() => WebAppName)
        .Executes(async () =>
        {
            Serilog.Log.Information(WebAppUser);
            var base64Auth = Convert.ToBase64String(Encoding.Default.GetBytes($"{WebAppUser}:{WebAppPassword}"));
            using (var memStream = new MemoryStream(File.ReadAllBytes(ArtifactDirectory / "deployment.zip")))
            {
                memStream.Position = 0;
                var content = new StreamContent(memStream);
                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", base64Auth);
                var requestUrl = $"https://{WebAppName}.scm.azurewebsites.net/api/zipdeploy";
                var response = await httpClient.PostAsync(requestUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    Assert.Fail("Deployment returned status code: " + response.StatusCode);
                }
            }
        });

}

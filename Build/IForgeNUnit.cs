using Nuke.Common;
using Nuke.Common.IO;
using ricaun.Nuke.Components;
using ricaun.Nuke.Extensions;
using System;
using System.IO;
using System.Net.Http;

namespace ricaun.Nuke.Components
{
    internal interface IForgeNUnit : IRelease, IHazForgeNUnit
    {
        Target ForgeNUnit => _ => _
                .TriggeredBy(Release)
                .OnlyWhenStatic(() => ForgeNUnitUrl.SkipEmpty())
                .OnlyWhenStatic(() => ForgeNUnitApi.SkipEmpty())
                .OnlyWhenStatic(() => ForgeNUnitApiKey.SkipEmpty())
                .Executes(async () =>
                {
                    var forgeNUnitUrl = ForgeNUnitUrl.Trim('/');
                    foreach (var file in PathConstruction.GlobFiles(ReleaseDirectory, "*.zip"))
                    {
                        var fileName = Path.GetFileName(file);
                        var requestUri = $"{forgeNUnitUrl}/{fileName}";
                        Serilog.Log.Information($"Upload: {fileName}");
                        using (HttpClient client = new HttpClient())
                        {
                            client.Timeout = TimeSpan.FromSeconds(600);
                            client.DefaultRequestHeaders.Add(ForgeNUnitApi, ForgeNUnitApiKey);
                            using (FileStream fs = new FileStream(file, FileMode.Open))
                            {
                                byte[] fileBytes = new byte[(int)fs.Length];
                                await fs.ReadAsync(fileBytes, 0, (int)fs.Length);
                                using (var content = new ByteArrayContent(fileBytes))
                                {
                                    var response = await client.PutAsync(requestUri, content);
                                    var responseContent = await response.Content.ReadAsStringAsync();
                                    Serilog.Log.Information(responseContent.JsonPrettify());
                                    response.EnsureSuccessStatusCode();
                                }
                            }
                        }
                    }

                });
    }

    internal interface IHazForgeNUnit : INukeBuild
    {
        [Secret][Parameter] public string ForgeNUnitUrl => TryGetValue(() => ForgeNUnitUrl);
        [Secret][Parameter] public string ForgeNUnitApi => TryGetValue(() => ForgeNUnitApi);
        [Secret][Parameter] public string ForgeNUnitApiKey => TryGetValue(() => ForgeNUnitApiKey);
    }

    static class JsonExtension
    {
        public static string JsonPrettify(this string json)
        {
            try
            {
                using (var stringReader = new StringReader(json))
                using (var stringWriter = new StringWriter())
                {
                    var jsonReader = new Newtonsoft.Json.JsonTextReader(stringReader);
                    var jsonWriter = new Newtonsoft.Json.JsonTextWriter(stringWriter) { Formatting = Newtonsoft.Json.Formatting.Indented };
                    jsonWriter.WriteToken(jsonReader);
                    return stringWriter.ToString();
                }
            }
            catch
            {
                return json;
            }
        }
    }
}

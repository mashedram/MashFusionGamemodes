using System.Reflection;
using System.Resources;
using MashAutoUpdatorPlugin;
using MelonLoader;

[assembly: AssemblyTitle(Plugin.Name)]
[assembly: AssemblyProduct(Plugin.Name)]
[assembly: AssemblyCopyright("Created by " + Plugin.Author)]
[assembly: AssemblyVersion(Plugin.Version)]
[assembly: AssemblyFileVersion(Plugin.Version)]
[assembly: NeutralResourcesLanguage("en")]
[assembly: MelonInfo(typeof(Plugin), Plugin.Name, Plugin.Version, Plugin.Author, null)]

[assembly: MelonGame("Stress Level Zero", "BONELAB")]
namespace MashAutoUpdatorPlugin;

public class Plugin : MelonPlugin
{
    public const string Name = "Mash Auto Updator";
    public const string Author = "Mash";
    public const string Version = "1.0.0";
    
    // TODO: Make these stored in a file
    public const string RemoteURL = "http://localhost:8000/";
    public const string Repo = "clockhunt";
    public const string Channel = "release";

    public override void OnPreInitialization()
    {
        
    }
}
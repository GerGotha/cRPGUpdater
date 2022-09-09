using Gameloop.Vdf;
using Gameloop.Vdf.Linq;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO.Compression;

const string downloadUrl = "https://c-rpg.eu/cRPG.zip";
const string userRoot = "HKEY_CURRENT_USER";
const string subkey = @"Software\Valve\Steam";
const string keyName = userRoot + "\\" + subkey;

string steamInstallPath = (string)Registry.GetValue(keyName, "SteamPath", null);

string steamLibraryVdfPath = steamInstallPath + "\\steamapps\\libraryfolders.vdf";
VProperty libraryVdf = VdfConvert.Deserialize(File.ReadAllText(steamLibraryVdfPath));

List<string> steamBlPaths = new List<string>();
int counter = 0;
while (true)
{
    string index = counter.ToString();
    if (libraryVdf.Value[index] == null)
        break;

    if (libraryVdf.Value[index]?["path"] == null)
    {
        continue;
    }
    string path = libraryVdf.Value[index]?["path"]?.ToString();
    path += @"\steamapps\common\Mount & Blade II Bannerlord";
    steamBlPaths.Add(path);
    counter++;
}

string targetPath = String.Empty;
foreach (string steamBlPath in steamBlPaths)
{
    if (Directory.Exists(steamBlPath))
    {
        targetPath = steamBlPath;
        break;
    }
}

if (targetPath == null)
    return;


string blPathExe = targetPath+@"\bin\Win64_Shipping_Client\Bannerlord.exe";
string modulesPath = targetPath + @"\Modules";
string crpgPath = modulesPath + @"\cRPG";

String timeStamp = DateTime.Now.ToFileTime().ToString();
string downloadPath = System.IO.Path.GetTempPath() + @"\cRPG"+ timeStamp + ".zip";

var httpClient = new HttpClient();
var httpResult = await httpClient.GetAsync(downloadUrl);
using var resultStream = await httpResult.Content.ReadAsStreamAsync();
using var fileStream = File.Create(downloadPath);
resultStream.CopyTo(fileStream);
fileStream.Close();

if (!File.Exists(System.IO.Path.GetTempPath() + @"\cRPG"+ timeStamp +".zip"))
    return;

if (Directory.Exists(crpgPath))
    Directory.Delete(crpgPath, true);

Directory.CreateDirectory(crpgPath);
ZipFile.ExtractToDirectory(downloadPath, crpgPath);
File.Delete(downloadPath);

ProcessStartInfo startInfo = new ProcessStartInfo();
startInfo.WorkingDirectory = Path.GetDirectoryName(blPathExe);
startInfo.FileName = "Bannerlord.exe";
startInfo.Arguments = "_MODULES_*Native*cRPG*_MODULES_ /multiplayer";
startInfo.UseShellExecute = true;

Process.Start(startInfo);


using Newtonsoft.Json;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace CobbleBuild {
   /// <summary>
   /// Async runs the actions asyncronously, Sync runs the actions syncronously, and SyncOnDemand runs the actions when AddOrRun() is called.
   /// </summary>
   enum ActionGroupType {
      Sync, Async, SyncOnDemand
   }
   /// <summary>
   /// Internal class designed to be able to either run the actions syncronously or asyncronously.
   /// Deprecated: Not really ideal. Didn't meaningfully improve performance.
   /// </summary>
   [Obsolete]
   internal class ActionGroup {
      public ActionGroupType type;
      private List<Action>? _actions;
      public ActionGroup(ActionGroupType type) {
         this.type = type;
         if (type != ActionGroupType.SyncOnDemand) {
            _actions = new List<Action>();
         }
      }
      /// <summary>
      /// Adds the tasks to be run unless set to on demand.
      /// </summary>
      /// <param name="action"></param>
      public void AddOrRun(Action action) {
         if (type != ActionGroupType.SyncOnDemand) {
            _actions.Add(action);
         }
         else {
            action();
         }
      }

      private void runAllSync() {
         foreach (var item in _actions) {
            item();
         }
      }

      private void runAllAsync() {
         var tasks = new Task[_actions.Count];
         for (var i = 0; i < tasks.Length; i++) {
            tasks[i] = Task.Factory.StartNew(_actions[i]);
         }
         Task.WaitAll(tasks);
      }
      /// <summary>
      /// Executes all tasks and measures how long it takes. Returns 0 if mode is set to SyncOnDemand
      /// </summary>
      /// <returns>Time taken in ms</returns>
      public long ExecuteAll() {
         if (type == ActionGroupType.SyncOnDemand) {
            return 0;
         }

         var timer = Stopwatch.StartNew();
         //(type == ActionGroupType.Sync) ? runAllSync() : runAllAsync();
         if (type == ActionGroupType.Sync) {
            runAllSync();
         }
         else {
            runAllAsync();
         }
         timer.Stop();
         return timer.ElapsedMilliseconds;
      }
   }
   internal static class Misc {
      public static List<string> validRegionalVariants = ["alolan", "galarian", "valencian"];
      private static string generatedDisclaimer = @"/* This File was generated with CobbleBuild.
 * These files might be overwritable in the future, but for now please submit any issues to CobbleBuild
 * instead of fixing them here. 
 */
";
      /// <summary>
      /// Serializes the passed in object as a json and saves it to the passed in path.
      /// </summary>
      /// <param name="anyObject">Object to be serialized.</param>
      /// <param name="path">Path to save the file to/</param>
      /// <param name="generatedHeading">Adds the comment at the top of the file saying that it is auto-generated. Defaults to true.</param>
      public static void SaveToJson(object anyObject, string path, bool generatedHeading = true) {
         string jsonData = JsonConvert.SerializeObject(anyObject, Config.config.SerializerSettings);
         if (generatedHeading)
            jsonData = jsonData.Insert(0, generatedDisclaimer);

         File.WriteAllText(path, jsonData);
      }
      public static async Task SaveToJsonAsync(object anyObject, string path, bool generatedHeading = true) {
         string jsonData = JsonConvert.SerializeObject(anyObject, Config.config.SerializerSettings);
         if (generatedHeading)
            jsonData = jsonData.Insert(0, generatedDisclaimer);

         await File.WriteAllTextAsync(path, jsonData);
      }
      /// <summary>
      /// Takes in a filepath and deserializes it into the specified type
      /// </summary>
      /// <typeparam name="T">Type to be serialized into.</typeparam>
      /// <param name="path">Path to load the json from.</param>
      /// <returns>Deserialized object</returns>
      /// <exception cref="Exception">Generic deserialization failed error.</exception>
      /// <exception cref="FileNotFoundException">File was not found.</exception>
      public static T LoadFromJson<T>(string path) {
         if (!File.Exists(path))
            throw new FileNotFoundException($"File {Path.GetFileName(path)} was not found.");

         T? data = JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
         if (data == null)
            throw new Exception("The JSON File could not be read.");
         return data;
      }

      public static async Task<T> LoadFromJsonAsync<T>(string path) {
         if (!File.Exists(path))
            throw new FileNotFoundException($"File {Path.GetFileName(path)} was not found.");

         T? data = JsonConvert.DeserializeObject<T>(await File.ReadAllTextAsync(path));
         if (data == null)
            throw new Exception("The JSON File could not be read.");
         return data;
      }

      /// <summary>
      /// Somehow, this is not an inbuilt function.
      /// </summary>
      /// <param name="sourcePath">Source File Path</param>
      /// <param name="outputPath">Destination File Path</param>
      public static async Task CopyAsync(string sourcePath, string outputPath) {
         using (var sourceStream = File.OpenRead(sourcePath)) {
            using (var destStream = File.Create(outputPath)) {
               await sourceStream.CopyToAsync(destStream);
            }
         }
      }

      public static void error(string msg) {
         Debug.WriteLine(msg);
         Console.ForegroundColor = ConsoleColor.Red;
         Console.WriteLine(msg);
         Console.ForegroundColor = ConsoleColor.White;
         Environment.Exit(1);
      }
      public static void softError(string msg) {
         Debug.WriteLine(msg);
         Console.ForegroundColor = ConsoleColor.Red;
         Console.WriteLine(msg);
         Console.ForegroundColor = ConsoleColor.White;
      }
      public static void warn(string msg) {
         Debug.WriteLine(msg);
         Console.ForegroundColor = ConsoleColor.Yellow;
         Console.WriteLine(msg);
         Console.ForegroundColor = ConsoleColor.White;
      }
      public static void info(string msg) {
         Debug.WriteLine(msg);
         Console.WriteLine(msg);
      }
      public static void printInnerExceptions(AggregateException exception) {
         foreach (var item in exception.InnerExceptions) {
            softError(item.Message);
         }
      }
      /// <summary>
      /// Extracts a file to a directory. Adds file to temporaryFolders list if config.temporairlyExtract is true.
      /// </summary>
      /// <param name="filePath">Path to the file to be extracted.</param>
      /// <param name="outputPath">Optional output path. If null, the path will be inherited from the file name</param>
      /// <returns>The path that the files were extracted to.</returns>
      /// <exception cref="FileNotFoundException"> Thrown when file specified in filePath doesnt exist.</exception>
      public static string ExtractZipFile(string filePath, string? outputPath = null) {
         if (!File.Exists(filePath))
            throw new FileNotFoundException($"File {filePath} could not be found.");

         if (outputPath == null)
            outputPath = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath));

         Directory.CreateDirectory(outputPath);
         System.IO.Compression.ZipFile.ExtractToDirectory(filePath, outputPath, true);
         if (Config.config.temporairlyExtract)
            Program.temporaryFolders.Add(outputPath);

         return outputPath;
      }
      public static string ToTitleCase(string input) {
         TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
         return textInfo.ToTitleCase(input.Replace("_", " "));
      }
      //Basic Yes or no prompt
      public static bool yesOrNo(string question) {
         Console.Write(question);
         Console.Write("(y/n):");
         ConsoleKeyInfo key = Console.ReadKey();
         Console.WriteLine();
         return (key.Key == ConsoleKey.Y);
      }

      public static T? DeserializeFromFile<T>(string filePath) {
         return JsonConvert.DeserializeObject<T>(File.ReadAllText(filePath));
      }
      //Yoinked from stackoverflow
      public static void OpenUri(string url) {
         try {
            Process.Start(url);
         }
         catch {
            //I don't know that this is still necessary but stack overflow guy kept it in so no touch I don't have mac to test
            // hack because of this: https://github.com/dotnet/corefx/issues/10361
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
               url = url.Replace("&", "^&");
               Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
               Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
               Process.Start("open", url);
            }
            else {
               throw;
            }
         }
      }
      public static void RunCmd(string FileName, string arguments, string workingDirectory) {
         if (Config.config.usePowershell) {
            Process process = new Process();
            process.StartInfo = new ProcessStartInfo("powershell.exe", $"{FileName} {arguments}") {
               WorkingDirectory = workingDirectory,
               UseShellExecute = false,
               //RedirectStandardOutput = true,
               RedirectStandardError = true,
               RedirectStandardInput = true,
            };
            process.Start();
            process.WaitForExit();
            if (process.ExitCode != 0) {
               softError("Deploy process exited with code " + process.ExitCode);
            }
         }
         else {
            Process process = new Process();
            process.StartInfo = new ProcessStartInfo(FileName, arguments) {
               WorkingDirectory = workingDirectory,
               UseShellExecute = true
            };
            process.Start();
            process.WaitForExit();
            if (process.ExitCode != 0) {
               softError("Deploy process exited with code " + process.ExitCode);
            }
         }
      }
      /// <summary>
      /// Get all files in a Dir and Subdirs
      /// Old implementation
      /// ExcludeDirs must contain exact path
      /// </summary>
      /// <param name="dirPath">Path of dir to look in.</param>
      /// <param name="excludeDirs">Full Path of directories to exclude.</param>
      /// <returns>Paths of Files</returns>
      public static string[] getAllFilesInDirandSubDirs(string dirPath, params string[] excludeDirs) {
         List<string> output = [];
         if (!excludeDirs.Contains(dirPath))
            output = output.Union(Directory.GetFiles(dirPath)).ToList();

         for (int i = 0; i < excludeDirs.Length; i++) {
            excludeDirs[i] = excludeDirs[i].Replace("\\", "/");
         }
         excludeDirs = excludeDirs.Select(x => x.Replace("\\", "/")).ToArray();
         foreach (string dir in Directory.GetDirectories(dirPath)) {
            if (!excludeDirs.Contains(dir.Replace("\\", "/"))) {
               foreach (string subdir in getAllFilesInDirandSubDirs(dir, excludeDirs)) {
                  output.Add(subdir);
               }
            }
         }
         return output.ToArray();
      }

      /// <summary>
      /// Get all files in dir and subdir
      /// </summary>
      /// <param name="dirPath">Path to directoyr</param>
      /// <param name="excludeDirs">Regex to use against Dirs to exlclude them. Does not use params to prevent ambiguous calls.</param>
      /// <returns>Array of Filepaths</returns>
      public static string[] getAllFilesInDirandSubDirs(string dirPath, Regex[] excludeDirs) {
         List<string> output = new List<string>();
         if (!excludeDirs.Any(x => x.IsMatch(dirPath)))
            output = output.Union(Directory.GetFiles(dirPath)).ToList();
         foreach (string dir in Directory.GetDirectories(dirPath)) {
            if (!excludeDirs.Any(x => x.IsMatch(dir))) {
               foreach (string subdir in getAllFilesInDirandSubDirs(dir, excludeDirs)) {
                  output.Add(subdir);
               }
            }
         }
         return output.ToArray();
      }
      //Meh
      public static string getInternalLocation(string outputPath) {
         string output = getPathFrom(outputPath, "textures");
         return output.Remove(output.Length - 4);
      }
      /// <summary>
      /// Returns part of the path after the specified folder name.
      /// ex: C:/Users/name/source/repos/test.exe with a foldername parameter of "source" would return source/repos/test.exe
      /// </summary>
      /// <param name="folderPath"></param>
      /// <param name="folderNameFrom"></param>
      /// <param name="includeFolderListed">Whether or not the specified folderName should remain in the output path</param>
      /// <exception cref="Exception">The Folder was not found in the path.</exception>
      public static string getPathFrom(string folderPath, string folderNameFrom, bool includeFolderListed = true) {
         folderPath = folderPath.Replace("\\", "/"); //Normalizing input
         int index = folderPath.IndexOf($"{folderNameFrom}/", StringComparison.OrdinalIgnoreCase);
         if (index == -1) {
            throw new Exception($"Folder Path {folderPath} does not contain folder {folderNameFrom}");
         }
         else {
            return folderPath.Substring(index + (includeFolderListed ? 0 : folderNameFrom.Length + 1));
         }
      }

      public static bool tryRemoveNamespace(string item, out string newItem) {
         Match nameMatch = Regex.Match(item, @":(.+)");
         if (!nameMatch.Success) {
            newItem = item;
            Misc.warn($"Couldn't remove namespace from {item}");
            return false;
         }
         newItem = nameMatch.Groups[1].Value;
         return true;

      }
      /// <summary>
      /// Excludes all characters that arent letters, digits.
      /// </summary>
      /// <param name="item"></param>
      /// <returns>Standardized id</returns>
      public static string toID(string item) {
         return new string(item
             .ToLower()
             //.Replace(' ', '_')
             .Where(c => char.IsLetterOrDigit(c)).ToArray());
      }
      /// <summary>
      /// Takes in a string and adds a namespace to it, as well as removing all invalid characters.
      /// </summary>
      /// <param name="item"></param>
      /// <param name="namespace">Namespace of the identifier </param>
      /// <returns>Identifier "namespace:normalized_item_id"</returns>
      public static string toIdentifier(string item, string @namespace = "cobblemon") {
         return $"{@namespace}:{toID(item)}";
      }
   }
   public class Registry<T> : Dictionary<string, List<T>> {
      public void Register(string key, T value) {
         if (this.ContainsKey(key)) {
            this[key].Add(value);
         }
         else {
            this[key] = [value];
         }
      }
      public void Unregister(string key, T value) {
         if (!this.ContainsKey(key))
            return;
         foreach (var item in this[key]) {
            if (object.Equals(item, value))
               this[key].Remove(value);
         }
      }
      public void UnregisterAll(string key) {
         if (this.ContainsKey(key))
            this.Remove(key);
      }
      public T GetFirst(string key) {
         if (!this.ContainsKey(key) || this[key].Count < 1)
            throw new Exception("Key has nothing registered to it.");
         return this[key][0];
      }
      public bool TryGetFirst(string key, out T? value) {
         try {
            value = GetFirst(key);
            return true;
         }
         catch { }
         value = default;
         return false;
      }
      /// <summary>
      /// Returns the amount of entries for a specific key.
      /// </summary>
      /// <param name="key"></param>
      /// <returns>Amount or 0 if key not registered.</returns>
      public int Amount(string key) {
         if (!this.ContainsKey(key))
            return 0;
         return this[key].Count;
      }
   }
}

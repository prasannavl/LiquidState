<Query Kind="Program">
  <Namespace>System</Namespace>
  <Namespace>System.Diagnostics</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

        //TODO: Make Version compatible with 3 segments.
        
        private const string versionFileName = "../VersionInfo.cs";
        private const string ciConfigFileName = "../appveyor.yml";
        private const string versionString = @"((\d+)\.(\d+)\.(\d+)\.(.*))";
        private const string buildToolsName = "LiquidState.BuildTools";
        
public void Main(string[] args)
        {
#if !CMD
            args = new string[] {  };
#endif

            ParseArgs(args);

#if !STANDALONE
            var dir = Util.CurrentQuery.Location;
#else
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
#endif
            ("Running from " + dir).Dump();
            Console.WriteLine();

            var versionFilePath = Path.Combine(dir, versionFileName);
            if (!File.Exists(versionFilePath))
            {
                throw new Exception("VersionInfo file not found at " + versionFilePath);
            }

            ("Version info file: " + versionFilePath).Dump();

            const string quotedVersionString = "\"" + versionString + "\"";
            var regex = new Regex(@"(?<=AssemblyVersion\()" + quotedVersionString + @"(?=\))", RegexOptions.Compiled);

            var v = GetVersionInfo(versionFilePath, regex);
            if (v == null)
                throw new Exception("AssemblyVersion of format \"#.#.#.[*#]\" not found.");

            ("Current version: " + v.ToString()).Dump();
            VersionBump(ref v, internalVersionUndoInfoPrefix);
            ("New version: " + v.ToString()).Dump();

            regex = new Regex(quotedVersionString, RegexOptions.Compiled);
            var versionFileData = File.ReadAllText(versionFilePath);
            var newVersionFileData = regex.Replace(versionFileData, string.Format("\"{0}\"", v.ToString()));
            Console.WriteLine();
            //newVersionFileData.Dump();
            try
            {
                File.WriteAllText(versionFilePath, newVersionFileData);
            }
            catch
            {
                ("Error occurred." + Environment.NewLine).Dump();
                TeeDumpWithHeader("Old VersionInfo data:", versionFileData);
                TeeDumpWithHeader("New VersionInfo data:", newVersionFileData);
                throw;
            }

            var ciConfigFilePath = Path.Combine(dir, ciConfigFileName);
            if (!File.Exists(ciConfigFilePath))
            {
                throw new Exception("CI Config file not found at " + ciConfigFilePath);
            }

            ("CI Config file: " + ciConfigFilePath).Dump();

            regex = new Regex(@"(?<=version:\s*')" + versionString + @"(?=')",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

            v = GetVersionInfo(ciConfigFilePath, regex);
            if (v == null)
                throw new Exception("CI Config Version of format \"#.#.#.*\" not found.");

            ("Current version: " + v).Dump();
            VersionBump(ref v, internalVersionUndoCIPrefix);
            ("New version: " + v).Dump();

            var configData = File.ReadAllText(ciConfigFilePath);
            var newConfigData = regex.Replace(configData, v.ToString());
            Console.WriteLine();
            //newConfigData.Dump();
            try
            {
                File.WriteAllText(ciConfigFilePath, newConfigData);
            }
            catch
            {
                ("Error occurred." + Environment.NewLine).Dump();
                TeeDumpWithHeader("Old CI Config data:", configData);
                TeeDumpWithHeader("New CI Config data:", newConfigData);
                throw;
            }
        }
        
        private const string internalVersionUndoInfoPrefix = "Version-Info";
        private const string internalVersionUndoCIPrefix = "Version-CI";
        
        private bool majorBump = false, minorBump = false, undo = false;
        private DirectoryInfo tempDir;

        private DirectoryInfo TempDir
        {
            get { return tempDir ?? (tempDir = GetOrCreateTempDirPath()); }
        }

        public void VersionBump(ref Version v, string prefix)
        {
            if (undo)
            {
                UndoVersion(ref v, prefix);
            }
            else
            {
                var versionFilePath = GenerateTempFilePath(prefix);
                File.WriteAllText(versionFilePath, v.ToString());

                if (majorBump)
                {
                    if (v.TryIncrement(ref v.Major))
                    {
                        v.Minor = v.Patch = 0.ToString();
                    }
                }
                else if (minorBump)
                {
                    if (v.TryIncrement(ref v.Minor))
                    {
                        v.Patch = 0.ToString();
                    }
                }
                else
                {
                    v.TryIncrement(ref v.Patch);
                }
            }
        }

        public Version GetVersionInfo(string filePath, Regex versionRegex)
        {
            Version v = null;
            foreach (var line in File.ReadLines(filePath))
            {
                var match = versionRegex.Match(line);
                if (match.Success)
                {
                    v = new Version()
                    {
                        Major = match.Groups[2].Value.Trim(),
                        Minor = match.Groups[3].Value.Trim(),
                        Patch = match.Groups[4].Value.Trim(),
                        BuildString = match.Groups[5].Value.Trim()
                    };
                    break;
                }
            }

            return v;
        }

        private static string GetMd5Hash(string s)
        {
            var md5 = System.Security.Cryptography.MD5.Create();
            var bytes = Encoding.Unicode.GetBytes(s);
            var hash = md5.ComputeHash(bytes);
            return BitConverter.ToString(hash);
        }

        private static string SanitizeString(string s, char[] invalidChars)
        {
            var sb = new StringBuilder(s.Length);
            foreach (var c in s)
            {
                if (!invalidChars.Contains(c))
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        private void UndoVersion(ref Version v, string prefix)
        {
            var files = Directory.GetFiles(TempDir.FullName);
            try
            {
                var file = files
                    .Select(x => new FileInfo(x))
                    .Where(x => x.Name.StartsWith(prefix))
                    .OrderBy(x => x.LastAccessTimeUtc)
                    .First();
                    
                var vInfo = File.ReadLines(file.FullName).First();
                v = Version.Parse(vInfo);
                file.Delete();
            }
            catch
            {
                "No previous versions found to undo.".Dump();
                Environment.Exit(1);
            }
        }

        private void TeeDumpWithHeader(string header, string contents)
        {
            Console.WriteLine(header);
            Console.WriteLine();
            contents.Dump();
            Console.WriteLine();
            var tempFilePath = GenerateTempFilePath(header);
            ("Dumping into: " + tempFilePath).Dump();
            File.WriteAllText(tempFilePath, contents);
        }

        private string GenerateTempFilePath(string prefix = null, string postFix = null)
        {
            var name = new StringBuilder();
            if (prefix != null) name.Append(prefix + "-");
            name.Append(Guid.NewGuid().ToString());
            if (postFix != null) name.Append("-" + postFix);
            var fileName = SanitizeString(name.ToString(), Path.GetInvalidFileNameChars());
            return Path.Combine(TempDir.FullName, fileName);
        }

        private DirectoryInfo GetOrCreateTempDirPath()
        {
            var tempPath = Path.GetTempPath();
#if !STANDALONE
            var sPath = Util.CurrentQuery.FilePath;
#else
            var sPath = Assembly.GetExecutingAssembly().CodeBase;
#endif
            var dataDir = GetMd5Hash(sPath);
            var dInfo = new DirectoryInfo(tempPath);
            var dir = dInfo.CreateSubdirectory(buildToolsName);
            var dir2 = dir.CreateSubdirectory(dataDir);
            return dir2;
        }

        private void ParseArgs(string[] args)
        {
            foreach (var arg in args)
            {
                if (arg == "/u")
                {
                    undo = true;
                }
                if (arg == "/major")
                {
                    majorBump = true;
                }

                if (arg == "/minor")
                {
                    minorBump = true;
                }
            }
        }

        public class Version
        {
            public string Major;
            public string Minor;
            public string Patch;
            public string BuildString;

            public override string ToString()
            {
                return string.Join(".", Major, Minor, Patch, BuildString);
            }

            public static Version Parse(string s)
            {
                var m = Regex.Match(s, versionString);
                if (!m.Success)
                {
                    throw new Exception("Invalid version info.");
                }

                var v = new Version()
                {
                    Major = m.Groups[2].Value.Trim(),
                    Minor = m.Groups[3].Value.Trim(),
                    Patch = m.Groups[4].Value.Trim(),
                    BuildString = m.Groups[5].Value.Trim()
                };

                return v;
            }

            public bool TryIncrement(ref string item)
            {
                int res;
                if (int.TryParse(item, out res))
                {
                    item = (++res).ToString();
                    return true;
                }
                return false;
            }

            public bool TryDecrement(ref string item)
            {
                int res;
                if (int.TryParse(item, out res))
                {
                    item = (--res).ToString();
                    return true;
                }
                return false;
            }
        }
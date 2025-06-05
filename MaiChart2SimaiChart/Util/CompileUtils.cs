using MaiLib;

namespace MaiChart2SimaiChart.Util;

public class CompileUtils
{
    public const int Success = 0;
    public const int Failed = 2;
    
    private static readonly object _sharedResourceLock = new();

    public class CompileDatabaseOption
    {
        public bool StrictDecimal { get; set; }
        public bool MusicIdFolderName { get; set; }
        public int CategorizeIndex { get; set; }
        public int ThreadCount { get; set; }

        public CompileDatabaseOption(
            bool strictDecimal,
            bool musicIdFolderName,
            int categorizeIndex,
            int threadCount
        )
        {
            StrictDecimal = strictDecimal;
            MusicIdFolderName = musicIdFolderName;
            CategorizeIndex = categorizeIndex;
            ThreadCount = threadCount;
        }
    }
    
    public static int CompileDatabaseWithNothing(string a000Path, string? output)
    {
        return CompileDatabase(a000Path, output, new CompileDatabaseOption(false, false, 0, 1));
    }

    public static int CompileDatabaseWithProgressBar(string a000Path, string? output, CompileDatabaseOption option)
    {
        var progressBar = new ConsoleProgressBar();
        return CompileDatabase(a000Path, output, option,
            not => progressBar.SetTotal(not),
            upt => progressBar.Update(),
            () => progressBar.Dispose()
        );
    }

    public static int CompileDatabase(
        string a000Path,
        string? output,
        CompileDatabaseOption option,
        Action<int>? onInit = null,
        Action<int>? onUpdate = null,
        Action? onFinish = null)
    {
        StaticSettings.CompiledTracks.Clear();
        StaticSettings.CompiledChart.Clear();

        if (a000Path.Equals(""))
        {
            a000Path = StaticSettings.DefaultPaths[0];
        }

        var musicLocation = Path.Combine(a000Path, "music");
        var originSoundLocation = Path.Combine(a000Path, "SoundData");
        var originImageLocation = Path.Combine(a000Path, "AssetBundleImages", "jacket");
        var originVideoLocation = Path.Combine(a000Path, "MovieData");

        var musicFolders = Directory.GetDirectories(musicLocation);

        var outputLocation = output ?? throw new NullReferenceException("Destination not specified");
        if (outputLocation.Equals(""))
        {
            outputLocation = StaticSettings.DefaultPaths[4];
        }

        try
        {
            if (0 <= option.CategorizeIndex && option.CategorizeIndex < StaticSettings.TrackCategorizeMethodSet.Length)
            {
                StaticSettings.GlobalTrackCategorizeMethod = StaticSettings.TrackCategorizeMethodSet[option.CategorizeIndex];
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message +
                              " The program will use Genre as default method. Press any key to continue.");
            StaticSettings.GlobalTrackCategorizeMethod = StaticSettings.TrackCategorizeMethodSet[0];
            option.CategorizeIndex = 0;
            Console.ReadKey();
        }

        if (option.ThreadCount <= 0)
        {
            option.ThreadCount = 1;
        }

        try
        {
            var musicFloderDict = new Dictionary<string, bool>();
            foreach (var folder in musicFolders) musicFloderDict.TryAdd(folder, false);

            var numberOfTracks = musicFolders.Length;
            if (onInit != null) onInit(numberOfTracks);
            try
            {
                var threadPool = Array.Empty<Thread>();
                for (var i = 0; i < option.ThreadCount; i++)
                {
                    var thread = new Thread(() =>
                    {
                        while (true)
                        {
                            string folderToCompile;
                            lock (musicFloderDict)
                            {
                                var uncompiledTrack = musicFloderDict.FirstOrDefault(x => !x.Value);
                                if (string.IsNullOrEmpty(uncompiledTrack.Key)) break;
                                folderToCompile = uncompiledTrack.Key;
                                musicFloderDict[folderToCompile] = true;
                            }

                            Compiler(folderToCompile, outputLocation, originSoundLocation, originImageLocation, option);
                            if (onUpdate != null) onUpdate(musicFloderDict.Sum(x => x.Value ? 1 : 0));
                        }
                    });
                    thread.Start();
                    Array.Resize(ref threadPool, threadPool.Length + 1);
                    threadPool[^1] = thread;
                }

                foreach (var thread in threadPool)
                {
                    thread.Join();
                }
            }
            finally
            {
                if (onFinish != null) onFinish();
            }

            Console.WriteLine("Total music compiled: {0}", StaticSettings.NumberTotalTrackCompiled);
            int index = 1;
            foreach (KeyValuePair<int, string> pair in StaticSettings.CompiledTracks)
            {
                Console.WriteLine($"[{index}]: {pair.Key} {pair.Value}");
                index++;
            }

            return Success;
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred during compilation: " + ex.Message);
            return Failed;
        }
    }

    private static void Compiler(
        string track,
        string outputLocation,
        string originSoundLocation,
        string originImageLocation,
        CompileDatabaseOption option
    )
    {
        Console.WriteLine("Iterating on folder {0}", track);
        var files = Directory.GetFiles(track);
        if (files.Length <= 1)
        {
            Console.WriteLine("Not enough files in the folder, skipping track: {0}", track);
            return;
        }

        if (!File.Exists(Path.Combine(track, "Music.xml")))
        {
            Console.WriteLine("There is no Music.xml in folder {0}", track);
            return;
        }

        TrackInformation trackInfo = new XmlInformation($"{track}/");
        Console.WriteLine("There is Music.xml in {0}", track);
        var shortID = StaticSettings.CompensateZero(trackInfo.TrackID).Substring(2);
        Console.WriteLine($"Name: {trackInfo.TrackName}");
        Console.WriteLine($"ID: {trackInfo.TrackID}");
        Console.WriteLine($"Genre: {trackInfo.TrackGenre}");

        string[] categorizeScheme =
        [
            trackInfo.TrackGenre, trackInfo.TrackSymbolicLevel, trackInfo.TrackVersion,
            trackInfo.TrackComposer, trackInfo.TrackBPM, trackInfo.StandardDeluxePrefix, ""
        ];

        var defaultCategorizedPath = Path.Combine(outputLocation, categorizeScheme[option.CategorizeIndex]);

        var trackNameSubstitute = option.MusicIdFolderName
            ? trackInfo.TrackID
            : $"{trackInfo.TrackID}_{trackInfo.TrackSortName}";

        if (!Directory.Exists(defaultCategorizedPath))
        {
            Directory.CreateDirectory(defaultCategorizedPath);
            Console.WriteLine("Created folder: {0}", defaultCategorizedPath);
        }
        else
        {
            Console.WriteLine("Already exist folder: {0}", defaultCategorizedPath);
        }

        var trackPath = option.MusicIdFolderName
            ? Path.Combine(defaultCategorizedPath, trackNameSubstitute)
            : Path.Combine(defaultCategorizedPath, trackNameSubstitute + trackInfo.DXChartTrackPathSuffix);

        if (!Directory.Exists(trackPath))
        {
            Directory.CreateDirectory(trackPath);
            Console.WriteLine("Created song folder: {0}", trackPath);
        }
        else
        {
            Console.WriteLine("Already exist song folder: {0}", trackPath);
        }

        SimaiCompiler compiler;
        if (trackInfo.InformationDict["Utage"] != "")
        {
            compiler = new SimaiCompiler(option.StrictDecimal, Path.Combine(track),
                Path.Combine(defaultCategorizedPath, trackNameSubstitute + "_Utage"), true);
            compiler.WriteOut(trackPath, true);
        }
        else
        {
            compiler = new SimaiCompiler(option.StrictDecimal, Path.Combine(track), trackPath);
            compiler.WriteOut(trackPath, true);
            StaticSettings.CompiledChart.Add(compiler.GenerateOneLineSummary());
        }

        Console.WriteLine("Finished compiling maidata {0} to: {1}", trackInfo.TrackName,
            Path.Combine(trackPath, "maidata.txt"));

        Console.WriteLine($"Convert music {trackInfo.TrackName}");
        string wavPath;
        
        lock (_sharedResourceLock)
        {
            wavPath = Task.Run(() =>
                    AudioConvert.GetCachedWavPath(originSoundLocation, int.Parse(trackInfo.TrackID)))
                .GetAwaiter()
                .GetResult();
        }

        AudioConvert.ConvertWavPathToMp3Stream(
            wavPath,
            new FileStream(Path.Combine(trackPath, "music.mp3"), FileMode.Create));

        Console.WriteLine($"Convert jacket {trackInfo.TrackName}");
        byte[]? img;

        lock (_sharedResourceLock)
        {
            img = ImageConvert.GetMusicJacketPngData(
                $"{originImageLocation}/ui_jacket_{int.Parse(trackInfo.TrackID) % 10000:000000}.ab");
        }

        if (img is not null)
            File.WriteAllBytes(Path.Combine(trackPath, "bg.png"), img);
    }
}
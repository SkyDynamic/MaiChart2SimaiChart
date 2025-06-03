using MaiChart2SimaiChart.Util;
using MaiLib;
using ManyConsole;

namespace MaiChart2SimaiChart.Commands;

public class CompileDatabase : ConsoleCommand
{
    public const int Success = 0;
    public const int Failed = 2;
    public bool StrictDecimal { get; set; }
    public bool MusicIdFolderName { get; set; }

    public string? A000Location { get; set; }

    public string? Difficulty { get; set; }

    public int CategorizeIndex { get; set; }

    public string? Destination { get; set; }

    public string CategorizeMethods { get; set; }

    public string? Rotate { get; set; }

    public int? ShiftTick { get; set; }
    
    public int ThreadCount { get; set; }

    public CompileDatabase()
    {
        CategorizeMethods = "";
        for (int i = 0; i < Program.TrackCategorizeMethodSet.Length; i++)
        {
            CategorizeMethods += $"[{i}]{Program.TrackCategorizeMethodSet[i]}\n";
        }

        IsCommand("CompileDatabase", "Compile whole ma2 database to format assigned");

        HasLongDescription(
            "This function enables user to compile whole database to the format they want. By default is simai for ma2.");
        HasRequiredOption("p|path=", "REQUIRED: Folder of A000 to override - end with a path separator",
            aPath => A000Location = aPath);
        HasRequiredOption("o|output=", "REQUIRED: Export compiled chart to location specified",
            dest => Destination = dest);
        HasOption("g|genre=", "The preferred categorizing scheme, includes:\n" + CategorizeMethods,
            genre => CategorizeIndex = int.Parse(genre));
        HasOption("r|rotate=",
            "Rotating method to rotate a chart: Clockwise90/180, Counterclockwise90/180, UpsideDown, LeftToRight",
            rotate => Rotate = rotate);
        HasOption("s|shift=", "Overall shift to the chart in unit of tick", tick => ShiftTick = int.Parse(tick));
        HasOption("d|decimal:", "Force output chart to have levels rated by decimal", _ => StrictDecimal = true);
        HasOption("n|number:", "Use musicID as folder name instead of sort name", _ => MusicIdFolderName = true);
        HasOption("t|thread:", "Number of threads to use for compiling, default is 1", threadCount =>
        {
            if (int.TryParse(threadCount, out var count) && count > 0)
                ThreadCount = count;
            else
                ThreadCount = 1;
        });
    }

    public override int Run(string[] remainingArguments)
    {
        try
        {
            string a000Location =
                A000Location ?? throw new FileNotFoundException("A000 location was not specified");

            if (a000Location.Equals(""))
            {
                a000Location = Program.DefaultPaths[0];
            }

            var musicLocation = Path.Combine(a000Location, "music");
            var originSoundLocation = Path.Combine(a000Location, "SoundData");
            var originImageLocation = Path.Combine(a000Location, "AssetBundleImages", "jacket");
            var originVideoLocation = Path.Combine(a000Location, "MovieData");

            string[] musicFolders = Directory.GetDirectories(musicLocation);

            string outputLocation = Destination ?? throw new NullReferenceException("Destination not specified");
            if (outputLocation.Equals(""))
            {
                outputLocation = Program.DefaultPaths[4];
            }

            try
            {
                if (0 <= CategorizeIndex && CategorizeIndex < Program.TrackCategorizeMethodSet.Length)
                {
                    Program.GlobalTrackCategorizeMethod = Program.TrackCategorizeMethodSet[CategorizeIndex];
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message +
                                  " The program will use Genre as default method. Press any key to continue.");
                Program.GlobalTrackCategorizeMethod = Program.TrackCategorizeMethodSet[0];
                CategorizeIndex = 0;
                Console.ReadKey();
            }

            // 新建一个字典，bool、string，string是musicFolders中的文件夹名，bool是是否已经编译过
            var musicFloderDict = new Dictionary<string, bool>();
            foreach (var folder in musicFolders) musicFloderDict.TryAdd(folder, false);

            int numberOfTracks = musicFolders.Length;
            using (var progressBar = new ConsoleProgressBar(numberOfTracks))
            {
                var threadPool = Array.Empty<Thread>();
                for (var i = 0; i < ThreadCount; i++)
                {
                    var thread = new Thread(() =>
                    {
                        while (true)
                        {
                            // 从dict中取出一个未编译的文件夹
                            var uncompiledTrack = musicFloderDict.FirstOrDefault(x => !x.Value);
                            if (string.IsNullOrEmpty(uncompiledTrack.Key)) break;
                            // 立即设置为已编译，防止其他线程重复编译
                            musicFloderDict[uncompiledTrack.Key] = true;
                            // 编译
                            Compiler(uncompiledTrack.Key, outputLocation, originSoundLocation, originImageLocation);
                            // 更新进度条
                            progressBar.Update();
                        }
                    });
                    thread.Start();
                    Array.Resize(ref threadPool, threadPool.Length + 1);
                    threadPool[^1] = thread;
                }
                // 等待所有线程完成
                foreach (var thread in threadPool)
                {
                    thread.Join();
                }
                /*
                foreach (var track in musicFolders)
                {
                    Console.WriteLine("Iterating on folder {0}", track);
                    // Check the file status
                    string[] files = Directory.GetFiles(track);
                    if (files.Length <= 1)
                    {
                        Console.WriteLine("Not enough files in the folder, skipping track: {0}", track);
                        progressBar.Update();
                        continue;
                    }

                    if (File.Exists(Path.Combine(track, "Music.xml")))
                    {
                        TrackInformation trackInfo = new XmlInformation($"{track}/");
                        Console.WriteLine("There is Music.xml in {0}", track);
                        string shortID = Program.CompensateZero(trackInfo.TrackID).Substring(2);
                        Console.WriteLine($"Name: {trackInfo.TrackName}");
                        Console.WriteLine($"ID: {trackInfo.TrackID}");
                        Console.WriteLine($"Genre: {trackInfo.TrackGenre}");
                        string[] categorizeScheme =
                        [
                            trackInfo.TrackGenre, trackInfo.TrackSymbolicLevel, trackInfo.TrackVersion,
                            trackInfo.TrackComposer, trackInfo.TrackBPM, trackInfo.StandardDeluxePrefix, ""
                        ];
                        var defaultCategorizedPath = Path.Combine(outputLocation, categorizeScheme[CategorizeIndex]);

                        string trackNameSubstitute = MusicIdFolderName
                            ? trackInfo.TrackID
                            : $"{trackInfo.TrackID}_{trackInfo.TrackSortName}";

                        if (!Directory.Exists(defaultCategorizedPath))
                        {
                            Directory.CreateDirectory(defaultCategorizedPath);
                            Console.WriteLine("Created folder: {0}", defaultCategorizedPath);
                        }
                        else Console.WriteLine("Already exist folder: {0}", defaultCategorizedPath);


                        var trackPath = MusicIdFolderName
                            ? Path.Combine(defaultCategorizedPath, trackNameSubstitute)
                            : Path.Combine(defaultCategorizedPath,
                                trackNameSubstitute + trackInfo.DXChartTrackPathSuffix);

                        if (!Directory.Exists(trackPath))
                        {
                            Directory.CreateDirectory(trackPath);
                            Console.WriteLine("Created song folder: {0}", trackPath);
                        }
                        else Console.WriteLine("Already exist song folder: {0}", trackPath);


                        SimaiCompiler compiler;
                        if (trackInfo.InformationDict["Utage"] != "")
                        {
                            compiler = new SimaiCompiler(StrictDecimal, Path.Combine(track),
                                Path.Combine(defaultCategorizedPath, trackNameSubstitute + "_Utage"), true);
                            compiler.WriteOut(trackPath, true);
                        }
                        else
                        {
                            compiler = new SimaiCompiler(StrictDecimal, Path.Combine(track), trackPath);
                            compiler.WriteOut(trackPath, true);
                            Program.CompiledChart.Add(compiler.GenerateOneLineSummary());
                        }

                        Console.WriteLine("Finished compiling maidata {0} to: {1}", trackInfo.TrackName,
                            Path.Combine(trackPath, "maidata.txt"));

                        Console.WriteLine($"Convert music {trackInfo.TrackName}");
                        var wavPath = Task.Run(() =>
                                AudioConvert.GetCachedWavPath(originSoundLocation, int.Parse(trackInfo.TrackID)))
                            .GetAwaiter()
                            .GetResult();
                        AudioConvert.ConvertWavPathToMp3Stream(
                            wavPath,
                            new FileStream(Path.Combine(trackPath, "music.mp3"), FileMode.Create));

                        Console.WriteLine($"Convert jacket {trackInfo.TrackName}");
                        var img = ImageConvert.GetMusicJacketPngData(
                            $"{originImageLocation}/ui_jacket_{int.Parse(trackInfo.TrackID) % 10000:000000}.ab");
                        if (img is not null) File.WriteAllBytes(Path.Combine(trackPath, "bg.png"), img);
                    }
                    else
                    {
                        Console.WriteLine("There is no Music.xml in folder {0}", track);
                    }

                    progressBar.Update();
                }*/
            }
            
            Console.WriteLine("Total music compiled: {0}", Program.NumberTotalTrackCompiled);
            int index = 1;
            foreach (KeyValuePair<int, string> pair in Program.CompiledTracks)
            {
                Console.WriteLine($"[{index}]: {pair.Key} {pair.Value}");
                index++;
            }

            return Success;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Program cannot proceed because of following error returned: \n{0}", ex.GetType());
            Console.Error.WriteLine(ex.Message);
            Console.Error.WriteLine(ex.StackTrace);
            Console.ReadKey();
            return Failed;
        }
    }

    private void Compiler(string track, string outputLocation, string originSoundLocation, string originImageLocation)
    {
        Console.WriteLine("Iterating on folder {0}", track);
        // Check the file status
        string[] files = Directory.GetFiles(track);
        if (files.Length <= 1)
        {
            Console.WriteLine("Not enough files in the folder, skipping track: {0}", track);
        }

        if (File.Exists(Path.Combine(track, "Music.xml")))
        {
            TrackInformation trackInfo = new XmlInformation($"{track}/");
            Console.WriteLine("There is Music.xml in {0}", track);
            string shortID = Program.CompensateZero(trackInfo.TrackID).Substring(2);
            Console.WriteLine($"Name: {trackInfo.TrackName}");
            Console.WriteLine($"ID: {trackInfo.TrackID}");
            Console.WriteLine($"Genre: {trackInfo.TrackGenre}");
            string[] categorizeScheme =
            [
                trackInfo.TrackGenre, trackInfo.TrackSymbolicLevel, trackInfo.TrackVersion,
                trackInfo.TrackComposer, trackInfo.TrackBPM, trackInfo.StandardDeluxePrefix, ""
            ];
            var defaultCategorizedPath = Path.Combine(outputLocation, categorizeScheme[CategorizeIndex]);

            string trackNameSubstitute = MusicIdFolderName
                ? trackInfo.TrackID
                : $"{trackInfo.TrackID}_{trackInfo.TrackSortName}";

            if (!Directory.Exists(defaultCategorizedPath))
            {
                Directory.CreateDirectory(defaultCategorizedPath);
                Console.WriteLine("Created folder: {0}", defaultCategorizedPath);
            }
            else Console.WriteLine("Already exist folder: {0}", defaultCategorizedPath);


            var trackPath = MusicIdFolderName
                ? Path.Combine(defaultCategorizedPath, trackNameSubstitute)
                : Path.Combine(defaultCategorizedPath, trackNameSubstitute + trackInfo.DXChartTrackPathSuffix);

            if (!Directory.Exists(trackPath))
            {
                Directory.CreateDirectory(trackPath);
                Console.WriteLine("Created song folder: {0}", trackPath);
            }
            else Console.WriteLine("Already exist song folder: {0}", trackPath);


            SimaiCompiler compiler;
            if (trackInfo.InformationDict["Utage"] != "")
            {
                compiler = new SimaiCompiler(StrictDecimal, Path.Combine(track),
                    Path.Combine(defaultCategorizedPath, trackNameSubstitute + "_Utage"), true);
                compiler.WriteOut(trackPath, true);
            }
            else
            {
                compiler = new SimaiCompiler(StrictDecimal, Path.Combine(track), trackPath);
                compiler.WriteOut(trackPath, true);
                Program.CompiledChart.Add(compiler.GenerateOneLineSummary());
            }

            Console.WriteLine("Finished compiling maidata {0} to: {1}", trackInfo.TrackName,
                Path.Combine(trackPath, "maidata.txt"));

            Console.WriteLine($"Convert music {trackInfo.TrackName}");
            var wavPath = Task.Run(() =>
                    AudioConvert.GetCachedWavPath(originSoundLocation, int.Parse(trackInfo.TrackID)))
                .GetAwaiter()
                .GetResult();
            AudioConvert.ConvertWavPathToMp3Stream(
                wavPath,
                new FileStream(Path.Combine(trackPath, "music.mp3"), FileMode.Create));

            Console.WriteLine($"Convert jacket {trackInfo.TrackName}");
            var img = ImageConvert.GetMusicJacketPngData(
                $"{originImageLocation}/ui_jacket_{int.Parse(trackInfo.TrackID) % 10000:000000}.ab");
            if (img is not null) File.WriteAllBytes(Path.Combine(trackPath, "bg.png"), img);
        }
        else
        {
            Console.WriteLine("There is no Music.xml in folder {0}", track);
        }
    }
}
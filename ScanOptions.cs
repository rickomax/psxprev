using Newtonsoft.Json;

namespace PSXPrev
{
    [JsonObject]
    public class ScanOptions
    {
        public const string DefaultFilter = "*.*";
        public const string EmptyFilter = "*";

        // These settings are only present for loading and saving purposes.
        // File path:
        [JsonProperty("path")]
        public string Path { get; set; } = string.Empty;
        [JsonProperty("filter")]
        public string Filter { get; set; } = DefaultFilter;

        // Scanner formats:
        [JsonIgnore]
        public bool CheckAll => !CheckAN && !CheckBFF && !CheckHMD && !CheckMOD && !CheckPMD && !CheckPSX && !CheckTIM && !CheckTMD && !CheckTOD && !CheckVDF;

        [JsonProperty("formatAN")]
        public bool CheckAN { get; set; }
        [JsonProperty("formatBFF")]
        public bool CheckBFF { get; set; }
        [JsonProperty("formatHMD")]
        public bool CheckHMD { get; set; }
        [JsonProperty("formatMOD")]
        public bool CheckMOD { get; set; } // Previously called Croc
        [JsonProperty("formatPMD")]
        public bool CheckPMD { get; set; }
        [JsonProperty("formatPSX")]
        public bool CheckPSX { get; set; }
        [JsonProperty("formatTIM")]
        public bool CheckTIM { get; set; }
        [JsonProperty("formatTMD")]
        public bool CheckTMD { get; set; }
        [JsonProperty("formatTOD")]
        public bool CheckTOD { get; set; }
        [JsonProperty("formatVDF")]
        public bool CheckVDF { get; set; }

        // Scanner options:
        [JsonProperty("ignoreHMDVersion")]
        public bool IgnoreHMDVersion { get; set; } = false;
        [JsonProperty("ignoreTIMVersion")]
        public bool IgnoreTIMVersion { get; set; } = false;
        [JsonProperty("ignoreTMDVersion")]
        public bool IgnoreTMDVersion { get; set; } = false;

        [JsonProperty("fileOffsetAlign")]
        public long Alignment { get; set; } = 1;

        // We want nullables, but we also want to preserve the last-used values the UI when null.
        [JsonProperty("fileOffsetHasStart")]
        public bool StartOffsetHasValue { get; set; } = false;
        [JsonProperty("fileOffsetHasStop")]
        public bool StopOffsetHasValue { get; set; } = false;
        [JsonProperty("fileOffsetStart")]
        public long StartOffsetValue { get; set; } = 0;
        [JsonProperty("fileOffsetStop")]
        public long StopOffsetValue { get; set; } = 1;
        [JsonIgnore]
        public long StartOffset => StartOffsetHasValue ? StartOffsetValue : 0;
        [JsonIgnore]
        public long? StopOffset => StopOffsetHasValue ? (long?)StopOffsetValue : null;

        [JsonProperty("fileOffsetOnlyStart")]
        public bool StartOffsetOnly { get; set; } = false;
        [JsonProperty("fileOffsetNext")]
        public bool NextOffset { get; set; } = false;

        [JsonProperty("fileScanAsync")]
        public bool AsyncFileScan { get; set; } = true;
        [JsonProperty("fileSearchTopDown")]
        public bool TopDownFileSearch { get; set; } = true; // AKA depth-first
        [JsonProperty("fileSearchISOContents")]
        public bool ReadISOContents { get; set; } = true; // Disable this by default when parsing command line arguments
        [JsonProperty("fileSearchBINContents")]
        public bool ReadBINContents { get; set; } = false;
        [JsonProperty("binSectorAlign")]
        public bool BINAlignToSector { get; set; } = false;

        // We want nullables, but we also want to preserve the last-used values in the UI when null.
        [JsonProperty("binSectorHasStartSize")]
        public bool BINSectorUserStartSizeHasValue { get; set; } = false;
        [JsonProperty("binSectorStart")]
        public int BINSectorUserStartValue { get; set; } = Common.Parsers.BinCDStream.SectorUserStart;
        [JsonProperty("binSectorSize")]
        public int BINSectorUserSizeValue { get; set; } = Common.Parsers.BinCDStream.SectorUserSize;
        [JsonIgnore]
        public int BINSectorUserStart => BINSectorUserStartSizeHasValue ? BINSectorUserStartValue : Common.Parsers.BinCDStream.SectorUserStart;
        [JsonIgnore]
        public int BINSectorUserSize  => BINSectorUserStartSizeHasValue ? BINSectorUserSizeValue  : Common.Parsers.BinCDStream.SectorUserSize;

        // Log options:
        [JsonProperty("logToFile")]
        public bool LogToFile { get; set; } = false;
        [JsonProperty("logToConsole")]
        public bool LogToConsole { get; set; } = true;
        [JsonProperty("consoleColor")]
        public bool UseConsoleColor { get; set; } = true;
        [JsonProperty("debugLogging")]
        public bool DebugLogging { get; set; } = false;
        [JsonProperty("errorLogging")]
        public bool ErrorLogging { get; set; } = false;

        // Program options:
        [JsonProperty("drawAllToVRAM")]
        public bool DrawAllToVRAM { get; set; } = false;
        [JsonProperty("fixUVAlignment")]
        public bool FixUVAlignment { get; set; } = true;


        public void Validate()
        {
            if (Path == null)
            {
                Path = string.Empty;
            }

            if (Filter == null)
            {
                Filter = DefaultFilter;
            }
            else if (string.IsNullOrWhiteSpace(Filter))
            {
                Filter = EmptyFilter; // When filter is empty, default to matching all files (with or without an extension).
            }
        }

        public ScanOptions Clone()
        {
            return (ScanOptions)MemberwiseClone();
        }
    }
}

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
        [JsonProperty("fileOffsetStart")]
        public long? StartOffset { get; set; } = null;
        [JsonProperty("fileOffsetStop")]
        public long? StopOffset { get; set; } = null;
        [JsonProperty("fileOffsetNone")]
        public bool NoOffset { get; set; } = false;
        [JsonProperty("fileOffsetNext")]
        public bool NextOffset { get; set; } = false;

        [JsonProperty("fileScanAsync")]
        public bool AsyncFileScan { get; set; } = true;
        [JsonProperty("fileSearchDepthFirst")]
        public bool DepthFirstFileSearch { get; set; } = true; // AKA top-down
        [JsonProperty("fileSearchISOContents")]
        public bool ReadISOContents { get; set; } = false;
        [JsonProperty("fileSearchBINContents")]
        public bool ReadBINContents { get; set; } = false;
        [JsonProperty("binSectorAlign")]
        public bool BINAlignToSector { get; set; } = false;
        [JsonProperty("binSectorUserStart")]
        public int? BINSectorUserStart = null;
        [JsonProperty("binSectorUserSize")]
        public int? BINSectorUserSize = null;

        // Log options:
        [JsonProperty("logToFile")]
        public bool LogToFile { get; set; } = false;
        [JsonProperty("logToConsole")]
        public bool LogToConsole { get; set; } = true;
        [JsonProperty("consoleColor")]
        public bool UseConsoleColor { get; set; } = true;
        [JsonProperty("debug")]
        public bool Debug { get; set; } = false;
        [JsonProperty("showErrors")]
        public bool ShowErrors { get; set; } = false;

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

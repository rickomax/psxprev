using Newtonsoft.Json;

namespace PSXPrev
{
    [JsonObject]
    public class ScanOptions
    {
        public const string DefaultFilter = "*.*";

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

        [JsonProperty("startFileOffset")]
        public long? StartOffset { get; set; } = null;
        [JsonProperty("stopFileOffset")]
        public long? StopOffset { get; set; } = null;
        [JsonProperty("nextFileOffset")]
        public bool NextOffset { get; set; } = false;

        [JsonProperty("depthFirstFileSearch")]
        public bool DepthFirstFileSearch { get; set; } = true; // AKA top-down
        [JsonProperty("asyncFileScan")]
        public bool AsyncFileScan { get; set; } = true;

        // Log options:
        [JsonProperty("logToFile")]
        public bool LogToFile { get; set; } = false;
        [JsonProperty("logToConsole")]
        public bool LogToConsole { get; set; } = true;
        [JsonProperty("debug")]
        public bool Debug { get; set; } = false;
        [JsonProperty("showErrors")]
        public bool ShowErrors { get; set; } = false;
        [JsonProperty("consoleColor")]
        public bool ConsoleColor { get; set; } = true;

        // Program options:
        [JsonProperty("drawAllToVRAM")]
        public bool DrawAllToVRAM { get; set; } = false;
        [JsonProperty("fixUVAlignment")]
        public bool FixUVAlignment { get; set; } = true;


        public ScanOptions Clone()
        {
            return (ScanOptions)MemberwiseClone();
        }
    }
}

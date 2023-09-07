using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using PSXPrev.Common.Utils;

namespace PSXPrev
{
    [JsonObject]
    public class ScanOptions : IEquatable<ScanOptions>
    {
        public static readonly ScanOptions Defaults = new ScanOptions();

        public const string DefaultFilter = "*.*";
        public const string EmptyFilter = "*";
        public const string DefaultRegexPattern = ".*";

        // Use a timeout to avoid user-specified runaway Regex patterns.
        private static readonly TimeSpan MatchTimeout = TimeSpan.FromMilliseconds(100);

        // Equality checking and optimization.
        [JsonIgnore]
        private string _equalityString;
        [JsonIgnore]
        private int _equalityStringHashCode;
        [JsonIgnore]
        private bool _isReadOnly;
        // Set to true to claim that fields will not be changed. Used to optimize equality checks.
        [JsonIgnore]
        public bool IsReadOnly
        {
            get => _isReadOnly;
            set
            {
                _isReadOnly = value;
                if (!_isReadOnly)
                {
                    _equalityString = null;
                    _equalityStringHashCode = 0;
                }
            }
        }

        // Optional display name to assign to history
        [JsonProperty("displayName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string DisplayName { get; set; } = null;
        // Optionally prevent this from being removed from the history list
        [JsonProperty("bookmarked", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool IsBookmarked { get; set; } = false;


        // These settings are only present for loading and saving purposes.
        // File path:
        [JsonProperty("path")]
        public string Path { get; set; } = string.Empty;
        [JsonProperty("wildcardFilter")]
        public string WildcardFilter { get; set; } = DefaultFilter;
        [JsonProperty("regexFilter")]
        public string RegexPattern { get; set; } = DefaultRegexPattern;
        [JsonIgnore]
        public string ValidatedWildcardFilter
        {
            get
            {
                if (WildcardFilter == null)
                {
                    return DefaultFilter;
                }
                else if (string.IsNullOrWhiteSpace(WildcardFilter))
                {
                    return EmptyFilter;
                }
                return WildcardFilter;
            }
        }
        [JsonIgnore]
        public string ValidatedRegexPattern
        {
            get
            {
                if (string.IsNullOrEmpty(RegexPattern))
                {
                    return DefaultRegexPattern;
                }
                return RegexPattern;
            }
        }
        [JsonProperty("useRegexFilter")]
        public bool UseRegex { get; set; } = false;

        // Scanner formats:
        [JsonIgnore]
        public bool CheckAll
        {
            get
            {
                return !CheckAN && !CheckBFF && !CheckHMD && !CheckMOD && !CheckPMD && !CheckPSX &&
                       !CheckSPT && !CheckTIM && !CheckTMD && !CheckTOD && !CheckVDF;
            }
        }

        public List<string> GetCheckedFormats(bool groupAll)
        {
            var formats = new List<string>();
            if (groupAll && CheckAll)
            {
                formats.Add("Any");
                return formats;
            }
            if (CheckAN)  formats.Add("AN");
            if (CheckBFF) formats.Add("BFF");
            if (CheckHMD) formats.Add("HMD");
            if (CheckMOD) formats.Add("MOD");
            if (CheckPMD) formats.Add("PMD");
            if (CheckPSX) formats.Add("PSX");
            if (CheckSPT) formats.Add("SPT");
            if (CheckTIM) formats.Add("TIM");
            if (CheckTMD) formats.Add("TMD");
            if (CheckTOD) formats.Add("TOD");
            if (CheckVDF) formats.Add("VDF");
            if (groupAll && formats.Count == 11)
            {
                formats.Clear();
                formats.Add("All");
            }
            return formats;
        }

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
        [JsonProperty("formatSPT")]
        public bool CheckSPT { get; set; }
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

        [JsonProperty("fileOffsetStartOnly")]
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
        public bool ReadBINContents { get; set; } = false; // Read indexed files instead of data of BIN file.
        [JsonProperty("fileSearchBINSectorData")]
        public bool ReadBINSectorData { get; set; } = false;

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


        public Regex GetRegexFilter(bool allowErrors)
        {
            try
            {
                if (!UseRegex)
                {
                    return StringUtils.WildcardToRegex(ValidatedWildcardFilter, MatchTimeout);
                }
                else
                {
                    return new Regex(ValidatedRegexPattern, RegexOptions.IgnoreCase, MatchTimeout);
                }
            }
            catch
            {
                if (!allowErrors)
                {
                    throw;
                }
                return null;
            }
        }

        public void Validate()
        {
            if (Path == null)
            {
                Path = string.Empty;
            }

            WildcardFilter = ValidatedWildcardFilter;
            RegexPattern   = ValidatedRegexPattern;
        }

        public override string ToString()
        {
            return ToString(-1);
        }

        public string ToString(int maxPathLength)
        {
            if (!string.IsNullOrEmpty(DisplayName))
            {
                return DisplayName;
            }

            var parts = new List<string>();

            string pathStr = null;
            if (maxPathLength != 0)
            {
                pathStr = Path ?? string.Empty;
                if (maxPathLength > 0)
                {
                    // Trim path by directories starting from the root.
                    // Add as many directories as possible until we hit the length cap.
                    var index = pathStr.Length;
                    do
                    {
                        // Alt: Allow overflow of at most one extra directory
                        //var length = pathStr.Length - index;
                        //if (length >= maxPathLength)
                        //{
                        //    break;
                        //}

                        var sep = Math.Max(pathStr.LastIndexOf('\\', index - 1), pathStr.LastIndexOf('/', index - 1));
                        sep = Math.Max(0, sep);

                        // Current: Allow overflow for the file name only
                        var length = pathStr.Length - sep;
                        if (index != pathStr.Length && length > maxPathLength)
                        {
                            break;
                        }

                        index = sep;
                    }
                    while (index > 0);

                    // Don't bother using a substring if we're replacing the same number of characters with dots
                    // (technically the dots would take up less space though)
                    if (index > 3)
                    {
                        pathStr = "..." + pathStr.Substring(index);
                    }
                }
            }

            string filterStr = null;
            if (!UseRegex)
            {
                if (ValidatedWildcardFilter != EmptyFilter)
                {
                    filterStr = ValidatedWildcardFilter;
                }
            }
            else
            {
                if (ValidatedRegexPattern != DefaultRegexPattern)
                {
                    filterStr = $"/{ValidatedRegexPattern}/"; // Use JavaScript notation to express this is regex
                }
            }

            string formatsStr = null;
            var formats = GetCheckedFormats(true);
            if (formats.Count > 0)
            {
                formatsStr = string.Join(",", formats).ToLower();
            }


            if (formatsStr != null) parts.Add(formatsStr);
            if (filterStr != null) parts.Add(filterStr);
            if (pathStr != null) parts.Add(pathStr);


            return string.Join(" | ", parts);
        }

        public ScanOptions Clone()
        {
            return (ScanOptions)MemberwiseClone();
        }

        // Normalize settings for equality checks
        private void Normalize()
        {
            Validate();

            DisplayName = null;
            IsBookmarked = false;

            Path = Path?.ToLower();

            if (Alignment < 1)
            {
                Alignment = 1;
            }

            WildcardFilter = WildcardFilter?.ToLower();
            RegexPattern   = RegexPattern?.ToLower();
            /*if (!UseRegex)
            {
                WildcardFilter = WildcardFilter?.ToLower();
                RegexPattern   = Defaults.RegexPattern;
            }
            else
            {
                WildcardFilter = Defaults.WildcardFilter;
                RegexPattern   = RegexPattern?.ToLower();
            }*/

            /*if (!StartOffsetHasValue)
            {
                StartOffsetValue = Defaults.StartOffsetValue;
            }
            if (!StopOffsetHasValue)
            {
                StopOffsetValue = Defaults.StopOffsetValue;
            }
            if (!BINSectorUserStartSizeHasValue)
            {
                BINSectorUserStartValue = Defaults.BINSectorUserStartValue;
                BINSectorUserSizeValue  = Defaults.BINSectorUserSizeValue;
            }*/
            /*if (!LogToConsole)
            {
                UseConsoleColor = Defaults.UseConsoleColor;
            }*/
        }

        // It's easier to manage equality by *not* having to update it every time settings are changed.
        // This is a lazy solution, but requires near-zero maintenance.
        private string GetEqualityString(out int? hashCode)
        {
            string equalityString;
            hashCode = null;
            if (_equalityString != null)
            {
                equalityString = _equalityString;
                hashCode = _equalityStringHashCode;
            }
            else
            {
                var options = Clone();
                options.Normalize();

                // Use settings to reduce the size of the string
                var jsonSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.None,
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Ignore,
                };
                equalityString = JsonConvert.SerializeObject(options, jsonSettings);// Formatting.None);

                if (_isReadOnly)
                {
                    hashCode = equalityString.GetHashCode();
                    _equalityString = equalityString;
                    _equalityStringHashCode = hashCode.Value;
                }
            }
            return equalityString;
        }

        public bool Equals(ScanOptions other)
        {
            var str      = GetEqualityString(out var hashCode);
            var strOther = other.GetEqualityString(out var hashCodeOther);
            // If both equalities are cached, then we can compare hash codes first to save time
            return (!hashCode.HasValue || !hashCodeOther.HasValue || hashCode == hashCodeOther) && str == strOther;
        }
    }
}

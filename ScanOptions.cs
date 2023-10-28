using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PSXPrev.Common.Parsers;
using PSXPrev.Common.Utils;

namespace PSXPrev
{
    public static class ScanFormats
    {
        public const string AN  = ANParser.FormatNameConst;
        public const string BFF = BFFParser.FormatNameConst;
        //public const string CLT = CLTParser.FormatNameConst;
        public const string HMD = HMDParser.FormatNameConst;
        public const string MOD = MODParser.FormatNameConst;
        public const string PIL = PILParser.FormatNameConst;
        public const string PMD = PMDParser.FormatNameConst;
        public const string PSX = PSXParser.FormatNameConst;
        //public const string PXL = PXLParser.FormatNameConst;
        public const string SPT = SPTParser.FormatNameConst;
        public const string TIM = TIMParser.FormatNameConst;
        public const string TMD = TMDParser.FormatNameConst;
        public const string TOD = TODParser.FormatNameConst;
        public const string VDF = VDFParser.FormatNameConst;

        // note: CLT and PXL are not supported yet
        public static readonly string[] All =
        {
            AN, BFF, /*CLT,*/ HMD, MOD, PIL, PMD, PSX, /*PXL,*/ SPT, TIM, TMD, TOD, VDF,
        };

        // Formats that can only be used if explicitly selected
        // todo: AN produces too many false positives to be enabled by default
        public static readonly string[] Explicit =
        {
            SPT,
        };

        // Formats that are used when no format is selected
        // Setup during static constructor
        public static readonly string[] Implicit;

        public static int Count => All.Length;

        public static bool IsSupported(string format)
        {
            return Array.IndexOf(All, format) != -1;
        }

        public static bool IsExplicit(string format)
        {
            return Array.IndexOf(Explicit, format) != -1;
        }

        public static bool IsImplicit(string format)
        {
            return Array.IndexOf(Implicit, format) != -1;
        }


        static ScanFormats()
        {
            Array.Sort(All);
            Array.Sort(Explicit);

            var implicitFormats = new List<string>();
            foreach (var format in All)
            {
                if (!IsExplicit(format))
                {
                    implicitFormats.Add(format);
                }
            }
            Implicit = implicitFormats.ToArray();
        }
    }

    [JsonObject]
    public class ScanOptions : IEquatable<ScanOptions>, ICloneable
    {
        public static readonly ScanOptions Defaults = new ScanOptions();

        public const string DefaultFilter = "*"; //"*.*";
        public const string DefaultRegexPattern = "^.*$";

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
                if (string.IsNullOrWhiteSpace(WildcardFilter))
                {
                    return DefaultFilter;
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
        [JsonProperty("formats")]
        public List<string> Formats { get; set; } = new List<string>();

        // Formats with reduced strictness, such as ignoring versions/magic
        [JsonProperty("unstrictFormats")]
        public List<string> UnstrictFormats { get; set; } = new List<string>();

        public bool ContainsFormat(string format)
        {
            return ContainsFormat(format, out _);
        }

        public bool ContainsFormat(string format, out bool isImplicit)
        {
            if (Formats.Count > 0)
            {
                isImplicit = false;
                return Formats.Contains(format);
            }
            else
            {
                isImplicit = ScanFormats.IsImplicit(format);
                return isImplicit;
            }
        }

        public bool AddFormat(string format)
        {
            if (!Formats.Contains(format) && ScanFormats.IsSupported(format))
            {
                Formats.Add(format);
                Formats.Sort();
                return true;
            }
            return false;
        }

        public bool RemoveFormat(string format)
        {
            return Formats.Remove(format);
        }

        public bool ContainsUnstrict(string format)
        {
            return UnstrictFormats.Contains(format);
        }

        public bool AddUnstrict(string format)
        {
            if (!UnstrictFormats.Contains(format) && ScanFormats.IsSupported(format))
            {
                UnstrictFormats.Add(format);
                UnstrictFormats.Sort();
                return true;
            }
            return false;
        }

        public bool RemoveUnstrict(string format)
        {
            return UnstrictFormats.Remove(format);
        }

        public string[] GetCheckedFormats(bool groupAll)
        {
            if (Formats.Count == ScanFormats.Count && groupAll)
            {
                return new string[] { "All" };
            }
            else if (Formats.Count == 0)
            {
                if (groupAll)
                {
                    return new string[] { "Defaults" };
                }
                else
                {
                    return (string[])ScanFormats.Implicit.Clone();
                }
            }
            else
            {
                return Formats.ToArray();
            }
        }

        // Scanner options:
        [JsonProperty("fileOffsetAlign")]
        public long Alignment { get; set; } = 1;

        // We want nullables, but we also want to preserve the last-used values in the UI when null.
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
        public bool ReadBINSectorData { get; set; } = false; // Read BIN file as one large data file.

        // We want nullables, but we also want to preserve the last-used values in the UI when null.
        // These may be removed in the future because it turns out sector sizes are always the same.
        // The reason I thought it was different is because Star Ocean 2 uses an archive format that cuts out more of the user size.
        [JsonProperty("binSectorHasStartSize")]
        public bool BINSectorUserStartSizeHasValue { get; set; } = false;
        [JsonProperty("binSectorStart")]
        public int BINSectorUserStartValue { get; set; } = BinCDStream.SectorUserStart;
        [JsonProperty("binSectorSize")]
        public int BINSectorUserSizeValue { get; set; } = BinCDStream.SectorUserSize;
        [JsonIgnore]
        public int BINSectorUserStart => BINSectorUserStartSizeHasValue ? BINSectorUserStartValue : BinCDStream.SectorUserStart;
        [JsonIgnore]
        public int BINSectorUserSize  => BINSectorUserStartSizeHasValue ? BINSectorUserSizeValue  : BinCDStream.SectorUserSize;

        // Log options:
        [JsonProperty("logToFile")]
        public bool LogToFile { get; set; } = false;
        [JsonProperty("logToConsole")]
        public bool LogToConsole { get; set; } = true;
        [JsonProperty("debugLogging")]
        public bool DebugLogging { get; set; } = false;
        [JsonProperty("errorLogging")]
        public bool ErrorLogging { get; set; } = false;

        // Program options:
        [JsonProperty("drawAllToVRAM")]
        public bool DrawAllToVRAM { get; set; } = false;


        // Used for version upgrades to read properties that are no longer present in the current class
        [JsonExtensionData(ReadData = true, WriteData = false)]
        private Dictionary<string, JToken> _unknownData;

        //[OnDeserialized]
        //private void OnDeserializedMethod(StreamingContext context)
        //{
        //}

        public void ValidateDeserialization(uint version)
        {
            Formats         = ValidateFormats(Formats);
            UnstrictFormats = ValidateFormats(UnstrictFormats);

            if (version <= 2 && _unknownData != null)
            {
                UpgradeFormatsToVersion3(version);
            }

            // Handle normal validation that's used outside of just deserialization
            Validate();

            _unknownData = null; // We don't need this anymore
        }

        // Remove null, unsupported, and duplicate format names, then ensure formats are sorted
        private static List<string> ValidateFormats(List<string> formats)
        {
            if (formats == null)
            {
                formats = new List<string>();
            }
            else
            {
                var duplicates = new HashSet<string>();
                for (var i = 0; i < formats.Count; i++)
                {
                    var format = formats[i];
                    if (format == null || !ScanFormats.IsSupported(format) || !duplicates.Add(format))
                    {
                        formats.RemoveAt(i);
                        i--;
                    }
                }
                formats.Sort();
            }
            return formats;
        }

        // Previously formats were stored in individual "format___": bool properties.
        // Handle switching to list storage.
        private void UpgradeFormatsToVersion3(uint version)
        {
            void UpgradeFormat(string propertyName, string format)
            {
                if (_unknownData.TryGetValue(propertyName, out var token))
                {
                    _unknownData.Remove(propertyName); // Not unknown data anymore
                    if (token.Type == JTokenType.Boolean && token.ToObject<bool>())
                    {
                        AddFormat(format);
                        // PIL format was introduced in the middle of version 2,
                        // and was grouped together with BFF.
                        if (version == 2 && format == BFFParser.FormatNameConst)
                        {
                            AddFormat(PILParser.FormatNameConst);
                        }
                    }
                }
            }

            void UpgradeUnstrict(string propertyName, string format)
            {
                if (_unknownData.TryGetValue(propertyName, out var token))
                {
                    _unknownData.Remove(propertyName); // Not unknown data anymore
                    if (token.Type == JTokenType.Boolean && token.ToObject<bool>())
                    {
                        AddUnstrict(format);
                    }
                }
            }

            UpgradeFormat("formatAN",  ANParser.FormatNameConst);
            UpgradeFormat("formatBFF", BFFParser.FormatNameConst);
            UpgradeFormat("formatHMD", HMDParser.FormatNameConst);
            UpgradeFormat("formatMOD", MODParser.FormatNameConst);
            UpgradeFormat("formatPMD", PMDParser.FormatNameConst);
            UpgradeFormat("formatPSX", PSXParser.FormatNameConst);
            if (version == 2)
            {
                // Format introduced in version 2
                UpgradeFormat("formatSPT", SPTParser.FormatNameConst);
            }
            UpgradeFormat("formatTIM", TIMParser.FormatNameConst);
            UpgradeFormat("formatTMD", TMDParser.FormatNameConst);
            UpgradeFormat("formatTOD", TODParser.FormatNameConst);
            UpgradeFormat("formatVDF", VDFParser.FormatNameConst);

            UpgradeUnstrict("ignoreHMDVersion", HMDParser.FormatNameConst);
            UpgradeUnstrict("ignoreTIMVersion", TIMParser.FormatNameConst);
            UpgradeUnstrict("ignoreTMDVersion", TMDParser.FormatNameConst);
        }


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

            if (string.IsNullOrEmpty(DisplayName))
            {
                DisplayName = null;
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
                if (ValidatedWildcardFilter != DefaultFilter)
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
            if (formats.Length > 0)
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
            var options = (ScanOptions)MemberwiseClone();
            options._unknownData = null;
            options.Formats = new List<string>(Formats);
            options.UnstrictFormats = new List<string>(UnstrictFormats);
            return options;
        }

        object ICloneable.Clone() => Clone();

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
        }

        // It's easier to manage equality by *not* having to update the function every time settings are added.
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

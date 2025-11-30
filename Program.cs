using System.Text;

class Program
{
    static int Main()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        while (true)
        {
            Rename();
        }
    }

    static void Rename()
    {
        Console.WriteLine("Enter path:");
        var root = Console.ReadLine();

        if (!Directory.Exists(root))
        {
            Console.WriteLine("Directory not found.");
            return;
        }

        var files = Directory.GetFiles(root, "*.mp3", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            try
            {
                var tfile = TagLib.File.Create(file);

                // -------------------------------
                // FIX ID3 TAGS (Cyrillic → Latin)
                // -------------------------------
                bool tagChanged = false;

                var rawArtist = tfile.Tag.FirstPerformer ?? "";
                var rawTitle = tfile.Tag.Title ?? "";

                if (ContainsCyrillic(rawArtist))
                {
                    var fixedArtist = Transliterate(rawArtist);
                    tfile.Tag.Performers = [fixedArtist];
                    tagChanged = true;
                }

                if (ContainsCyrillic(rawTitle))
                {
                    var fixedTitle = Transliterate(rawTitle);
                    tfile.Tag.Title = fixedTitle;
                    tagChanged = true;
                }

                if (tagChanged)
                    tfile.Save();

                // Re-read after potential ID3 changes
                var artist = Clean(tfile.Tag.FirstPerformer);
                var title = Clean(tfile.Tag.Title);
                var bitrate = tfile.Properties.AudioBitrate;

                // --------------------------------
                // Construct base name
                // --------------------------------
                string newName = !string.IsNullOrWhiteSpace(artist) && !string.IsNullOrWhiteSpace(title)
                    ? $"{artist} - {title}"
                    : Path.GetFileNameWithoutExtension(file);

                // Remove bracket sections
                newName = RemoveBracketSections(newName);

                // Mojibake fix only if needed
                if (LooksLikeMojibake1251(newName))
                    newName = FixMojibake1251(newName);

                // Transliterate if Cyrillic
                if (ContainsCyrillic(newName))
                    newName = Transliterate(newName);

                // Add bitrate
                newName = AddBitrate(newName, bitrate);

                // Cleanup spaces
                while (newName.Contains("  "))
                    newName = newName.Replace("  ", " ");

                newName = newName.Trim();

                // Ensure extension
                if (!newName.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
                    newName += ".mp3";

                var newPath = Path.Combine(Path.GetDirectoryName(file)!, newName);

                // Rename if different
                if (!file.Equals(newPath, StringComparison.OrdinalIgnoreCase))
                {
                    File.Move(file, newPath, overwrite: true);
                    Console.WriteLine($"Renamed: {newName}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing {file}: {ex.Message}");
            }
        }
    }

    // -----------------------------
    // Utility functions
    // -----------------------------

    static string Clean(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return "";
        foreach (var c in Path.GetInvalidFileNameChars())
            s = s.Replace(c, '_');
        return s.Trim();
    }

    static string AddBitrate(string name, int bitrate)
    {
        if (bitrate == 0) return name;
        var bitrateString = $"({bitrate}kbps)";
        name = name.Replace(".mp3", "");
        name = name.Replace(bitrateString, "");
        return $"{name} {bitrateString}".Trim();
    }

    static string RemoveBracketSections(string s)
        => System.Text.RegularExpressions.Regex.Replace(s, @"\[[^\]]*\]", "").Trim();

    static bool LooksLikeMojibake1251(string s)
    {
        // Extended Latin chars (0x80–0xFF) but no Cyrillic
        bool hasExtendedLatin = s.Any(ch => ch >= 0x80 && ch <= 0xFF);
        bool hasCyrillic = ContainsCyrillic(s);
        return hasExtendedLatin && !hasCyrillic;
    }

    static string FixMojibake1251(string s)
        => Encoding.GetEncoding(1251).GetString(Encoding.GetEncoding(1252).GetBytes(s));

    static bool ContainsCyrillic(string s)
        => s.Any(ch => ch >= 0x0400 && ch <= 0x04FF);

    static string Transliterate(string input)
    {
        var sb = new StringBuilder(input.Length);
        foreach (var ch in input)
        {
            var s = ch.ToString();
            if (CyrillicToLatin.TryGetValue(s, out var latin))
                sb.Append(latin);
            else
                sb.Append(s);
        }
        return sb.ToString();
    }

    static readonly Dictionary<string, string> CyrillicToLatin = new()
    {
        ["А"] = "A",
        ["Б"] = "B",
        ["В"] = "V",
        ["Г"] = "G",
        ["Д"] = "D",
        ["Е"] = "E",
        ["Ж"] = "Zh",
        ["З"] = "Z",
        ["И"] = "I",
        ["Й"] = "Y",
        ["К"] = "K",
        ["Л"] = "L",
        ["М"] = "M",
        ["Н"] = "N",
        ["О"] = "O",
        ["П"] = "P",
        ["Р"] = "R",
        ["С"] = "S",
        ["Т"] = "T",
        ["У"] = "U",
        ["Ф"] = "F",
        ["Х"] = "H",
        ["Ц"] = "Ts",
        ["Ч"] = "Ch",
        ["Ш"] = "Sh",
        ["Щ"] = "Sht",
        ["Ъ"] = "A",
        ["Ь"] = "",
        ["Ю"] = "Yu",
        ["Я"] = "Ya",

        ["а"] = "a",
        ["б"] = "b",
        ["в"] = "v",
        ["г"] = "g",
        ["д"] = "d",
        ["е"] = "e",
        ["ж"] = "zh",
        ["з"] = "z",
        ["и"] = "i",
        ["й"] = "y",
        ["к"] = "k",
        ["л"] = "l",
        ["м"] = "m",
        ["н"] = "n",
        ["о"] = "o",
        ["п"] = "p",
        ["р"] = "r",
        ["с"] = "s",
        ["т"] = "t",
        ["у"] = "u",
        ["ф"] = "f",
        ["х"] = "h",
        ["ц"] = "ts",
        ["ч"] = "ch",
        ["ш"] = "sh",
        ["щ"] = "sht",
        ["ъ"] = "a",
        ["ь"] = "",
        ["ю"] = "yu",
        ["я"] = "ya",
    };
}

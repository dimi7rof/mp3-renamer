# MP3 Renamer

[![.NET](https://img.shields.io/badge/dotnet-9.0-blue.svg)](https://dotnet.microsoft.com/)
[![Build](https://github.com/dimi7rof/mp3-renamer/actions/workflows/release.yml/badge.svg)](https://github.com/dimi7rof/mp3-renamer/actions/workflows/release.yml)
[![Release](https://img.shields.io/github/v/release/dimi7rof/mp3-renamer)](https://github.com/dimi7rof/mp3-renamer/releases)

A single-file .NET 9 console application that:

- Recursively finds `.mp3` files in a folder
- Reads ID3 tags (artist, title, bitrate)
- Removes `[bracketed text]`
- Fixes mojibake if filenames are mis-encoded
- Transliterates Cyrillic → Latin in filenames **and ID3 tags**
- Renames files to: {artist} - {title} ({bitrate}kbps).mp3

### Example

- Before: `Êåáà - Ïèòàø çà ìåí [www.example.com].mp3`
- After: `Keba - Pitash za men (320kbps).mp3`

## Notes

- Bitrate is added only if available.
- Removes duplicate spaces in filenames.
- Safe renaming with overwrite enabled.
- Works for Cyrillic and Mojibake filenames.
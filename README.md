# Drugs on Duty

A Rimworld mod that lets colonists take a drug allowed by their existing drug policy schedule during Work timetable hours, instead of waiting until the work block ends.

## Requirements

- [Harmony](https://steamcommunity.com/sharedfiles/filedetails/?id=2009463077) (declared as a mod dependency; Rimworld will prompt to install it if missing)

## Building from source

The mod source lives under `1.6/source/` as a .NET SDK project targeting `net472`:

```
cd "1.6/source"
dotnet build -c Release
```

The compiled DLL is written to `1.6/Assemblies/Drugs on Duty.dll`. A working Rimworld install isn't required to build - `Krafs.Rimworld.Ref` and `Lib.Harmony.Ref` provide stub reference assemblies via NuGet.

## License

GPL-3.0-or-later - see [LICENSE](LICENSE) and [COPYRIGHT](COPYRIGHT).

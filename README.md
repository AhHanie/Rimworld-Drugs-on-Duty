# Drugs on Duty

A Rimworld mod that lets colonists take a drug allowed by their existing drug policy schedule during Work timetable hours, instead of waiting until the work block ends.

## Requirements

- [Harmony](https://steamcommunity.com/sharedfiles/filedetails/?id=2009463077) (declared as a mod dependency; Rimworld will prompt to install it if missing)

## Building from source

Each supported RimWorld version has its own project under `<version>/source/`, and the compiled DLL goes to `<version>/Assemblies/Drugs on Duty.dll`. The `1.6` tree is kept as-is; fixes that apply to both versions need to be made in both.

**RimWorld 1.6** - .NET SDK project targeting `net472`. A RimWorld install isn't required: `Krafs.Rimworld.Ref` and `Lib.Harmony.Ref` provide stub reference assemblies via NuGet.

```
dotnet build "1.6/source/Drugs on Duty.csproj" -c Release
```

**RimWorld 1.7** - .NET SDK project targeting `net10.0` (.NET SDK 10 required). It compiles against the installed game and a Harmony 3 `0Harmony.dll` rather than NuGet packages. The defaults match the author's machine; override them for yours:

```
dotnet build "1.7/source/Drugs on Duty.csproj" -c Release -p:RimWorldDir="C:\path\to\RimWorld" -p:HarmonyDll="C:\path\to\Harmony\1.7\0Harmony.dll"
```

`RimWorldDir` is the game's root folder (it holds `Assembly-CSharp.dll` and `UnityStubs.dll`). The build stops with a clear error if either path is wrong.

## License

GPL-3.0-or-later - see [LICENSE](LICENSE) and [COPYRIGHT](COPYRIGHT).

# GitHub Copilot Instructions for ReadyThoseGuns Mod

## Mod Overview and Purpose
The **Ready Those Guns** mod aims to improve the functionality of mannable mortar-like turrets in the game RimWorld. It introduces a job for reloading and readying mortars as part of the hauling system. This mod ensures that mortars are prepared for use when needed, even allowing pawns incapable of violence to assist in the process.

## Key Features and Systems
### Job System
- **JobDefOf**: Defines new jobs for reloading and readying mortars.
- **JobDriver_Ready**: Handles the logic for executing the reloading and readying jobs.
- **WorkGiver_Ready**: Manages the assignment of these jobs to pawns.

### XML Integration
- **Jobs_Ready.xml**: Defines new job definitions in XML.
- **WorkGivers_Ready.xml**: Defines new work giver definitions in XML.

### Harmony Patching
The mod uses Harmony patching to integrate seamlessly with the game's existing mechanics. It ensures compatibility with various mortar mods and respects vanilla gizmo settings.

## Coding Patterns and Conventions
- **Main.cs**: Contains the main entry point of the mod, handling initialization and logging.
- **JobDefOf.cs**: Defines custom job definitions.
- **JobDriver_Ready.cs**: Implements the logic for the custom jobs.
- **WorkGiver_Ready.cs**: Manages the assignment of the custom jobs to pawns.

## Suggestions for Copilot
### General Guidelines
- **Use Descriptive Naming**: Ensure that class, method, and variable names are clear and descriptive.
- **Follow RimWorld Coding Standards**: Adhere to the coding conventions used in the RimWorld source code.
- **Documentation**: Add XML documentation to new classes and methods for better understanding and maintainability.
- **Testing**: Write unit tests for new features to ensure stability and reliability.

### Specific Recommendations
- **XML Defs**: When creating new XML definitions, ensure they are well-structured and compatible with the game's data loader.
- **Harmony Patches**: Use Harmony patches judiciously and document the purpose and impact of each patch.
- **Mod Compatibility**: When adding compatibility for new mortar mods, test thoroughly to ensure seamless integration.

### Example Usage
#### Creating a New Job Definition
xml
<!-- E:\SteamLibrary\steamapps\common\RimWorld\Mods\ReadyThoseGuns\1.7\Defs\Jobs_Ready.xml -->
<Defs>
    <JobDef>
        <defName>ReloadMortar</defName>
        <label>Reload Mortar</label>
        <description>Reload the mortar to prepare it for firing.</description>
        <workGiverDefs>
            <li>WorkGiver_Ready</li>
        </workGiverDefs>
        <driverClass>JobDriver_Ready</driverClass>
    </JobDef>
</Defs>


#### Implementing a Harmony Patch
csharp
// E:\SteamLibrary\steamapps\common\RimWorld\Mods\ReadyThoseGuns\Source\ReadyThoseGuns\Main.cs
using HarmonyLib;
using Verse;

namespace ReadyThoseGuns
{
    public class Main : Mod
    {
        public Main(ModContentPack content) : base(content)
        {
            new Harmony("Mlie.ReadyThoseGuns").PatchAll();
        }

        public override void PostStartup()
        {
            base.PostStartup();
            Log.Message("ReadyThoseGuns is ready to go!");
        }
    }
}


## Conclusion
By following these guidelines and recommendations, you can contribute effectively to the **Ready Those Guns** mod, ensuring that your code is maintainable, efficient, and compatible with the game's existing systems. If you have any questions or need further assistance, feel free to ask!

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.


## Hard rules (must follow)
- Do NOT run commands that modify the repo (no git commit, git apply, dotnet format) unless explicitly asked.
- Prefer minimal reads: read only the smallest code region needed (around the suspicious lines).
- When mentioning SonarQube issues, automatically use the SonarQube MCP service to fetch and address issues instead of making inferred fixes without querying SonarQube first.
- When mentioning the rimworld log, automatically use the Rimworld MCP service to fetch the log.


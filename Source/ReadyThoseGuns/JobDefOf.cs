using RimWorld;
using Verse;

namespace ReadyThoseGuns;

[DefOf]
public static class JobDefOf
{
    public static JobDef RTG_Ready;

    static JobDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(JobDefOf));
    }
}

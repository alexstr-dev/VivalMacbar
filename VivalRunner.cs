using System.Diagnostics;
using Eto;
using Eto.Forms;
using Vival.bars;

namespace Vival;

public static class VivalRunner
{
    /// <summary>
    /// Main function to be called upon startup.
    /// </summary>
    [STAThread]
    public static void Run()
    {
        // ? If Vival was already opened, close it. Acting as a toggle behavior.
        if (Process.GetProcessesByName("Vival").Length is not 1)
        {
            KillVival();
            return;
        }

        new Application(Platforms.Gtk).Run(new VivalBar());
    }

    /// <summary>
    /// Kills all Vival instances.
    /// </summary>
    private static void KillVival()
    {
        var proc = Process.GetProcessesByName("Vival");
        for (var i = 0; i < proc.Length; i++) proc[i].Kill();
        Environment.Exit(0);
    }
}
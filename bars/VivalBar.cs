using Eto.Drawing;
using Eto.Forms;

namespace Vival.bars;

public sealed class VivalBar : Form
{
    #region Variables
    /// <summary>
    /// The date label.
    /// </summary>
    // ? Labels that should update after startup should all be assigned their own variable.
    // ? This is because you can change the value of it directly, only redrawing the single component rather than the entire cell-set which is much more expensive.
    private readonly Label date = new()
    {
        Text = $"{DateTime.Now:D} ({DateTime.Now:HH:m:s})", TextColor = Colors.White,
        TextAlignment =
            TextAlignment.Right // ! <= Don't try and rely on this alignment logic, it hardly even works as it should.
    };

    /// <summary>
    /// The workspaces label (current / max).
    /// </summary>
    private readonly Label workspaces = new()
    {
        Text = $"1 / {LinuxUtils.workspacesCount}",
        TextColor = Colors.Gray
    };

    /// <summary>
    /// The currently active window.
    /// </summary>
    private readonly Label activeWindow = new()
    {
        Text = "Vival",
        TextColor = Colors.White
    };

    /// <summary>
    /// Padding for <see cref="DrawContent"/>.
    /// </summary>
    private readonly Padding contentPadding = new(10, 1, 0, 0);

    /// <summary>
    /// Padding for <see cref="DrawDate"/>.
    /// </summary>
    private readonly Padding datePadding = new(0, 0, 10, 0);

    /// <summary>
    /// Max and Min size.
    /// </summary>
    // * Replace 1920 with your screens width and make proper adjustments if needed.
    // * 1920 can also (in theory, not tested) be extended to your total resolution (all monitors combined) to stretch across them.
    private readonly Size defaultSize = new(1920, 22);

    /// <summary>
    /// Spacing for <see cref="DrawContent"/>.
    /// </summary>
    private readonly Size contentSpacing = new(5, 5);

    /// <summary>
    /// This is the position that's used for the bar.
    /// <para>0, 0 by default which makes it spawn on your first monitor.</para>
    /// </summary>
    private readonly Point position = new(0, 0);

    /// <summary>
    /// Symbol used in <see cref="Separator"/>.
    /// </summary>
    private const string separator = "|";
    #endregion

    /// <summary>
    /// Begin drawing the bar.
    /// </summary>
    public VivalBar()
    {
        Title = "Vival";
        Application.Instance.InvokeAsync(async () => await DrawContent());
        // * Not like xmonad gives the slightest shit about MinimumSize but yeah.
        Size = MinimumSize = defaultSize;
        AutoSize = true;
        Resizable = Topmost = false;
        BackgroundColor = Color.FromArgb(10, 10, 10);
        HackSyncSize();
        UpdateBar();
        SyncWorkspace();
    }

    /// <summary>
    /// Creates a new gray "|" separator.
    /// </summary>
    /// <returns></returns>
    private TableCell Separator() => new(new Label {Text = separator, TextColor = Colors.Gray});

    /// <summary>
    /// Draws all of the content.
    /// </summary>
    private async Task DrawContent() =>
        // * Begin drawing the cells.
        Content = new TableLayout
        {
            Spacing = contentSpacing,
            Padding = contentPadding,
            Rows =
            {
                new TableRow(DrawText(" ", Colors.White, null, 0, 3),
                    activeWindow,
                    DrawText("", Colors.White, null, 5, 3),
                    DrawDate())
            }
        };

    /// <summary>
    /// Draws text.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="textColor"></param>
    /// <param name="callback"></param>
    /// <param name="leftOffset"></param>
    /// <param name="yOffset"></param>
    /// <returns></returns>
    private TableCell DrawText(string text, Color textColor, EventHandler<MouseEventArgs>? callback = null,
        int leftOffset = 0, int yOffset = 0)
    {
        var label = new Label {Text = text, TextColor = textColor};
        if (callback is not null) label.MouseDown += callback;
        var layout = new TableLayout {Rows = {label}};
        var cell = new TableCell(layout);
        if (leftOffset + yOffset is 0) return cell;
        layout.Padding = new Padding(leftOffset, yOffset);
        return cell;
    }

    /// <summary>
    /// Draws the current date and time.
    /// </summary>
    /// <returns></returns>
    private TableCell DrawDate() =>
        new(new TableLayout
        {
            Padding = datePadding,
            Rows = {date}
        });

    /// <summary>
    /// Shitty way of trying to keep the size consistent.
    /// <para>Should be reworked in the future if possible.</para>
    /// </summary>
    private async void HackSyncSize()
    {
        const int wait = 50;
        while (true)
        {
            Size = defaultSize;
            Location = position;
            await Task.Delay(wait);
        }
        // ReSharper disable once FunctionNeverReturns
    }

    /// <summary>
    /// Syncs the bar to the currently active workspace.
    /// <para>Thanks nukistan for helping with this.</para>
    /// </summary>
    private async void SyncWorkspace()
    {
        const int wait = 100;
        const string @class = "Vival";
        while (true)
        {
            try
            {
                // ! Fix for Issue #1: xmonad border flashing
                if (await LinuxUtils.GetClassWorkspace(@class) != await LinuxUtils.GetActiveWorkspace(false))
                    await LinuxUtils.ExecuteCommand(
                        "xdotool search --class Vival set_desktop_for_window %@ $(xdotool get_desktop)");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                // ignored from here.
            }

            await Task.Delay(wait);
        }
    }

    /// <summary>
    /// Updates the bars content.
    /// </summary>
    private async void UpdateBar()
    {
        const int wait = 360;
        while (true)
        {
            await Application.Instance.InvokeAsync(async () =>
            {
                date.Text = DateTime.Now.ToString("            ddd MMM dd  HH:mm");
                workspaces.Text = $"{await LinuxUtils.GetActiveWorkspace()} / {LinuxUtils.workspacesCount}";
                activeWindow.Text = await LinuxUtils.GetActiveWindowTitle();
            });
            await Task.Delay(wait);
            // ! GC.Collect() is needed mainly because of the allocation issues from redrawing components.
            GC.Collect();
        }
    }
}
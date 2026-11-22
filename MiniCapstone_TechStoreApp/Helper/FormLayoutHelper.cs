
using System;
using System.Drawing;
using System.Windows.Forms;

public static class FormLayoutHelper
{
    public static void CenterPanel(Form form, Control panel, int maxWidth, int maxHeight)
    {
        // Determine available form dimensions
        int availableWidth = form.ClientSize.Width;
        int availableHeight = form.ClientSize.Height;

        // Keep panel within the available screen area
        int panelWidth = Math.Min(maxWidth, availableWidth);
        int panelHeight = Math.Min(maxHeight, availableHeight);

        // Apply panel dimensions
        panel.Size = new Size(panelWidth, panelHeight);

        // Center the panel
        panel.Left = (availableWidth - panel.Width) / 2;
        panel.Top = (availableHeight - panel.Height) / 2;
    }
}

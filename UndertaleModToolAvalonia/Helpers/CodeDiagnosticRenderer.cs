using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;

namespace UndertaleModToolAvalonia;

/// <summary>
/// Draws squiggly underlines for <see cref="CodeDiagnostic"/>s, with a color based on severity.
/// Registered as a <see cref="IBackgroundRenderer"/> on a <see cref="TextView"/>.
/// </summary>
public sealed class CodeDiagnosticRenderer : IBackgroundRenderer
{
    static readonly IPen ErrorPen = new Pen(new SolidColorBrush(Color.FromRgb(0xE0, 0x40, 0x40)), 1);
    static readonly IPen WarningPen = new Pen(new SolidColorBrush(Color.FromRgb(0xE0, 0xA0, 0x20)), 1);

    readonly record struct Marker(TextSegment Segment, CodeDiagnosticSeverity Severity, string Message);

    readonly List<Marker> markers = [];

    /// <inheritdoc/>
    public KnownLayer Layer => KnownLayer.Selection;

    public bool HasMarkers => markers.Count > 0;

    /// <summary>
    /// Replaces all currently displayed diagnostics with the given ones.
    /// </summary>
    public void SetDiagnostics(TextDocument document, IReadOnlyList<CodeDiagnostic> diagnostics)
    {
        markers.Clear();

        foreach (CodeDiagnostic diagnostic in diagnostics)
        {
            if (!TryGetOffset(document, diagnostic.Line, diagnostic.Column, out int offset))
                continue;

            int maxLength = document.TextLength - offset;
            if (maxLength <= 0)
                continue;

            int length = Math.Clamp(diagnostic.Width, 1, maxLength);
            TextSegment segment = new() { StartOffset = offset, Length = length };
            markers.Add(new Marker(segment, diagnostic.Severity, diagnostic.Message ?? ""));
        }
    }

    /// <summary>
    /// Removes all currently displayed diagnostics.
    /// </summary>
    public void Clear()
    {
        markers.Clear();
    }

    /// <summary>
    /// Gets the message of the diagnostic at the given document offset, if any.
    /// </summary>
    public string? GetMessageAtOffset(int offset)
    {
        foreach (Marker marker in markers)
        {
            if (offset >= marker.Segment.StartOffset && offset <= marker.Segment.EndOffset)
                return marker.Message;
        }
        return null;
    }

    /// <inheritdoc/>
    public void Draw(TextView textView, DrawingContext drawingContext)
    {
        if (!textView.VisualLinesValid || markers.Count == 0)
            return;

        foreach (Marker marker in markers)
        {
            IPen pen = marker.Severity == CodeDiagnosticSeverity.Warning ? WarningPen : ErrorPen;
            DrawSquigglyLine(drawingContext, pen, BackgroundGeometryBuilder.GetRectsForSegment(textView, marker.Segment));
        }
    }

    static void DrawSquigglyLine(DrawingContext drawingContext, IPen pen, IEnumerable<Rect> rects)
    {
        var geometry = new StreamGeometry();

        using (StreamGeometryContext context = geometry.Open())
        {
            foreach (Rect rect in rects)
            {
                if (rect.Width <= 0)
                    continue;

                const double Step = 4.0;
                const double Amplitude = 1.5;

                double y = rect.Bottom - 1.0;
                bool up = true;

                context.BeginFigure(new Point(rect.Left, y), false);

                for (double x = rect.Left; x < rect.Right; x += Step)
                {
                    double end = Math.Min(x + Step, rect.Right);
                    double midX = (x + end) / 2;
                    context.QuadraticBezierTo(new Point(midX, y + (up ? -Amplitude : Amplitude)), new Point(end, y), true);
                    up = !up;
                }
            }
        }

        drawingContext.DrawGeometry(null, pen, geometry);
    }

    static bool TryGetOffset(TextDocument document, int line, int column, out int offset)
    {
        offset = 0;

        if (line < 1 || line > document.LineCount)
            return false;

        DocumentLine documentLine = document.GetLineByNumber(line);

        column = Math.Clamp(column, 1, documentLine.Length + 1);
        offset = documentLine.Offset + column - 1;

        return true;
    }
}

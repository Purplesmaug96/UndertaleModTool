namespace UndertaleModToolAvalonia;

/// <summary>
/// Severity of a code diagnostic displayed in a code editor.
/// </summary>
public enum CodeDiagnosticSeverity
{
    Error,
    Warning,
}

/// <summary>
/// A single diagnostic to display as a squiggly underline in a code editor.
/// Positions are one-indexed, and <see cref="Width"/> is measured in characters.
/// </summary>
public readonly record struct CodeDiagnostic(
    CodeDiagnosticSeverity Severity,
    int Line,
    int Column,
    int Width,
    string Message);

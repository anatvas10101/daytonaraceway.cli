using System.Text;

namespace DaytonaRaceway.Cli.Output.Html;

public static partial class HtmlOutput
{
    private static StringBuilder BuildHtmlHead(string title)
        => new($$"""
                 <!DOCTYPE html>
                 <html>
                     <head><meta charset="utf-8">
                     <title>{{title}}</title>
                     <style>body{display:inline-flex;} table{margin: 0 10px;}</style>
                 </head>
                 """);
}

/// <summary>
/// Module for formatting and outputting lint analysis results
/// </summary>
module LintKit.CLI.Output

open System
open System.Text.Json
open FSharp.Analyzers.SDK
open LintKit.CLI.Runner

/// <summary>
/// Supported output formats for lint results
/// </summary>
type OutputFormat = 
    /// Plain text output format
    | Text
    /// SARIF (Static Analysis Results Interchange Format) for CI/CD integration
    | Sarif

/// <summary>
/// Parses a string into an OutputFormat
/// </summary>
/// <param name="format">Format string ("text" or "sarif")</param>
/// <returns>OutputFormat enum value</returns>
/// <exception cref="System.InvalidOperationException">Thrown when format is not supported</exception>
let parseOutputFormat (format: string) =
    match format.ToLowerInvariant() with
    | "text" -> Text
    | "sarif" -> Sarif
    | _ -> failwith $"Unknown output format: {format}. Supported formats: text, sarif"

/// <summary>
/// Formats analysis result as plain text
/// </summary>
/// <param name="result">The analysis result to format</param>
/// <param name="verbose">Whether to include verbose output</param>
/// <returns>Formatted text string</returns>
let formatTextOutput (result: AnalysisResult) (verbose: bool) =
    let output = System.Text.StringBuilder()
    
    // Report errors first
    for error in result.Errors do
        output.AppendLine($"Error: {error}") |> ignore
    
    if result.Messages.IsEmpty then
        if verbose then
            output.AppendLine("Analysis completed successfully - no violations found") |> ignore
    else
        if verbose then
            output.AppendLine($"Found {result.Messages.Length} violation(s):") |> ignore
        
        for message in result.Messages do
            let severityStr = 
                match message.Severity with
                | Severity.Warning -> "warning"
                | Severity.Error -> "error"
                | Severity.Info -> "info"
                | Severity.Hint -> "hint"
            
            output.AppendLine($"[{message.Code}] {severityStr}: {message.Message}") |> ignore
            
            if verbose then
                output.AppendLine($"  Type: {message.Type}") |> ignore
                if not (String.IsNullOrEmpty(message.Range.FileName)) then
                    output.AppendLine($"  File: {message.Range.FileName}") |> ignore
    
    output.ToString().TrimEnd()

/// <summary>
/// Formats analysis result as SARIF JSON
/// </summary>
/// <param name="result">The analysis result to format</param>
/// <returns>SARIF-formatted JSON string</returns>
let formatSarifOutput (result: AnalysisResult) =
    let rules = 
        result.Messages 
        |> List.groupBy (fun m -> m.Code)
        |> List.map (fun (code, messages) ->
            let firstMessage = List.head messages
            JsonSerializer.SerializeToElement({|
                id = code
                name = firstMessage.Type
                shortDescription = {| text = firstMessage.Message |}
                fullDescription = {| text = firstMessage.Message |}
                defaultConfiguration = {| level = "warning" |}
            |})
        )
    
    let results = 
        result.Messages
        |> List.map (fun message ->
            JsonSerializer.SerializeToElement({|
                ruleId = message.Code
                level = match message.Severity with
                        | Severity.Error -> "error"
                        | Severity.Warning -> "warning"
                        | Severity.Info -> "note"
                        | Severity.Hint -> "note"
                message = {| text = message.Message |}
                locations = [|
                    {|
                        physicalLocation = {|
                            artifactLocation = {| uri = message.Range.FileName |}
                            region = {|
                                startLine = 1
                                startColumn = 1
                                endLine = 1
                                endColumn = 1
                            |}
                        |}
                    |}
                |]
            |})
        )
    
    let sarif = {|
        version = "2.1.0"
        ``$schema`` = "https://raw.githubusercontent.com/oasis-tcs/sarif-spec/master/Schemata/sarif-schema-2.1.0.json"
        runs = [|
            {|
                tool = {|
                    driver = {|
                        name = "FSharp.LintKit"
                        version = "0.1.0"
                        informationUri = "https://github.com/yourusername/FSharp.LintKit"
                        rules = rules |> List.toArray
                    |}
                |}
                results = results |> List.toArray
            |}
        |]
    |}
    
    JsonSerializer.Serialize(sarif, JsonSerializerOptions(WriteIndented = true))

/// <summary>
/// Formats analysis result according to the specified format
/// </summary>
/// <param name="format">Output format to use</param>
/// <param name="result">The analysis result to format</param>
/// <param name="verbose">Whether to include verbose output (ignored for SARIF)</param>
/// <returns>Formatted output string</returns>
let formatOutput (format: OutputFormat) (result: AnalysisResult) (verbose: bool) =
    match format with
    | Text -> formatTextOutput result verbose
    | Sarif -> formatSarifOutput result
/// <summary>
/// Module for executing lint analysis on F# source files
/// </summary>
module LintKit.CLI.Runner

open System.IO
open FSharp.Analyzers.SDK
open LintKit.CLI.AnalyzerLoader

/// <summary>
/// Result of running lint analysis
/// </summary>
type AnalysisResult = {
    /// List of lint messages found during analysis
    Messages: Message list
    /// List of error messages encountered during analysis
    Errors: string list
}

/// <summary>
/// Runs a single analyzer on a file
/// </summary>
/// <param name="_analyzer">The analyzer to run (currently unused in mock implementation)</param>
/// <param name="filePath">Path to the F# file to analyze</param>
/// <returns>Async result containing messages on success or error message on failure</returns>
let runAnalyzer (_analyzer: Analyzer<CliContext>) (filePath: string) =
    async {
        try
            if not (File.Exists filePath) then
                return Error $"File not found: {filePath}"
            else
                // For now, create a simple mock context 
                // TODO: Implement proper F# parsing and context creation
                
                // Return empty messages for now - demonstrates the flow works
                return Ok []
        with
        | ex ->
            return Error $"Failed to run analyzer on {filePath}: {ex.Message}"
    }

/// <summary>
/// Runs all loaded analyzers on a single file
/// </summary>
/// <param name="loadedAnalyzers">List of loaded analyzer assemblies</param>
/// <param name="filePath">Path to the F# file to analyze</param>
/// <returns>Async AnalysisResult containing all messages and errors</returns>
let runAnalyzersOnFile (loadedAnalyzers: LoadedAnalyzer list) (filePath: string) =
    async {
        let results = ResizeArray<Message>()
        let errors = ResizeArray<string>()
        
        for loaded in loadedAnalyzers do
            for analyzer in loaded.Analyzers do
                let! result = runAnalyzer analyzer filePath
                match result with
                | Ok messages -> results.AddRange messages
                | Error error -> errors.Add error
        
        return {
            Messages = results |> Seq.toList
            Errors = errors |> Seq.toList
        }
    }

/// <summary>
/// Finds all F# source files in a target path
/// </summary>
/// <param name="targetPath">Path to file or directory to search</param>
/// <returns>List of F# file paths (.fs and .fsx files)</returns>
let findFSharpFiles (targetPath: string) =
    if File.Exists(targetPath) then
        if targetPath.EndsWith ".fs" || targetPath.EndsWith ".fsx" then
            [targetPath]
        else
            []
    elif Directory.Exists targetPath then
        Directory.GetFiles(targetPath, "*.fs", SearchOption.AllDirectories)
        |> Array.append (Directory.GetFiles(targetPath, "*.fsx", SearchOption.AllDirectories))
        |> Array.toList
    else
        []

/// <summary>
/// Runs lint analysis on a target path (file or directory)
/// </summary>
/// <param name="loadedAnalyzers">List of loaded analyzer assemblies</param>
/// <param name="targetPath">Path to file or directory to analyze</param>
/// <returns>Async AnalysisResult containing all messages and errors from analysis</returns>
let runAnalysisOnTarget (loadedAnalyzers: LoadedAnalyzer list) (targetPath: string) =
    async {
        let files = findFSharpFiles targetPath
        
        if files.IsEmpty then
            return {
                Messages = []
                Errors = [$"No F# files found in: {targetPath}"]
            }
        else
            let allResults = ResizeArray<Message>()
            let allErrors = ResizeArray<string>()
            
            for file in files do
                let! result = runAnalyzersOnFile loadedAnalyzers file
                allResults.AddRange result.Messages
                allErrors.AddRange result.Errors
            
            return {
                Messages = allResults |> Seq.toList
                Errors = allErrors |> Seq.toList
            }
    }
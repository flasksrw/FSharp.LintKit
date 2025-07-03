/// <summary>
/// Module for executing lint analysis on F# source files
/// </summary>
module LintKit.CLI.Runner

open System
open System.IO
open FSharp.Analyzers.SDK
open FSharp.Analyzers.SDK.Testing
open FSharp.Compiler.CodeAnalysis
open Ionide.ProjInfo
open Ionide.ProjInfo.FCS
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
/// Determines the target type and finds relevant project files
/// </summary>
/// <param name="targetPath">Path to solution, project, directory, or file</param>
/// <returns>List of project file paths to analyze</returns>
let findProjectFiles (targetPath: string) =
    let fullPath = Path.GetFullPath(targetPath)
    
    if File.Exists fullPath then
        let ext = Path.GetExtension(fullPath).ToLowerInvariant()
        match ext with
        | ".sln" | ".slnx" ->
            // Solution file: extract F# projects
            let solutionDir = Path.GetDirectoryName(fullPath)
            let lines = File.ReadAllLines(fullPath)
            lines
            |> Array.choose (fun line ->
                if line.Contains(".fsproj") then
                    let parts = line.Split([|'"'|], StringSplitOptions.RemoveEmptyEntries)
                    if parts.Length >= 2 then
                        let projPath = Path.Combine(solutionDir, parts.[1])
                        if File.Exists(projPath) then Some projPath else None
                    else None
                else None)
            |> Array.toList
        | ".fsproj" ->
            // Project file
            [fullPath]
        | ".fs" | ".fsx" ->
            // F# source file: find containing project
            let rec searchUpward (dir: string) =
                if Directory.Exists dir then
                    let projFiles = Directory.GetFiles(dir, "*.fsproj")
                    if projFiles.Length > 0 then
                        [projFiles.[0]]
                    else
                        let parent = Directory.GetParent(dir)
                        if parent <> null then
                            searchUpward parent.FullName
                        else
                            []
                else
                    []
            let fileDir = Path.GetDirectoryName(fullPath)
            searchUpward fileDir
        | _ -> []
    elif Directory.Exists fullPath then
        // Directory: find all F# projects recursively
        Directory.GetFiles(fullPath, "*.fsproj", SearchOption.AllDirectories)
        |> Array.toList
    else
        []

/// <summary>
/// Creates a CliContext using Ionide.ProjInfo for proper project analysis
/// </summary>
/// <param name="projectPath">Path to the .fsproj file</param>
/// <param name="sourceText">Source code content</param>
/// <returns>Async CliContext for analysis</returns>
let createCliContextFromProject (projectPath: string) (sourceText: string) =
    async {
        try
            // Initialize MSBuild tools
            let projectDir = DirectoryInfo(Path.GetDirectoryName(projectPath))
            let toolsPath = Init.init projectDir None
            
            // Create workspace loader
            let loader = WorkspaceLoader.Create(toolsPath, [])
            
            // Load the project
            let projectOptions = loader.LoadProjects([projectPath]) |> Seq.toArray
            
            if projectOptions.Length > 0 then
                // Convert to FSharpProjectOptions using Ionide.ProjInfo.FCS
                let fcsOptions = FCS.mapManyOptions projectOptions |> Seq.toArray
                return getContext fcsOptions.[0] sourceText
            else
                // Fallback to testing utilities
                let! testOptions = mkOptionsFromProject "net9.0" [] |> Async.AwaitTask
                return getContext testOptions sourceText
        with
        | ex ->
            printfn "Error loading project %s: %s" projectPath ex.Message
            // Fallback to testing utilities
            let! testOptions = mkOptionsFromProject "net9.0" [] |> Async.AwaitTask
            return getContext testOptions sourceText
    }

/// <summary>
/// Creates a CliContext for F# source file analysis with project discovery
/// </summary>
/// <param name="filePath">Path to the F# source file</param>
/// <param name="sourceText">Source code content</param>
/// <returns>Async CliContext for analysis</returns>
let createCliContext (filePath: string) (sourceText: string) =
    async {
        let projectFiles = findProjectFiles filePath
        match projectFiles with
        | projFile :: _ ->
            printfn "Using project file: %s" projFile
            return! createCliContextFromProject projFile sourceText
        | [] ->
            printfn "No project file found, using testing utilities"
            let! projectOptions = mkOptionsFromProject "net9.0" [] |> Async.AwaitTask
            return getContext projectOptions sourceText
    }

/// <summary>
/// Runs a single analyzer on a file
/// </summary>
/// <param name="analyzer">The analyzer to run</param>
/// <param name="filePath">Path to the F# file to analyze</param>
/// <returns>Async result containing messages on success or error message on failure</returns>
let runAnalyzer (analyzer: Analyzer<CliContext>) (filePath: string) =
    async {
        try
            if not (File.Exists filePath) then
                return Error $"File not found: {filePath}"
            else
                let sourceText = File.ReadAllText(filePath)
                let! context = createCliContext filePath sourceText
                let! messages = analyzer context
                return Ok messages
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
/// Finds all F# source files from projects discovered in target path
/// </summary>
/// <param name="targetPath">Path to solution, project, directory, or file</param>
/// <returns>List of F# file paths (.fs and .fsx files)</returns>
let findFSharpFilesFromTarget (targetPath: string) =
    let fullPath = Path.GetFullPath(targetPath)
    
    if File.Exists fullPath then
        let ext = Path.GetExtension(fullPath).ToLowerInvariant()
        match ext with
        | ".sln" | ".slnx" | ".fsproj" ->
            // For solution/project files, extract F# files from projects
            let projectFiles = findProjectFiles targetPath
            projectFiles
            |> List.collect (fun projFile ->
                try
                    let projDir = Path.GetDirectoryName(projFile)
                    let projContent = File.ReadAllText(projFile)
                    // Simple extraction of F# files from project
                    let lines = projContent.Split([|'\n'; '\r'|], StringSplitOptions.RemoveEmptyEntries)
                    lines
                    |> Array.choose (fun line ->
                        if line.Contains("Include=") && (line.Contains(".fs\"") || line.Contains(".fsx\"")) then
                            let parts = line.Split([|'"'|], StringSplitOptions.RemoveEmptyEntries)
                            if parts.Length >= 2 then
                                let fsFile = Path.Combine(projDir, parts.[1])
                                if File.Exists(fsFile) then Some fsFile else None
                            else None
                        else None)
                    |> Array.toList
                with
                | ex ->
                    printfn "Error reading project %s: %s" projFile ex.Message
                    [])
        | ".fs" | ".fsx" ->
            // Single F# file
            [fullPath]
        | _ -> []
    elif Directory.Exists fullPath then
        // Directory: find all F# files recursively
        Directory.GetFiles(fullPath, "*.fs", SearchOption.AllDirectories)
        |> Array.append (Directory.GetFiles(fullPath, "*.fsx", SearchOption.AllDirectories))
        |> Array.toList
    else
        []

/// <summary>
/// Runs lint analysis on a target path (solution, project, directory, or file)
/// </summary>
/// <param name="loadedAnalyzers">List of loaded analyzer assemblies</param>
/// <param name="targetPath">Path to solution, project, directory, or file to analyze</param>
/// <returns>Async AnalysisResult containing all messages and errors from analysis</returns>
let runAnalysisOnTarget (loadedAnalyzers: LoadedAnalyzer list) (targetPath: string) =
    async {
        let files = findFSharpFilesFromTarget targetPath
        
        if files.IsEmpty then
            return {
                Messages = []
                Errors = [$"No F# files found in: {targetPath}"]
            }
        else
            let allResults = ResizeArray<Message>()
            let allErrors = ResizeArray<string>()
            
            printfn "Found %d F# files to analyze" files.Length
            
            for file in files do
                let! result = runAnalyzersOnFile loadedAnalyzers file
                allResults.AddRange result.Messages
                allErrors.AddRange result.Errors
            
            return {
                Messages = allResults |> Seq.toList
                Errors = allErrors |> Seq.toList
            }
    }
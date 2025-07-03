module LintKit.CLI.Runner

open System.IO
open FSharp.Analyzers.SDK
open LintKit.CLI.AnalyzerLoader

type AnalysisResult = {
    Messages: Message list
    Errors: string list
}

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
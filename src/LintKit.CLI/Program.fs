open System
open Argu

type Arguments =
    | [<Mandatory; Unique>] Analyzers of path: string list
    | [<Mandatory; Unique>] Target of path: string
    | [<Unique>] Format of format: string
    | [<Unique>] Verbose
    | [<Unique>] Quiet

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Analyzers _ -> "Path to analyzer DLLs (multiple allowed)"
            | Target _ -> "Target folder or file to analyze"
            | Format _ -> "Output format: text (default), sarif"
            | Verbose -> "Enable verbose output"
            | Quiet -> "Enable quiet output (minimal)"

[<EntryPoint>]
let main argv =
    let parser = ArgumentParser.Create<Arguments>(programName = "fsharplintkit")
    
    try
        let results = parser.ParseCommandLine(inputs = argv, raiseOnUsage = true)
        
        let analyzers = results.GetResult Analyzers
        let target = results.GetResult Target
        let format = results.GetResult(Format, defaultValue = "text")
        let verbose = results.Contains Verbose
        let quiet = results.Contains Quiet
        
        if verbose && quiet then
            eprintfn "Error: Cannot specify both --verbose and --quiet"
            1
        else
            printfn "FSharp.LintKit v0.1.0"
            if verbose then
                printfn "Analyzers: %A" analyzers
                printfn "Target: %s" target
                printfn "Format: %s" format
            
            // Load analyzers from DLLs
            let (loadedAnalyzers, errors) = LintKit.CLI.AnalyzerLoader.loadAnalyzersFromPaths analyzers
            
            // Report any loading errors
            for error in errors do
                eprintfn "Error: %s" error
            
            if loadedAnalyzers.IsEmpty then
                eprintfn "No analyzers could be loaded"
                1
            else
                if verbose then
                    for loaded in loadedAnalyzers do
                        printfn "Loaded %d analyzer(s) from %s" loaded.Analyzers.Length loaded.Assembly.Location
                
                printfn "Running lint analysis with %d analyzer(s)..." (loadedAnalyzers |> List.sumBy (fun a -> a.Analyzers.Length))
                
                // Run analysis
                let result = LintKit.CLI.Runner.runAnalysisOnTarget loadedAnalyzers target |> Async.RunSynchronously
                
                // Report errors
                for error in result.Errors do
                    eprintfn "Error: %s" error
                
                // Report results
                if result.Messages.IsEmpty then
                    if not quiet then printfn "Analysis completed successfully - no violations found"
                    0
                else
                    if not quiet then printfn "Found %d violation(s):" result.Messages.Length
                    for message in result.Messages do
                        printfn "[%s] %s: %s" message.Code message.Type message.Message
                    1
            
    with
    | :? ArguParseException as ex ->
        printfn "%s" ex.Message
        1
    | ex ->
        eprintfn "Error: %s" ex.Message
        1

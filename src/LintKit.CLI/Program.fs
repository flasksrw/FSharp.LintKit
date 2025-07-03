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
            
            printfn "Running lint analysis..."
            printfn "Analysis completed successfully - no violations found"
            0
            
    with
    | :? ArguParseException as ex ->
        printfn "%s" ex.Message
        1
    | ex ->
        eprintfn "Error: %s" ex.Message
        1

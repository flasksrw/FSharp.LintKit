/// <summary>
/// Module for loading F# analyzers from external DLL files
/// </summary>
module LintKit.CLI.AnalyzerLoader

open System.IO
open System.Reflection
open FSharp.Analyzers.SDK

/// <summary>
/// Represents a loaded analyzer assembly containing one or more analyzers
/// </summary>
type LoadedAnalyzer = {
    /// The loaded assembly containing analyzers
    Assembly: Assembly
    /// List of analyzers found in the assembly
    Analyzers: Analyzer<CliContext> list
}

/// <summary>
/// Loads analyzers from a single DLL file
/// </summary>
/// <param name="dllPath">Path to the DLL file containing analyzers</param>
/// <returns>Result containing LoadedAnalyzer on success or error message on failure</returns>
let loadAnalyzerFromPath (dllPath: string) =
    try
        if not (File.Exists(dllPath)) then
            Error $"Analyzer DLL not found: {dllPath}"
        else
            // Load the assembly
            let assembly = Assembly.LoadFrom(dllPath)
            
            // Find all analyzer functions/properties with CliAnalyzer attribute
            let methods = 
                assembly.GetTypes()
                |> Array.collect (fun t -> t.GetMethods(BindingFlags.Static ||| BindingFlags.Public))
                |> Array.filter (fun m -> 
                    m.GetCustomAttributes(typeof<CliAnalyzerAttribute>, false).Length > 0)
            
            let properties = 
                assembly.GetTypes()
                |> Array.collect (fun t -> t.GetProperties(BindingFlags.Static ||| BindingFlags.Public))
                |> Array.filter (fun p -> 
                    p.GetCustomAttributes(typeof<CliAnalyzerAttribute>, false).Length > 0)
            
            
            let analyzers = 
                [
                    // Try properties first
                    yield! properties |> Array.choose (fun p ->
                        try
                            if p.PropertyType = typeof<Analyzer<CliContext>> then
                                let analyzer = p.GetValue(null) :?> Analyzer<CliContext>
                                Some analyzer
                            else
                                None
                        with
                        | ex -> 
                            eprintfn $"Failed to load analyzer from property {p.Name}: {ex.Message}"
                            None
                    )
                    
                    // Then try methods (functions)
                    yield! methods |> Array.choose (fun m ->
                        try
                            // Check if it's an analyzer function (CliContext -> Async<Message list>)
                            let paramTypes = m.GetParameters() |> Array.map (fun p -> p.ParameterType)
                            if paramTypes.Length = 1 && paramTypes.[0] = typeof<CliContext> then
                                // Create analyzer function by wrapping the method
                                let analyzer: Analyzer<CliContext> = fun (ctx: CliContext) ->
                                    m.Invoke(null, [|ctx|]) :?> Async<Message list>
                                Some analyzer
                            else
                                None
                        with
                        | ex -> 
                            eprintfn $"Failed to load analyzer from method {m.Name}: {ex.Message}"
                            None
                    )
                ]
            
            if analyzers.IsEmpty then
                Error $"No valid analyzers found in {dllPath}"
            else
                Ok { Assembly = assembly; Analyzers = analyzers }
    with
    | ex ->
        Error $"Failed to load analyzer from {dllPath}: {ex.Message}"

/// <summary>
/// Loads analyzers from multiple DLL files
/// </summary>
/// <param name="dllPaths">List of paths to DLL files</param>
/// <returns>Tuple containing list of successfully loaded analyzers and list of error messages</returns>
let loadAnalyzersFromPaths (dllPaths: string list) =
    let results = dllPaths |> List.map loadAnalyzerFromPath
    
    let errors = 
        results 
        |> List.choose (function Error e -> Some e | Ok _ -> None)
    
    let loaded = 
        results 
        |> List.choose (function Ok a -> Some a | Error _ -> None)
    
    (loaded, errors)
/// <summary>
/// Tests for the AnalyzerLoader module
/// </summary>
module AnalyzerLoaderTests

open System
open System.IO
open Xunit
open LintKit.CLI.AnalyzerLoader

[<Fact>]
let ``loadAnalyzerFromPath should return error for non-existent file`` () =
    let nonExistentPath = "nonexistent.dll"
    let result = loadAnalyzerFromPath nonExistentPath
    
    match result with
    | Error msg -> 
        Assert.Contains("not found", msg)
        Assert.Contains(nonExistentPath, msg)
    | Ok _ -> 
        Assert.True(false, "Expected error for non-existent file")

[<Fact>]
let ``loadAnalyzerFromPath should return error for invalid DLL`` () =
    // Create a temporary invalid DLL file
    let tempFile = Path.GetTempFileName()
    File.WriteAllText(tempFile, "This is not a valid DLL")
    
    try
        let result = loadAnalyzerFromPath tempFile
        
        match result with
        | Error msg -> 
            Assert.Contains("Failed to load analyzer", msg)
        | Ok _ -> 
            Assert.True(false, "Expected error for invalid DLL")
    finally
        File.Delete(tempFile)

[<Fact>]
let ``loadAnalyzerFromPath should load valid analyzer DLL`` () =
    // Use the built analyzer DLL
    let analyzerDllPath = Path.Combine(
        Directory.GetCurrentDirectory(), 
        "..", "..", "..", "..", "src", "LintKit.AnalyzerPatterns", "bin", "Debug", "net9.0", "LintKit.AnalyzerPatterns.dll"
    )
    
    if File.Exists(analyzerDllPath) then
        let result = loadAnalyzerFromPath analyzerDllPath
        
        match result with
        | Ok loadedAnalyzer -> 
            Assert.NotEmpty(loadedAnalyzer.Analyzers)
            Assert.NotNull(loadedAnalyzer.Assembly)
        | Error msg -> 
            Assert.True(false, $"Expected success but got error: {msg}")
    else
        // Skip test if DLL not found (not built yet)
        ()

[<Fact>]
let ``loadAnalyzersFromPaths should handle mixed valid and invalid paths`` () =
    let validPath = Path.Combine(
        Directory.GetCurrentDirectory(), 
        "..", "..", "..", "..", "src", "LintKit.AnalyzerPatterns", "bin", "Debug", "net9.0", "LintKit.AnalyzerPatterns.dll"
    )
    let invalidPath = "nonexistent.dll"
    
    let (loaded, errors) = loadAnalyzersFromPaths [validPath; invalidPath]
    
    // Should have at least one error (from invalid path)
    Assert.NotEmpty(errors)
    
    // If valid path exists, should have at least one loaded analyzer
    if File.Exists(validPath) then
        Assert.NotEmpty(loaded)
    else
        // If valid path doesn't exist, should have two errors
        Assert.Equal(2, errors.Length)

// === Boundary Value Tests ===
[<Fact>]
let ``loadAnalyzersFromPaths should handle empty path list`` () =
    let (loaded, errors) = loadAnalyzersFromPaths []
    
    Assert.Empty(loaded)
    Assert.Empty(errors)

[<Fact>]
let ``loadAnalyzerFromPath should handle very long path`` () =
    let longPath = String.replicate 300 "a" + ".dll"
    let result = loadAnalyzerFromPath longPath
    
    match result with
    | Error msg -> 
        Assert.Contains("not found", msg)
    | Ok _ -> 
        Assert.True(false, "Expected error for very long path")

[<Fact>]
let ``loadAnalyzerFromPath should handle path with special characters`` () =
    let specialPath = "special@#$%^&*().dll"
    let result = loadAnalyzerFromPath specialPath
    
    match result with
    | Error msg -> 
        Assert.Contains("not found", msg)
    | Ok _ -> 
        Assert.True(false, "Expected error for special character path")

// === Triangulation Tests ===
[<Fact>]
let ``loadAnalyzerFromPath should return multiple analyzers if DLL contains multiple`` () =
    let validPath = Path.Combine(
        Directory.GetCurrentDirectory(), 
        "..", "..", "..", "..", "src", "LintKit.AnalyzerPatterns", "bin", "Debug", "net9.0", "LintKit.AnalyzerPatterns.dll"
    )
    
    if File.Exists(validPath) then
        let result = loadAnalyzerFromPath validPath
        
        match result with
        | Ok loadedAnalyzer -> 
            // Our test DLL should contain exactly 10 analyzers
            Assert.Equal(10, loadedAnalyzer.Analyzers.Length)
        | Error msg -> 
            Assert.True(false, $"Expected success but got error: {msg}")
    else
        // Skip test if DLL not found
        ()

[<Fact>]
let ``loadAnalyzersFromPaths should accumulate all analyzers from multiple DLLs`` () =
    let validPath = Path.Combine(
        Directory.GetCurrentDirectory(), 
        "..", "..", "..", "..", "src", "LintKit.AnalyzerPatterns", "bin", "Debug", "net9.0", "LintKit.AnalyzerPatterns.dll"
    )
    
    if File.Exists(validPath) then
        // Load the same DLL twice to test accumulation
        let (loaded, errors) = loadAnalyzersFromPaths [validPath; validPath]
        
        Assert.Empty(errors)
        Assert.Equal(2, loaded.Length)
        
        // Should have 20 total analyzers (10 from each DLL)
        let totalAnalyzers = loaded |> List.sumBy (fun l -> l.Analyzers.Length)
        Assert.Equal(20, totalAnalyzers)
    else
        // Skip test if DLL not found
        ()
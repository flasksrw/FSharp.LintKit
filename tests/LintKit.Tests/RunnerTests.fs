/// <summary>
/// Tests for the Runner module
/// </summary>
module RunnerTests

open System
open System.IO
open Xunit
open LintKit.CLI.Runner

[<Fact>]
let ``findProjectFiles should find fsproj files in directory`` () =
    let currentDir = Directory.GetCurrentDirectory()
    let testDir = Path.Combine(currentDir, "..", "..", "..", "..", "src", "LintKit.CLI")
    
    if Directory.Exists(testDir) then
        let projectFiles = findProjectFiles testDir
        Assert.NotEmpty(projectFiles)
        Assert.True(projectFiles |> List.exists (fun p -> p.EndsWith(".fsproj")))
    else
        // Skip test if directory doesn't exist
        ()

[<Fact>]
let ``findProjectFiles should return empty list for non-existent directory`` () =
    let nonExistentDir = Path.Combine(Directory.GetCurrentDirectory(), "nonexistent")
    let projectFiles = findProjectFiles nonExistentDir
    Assert.Empty(projectFiles)

[<Fact>]
let ``findFSharpFilesFromTarget should find fs files in directory`` () =
    let currentDir = Directory.GetCurrentDirectory()
    let testDir = Path.Combine(currentDir, "..", "..", "..", "..", "src", "LintKit.CLI")
    
    if Directory.Exists(testDir) then
        let fsFiles = findFSharpFilesFromTarget testDir
        Assert.NotEmpty(fsFiles)
        Assert.True(fsFiles |> List.forall (fun f -> f.EndsWith(".fs")))
    else
        // Skip test if directory doesn't exist
        ()

[<Fact>]
let ``findFSharpFilesFromTarget should return empty list for non-existent directory`` () =
    let nonExistentDir = Path.Combine(Directory.GetCurrentDirectory(), "nonexistent")
    let fsFiles = findFSharpFilesFromTarget nonExistentDir
    Assert.Empty(fsFiles)

// === Boundary Value Tests ===
[<Fact>]
let ``findFSharpFilesFromTarget should handle empty directory`` () =
    // Create a temporary empty directory
    let tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())
    Directory.CreateDirectory(tempDir) |> ignore
    
    try
        let fsFiles = findFSharpFilesFromTarget tempDir
        Assert.Empty(fsFiles)
    finally
        Directory.Delete(tempDir)

[<Fact>]
let ``findFSharpFilesFromTarget should handle directory with only non-F# files`` () =
    // Create a temporary directory with non-F# files
    let tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())
    Directory.CreateDirectory(tempDir) |> ignore
    let txtFile = Path.Combine(tempDir, "test.txt")
    let csFile = Path.Combine(tempDir, "test.cs")
    
    try
        File.WriteAllText(txtFile, "test content")
        File.WriteAllText(csFile, "// C# file")
        
        let fsFiles = findFSharpFilesFromTarget tempDir
        Assert.Empty(fsFiles)
    finally
        Directory.Delete(tempDir, true)

[<Fact>]
let ``findFSharpFilesFromTarget should handle single F# file target`` () =
    // Create a temporary F# file
    let tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".fs")
    
    try
        File.WriteAllText(tempFile, "module Test")
        
        let fsFiles = findFSharpFilesFromTarget tempFile
        let singleFile = Assert.Single(fsFiles)
        Assert.Equal(tempFile, singleFile)
    finally
        File.Delete(tempFile)

[<Fact>]
let ``findProjectFiles should handle directory with mixed project types`` () =
    let currentDir = Directory.GetCurrentDirectory()
    let srcDir = Path.Combine(currentDir, "..", "..", "..", "..", "src")
    
    if Directory.Exists(srcDir) then
        let projectFiles = findProjectFiles srcDir
        // Should find F# projects but not other types
        Assert.True(projectFiles |> List.forall (fun p -> p.EndsWith(".fsproj")))
    else
        // Skip test if directory doesn't exist
        ()

// === Triangulation Tests ===
[<Fact>]
let ``findProjectFiles should handle solution file`` () =
    let currentDir = Directory.GetCurrentDirectory()
    let solutionFile = Path.Combine(currentDir, "..", "..", "..", "..", "FSharp.LintKit.slnx")
    
    if File.Exists(solutionFile) then
        let projectFiles = findProjectFiles solutionFile
        // Should find F# projects from the solution file
        Assert.True(projectFiles.Length >= 2) // At least CLI and Analyzers projects
        Assert.True(projectFiles |> List.forall (fun p -> p.EndsWith(".fsproj")))
    else
        // Skip test if solution file doesn't exist
        ()

[<Fact>]
let ``findProjectFiles should handle single project file`` () =
    let currentDir = Directory.GetCurrentDirectory()
    let projectFile = Path.Combine(currentDir, "..", "..", "..", "..", "src", "LintKit.CLI", "LintKit.CLI.fsproj")
    
    if File.Exists(projectFile) then
        let projectFiles = findProjectFiles projectFile
        // Should return the single project file
        let singleProject = Assert.Single(projectFiles)
        Assert.Equal(projectFile, singleProject)
    else
        // Skip test if project file doesn't exist
        ()
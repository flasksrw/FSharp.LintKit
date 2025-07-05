/// <summary>
/// **AI Test Pattern Reference**: Shows how to test code that references external packages
/// 
/// **Critical Pattern for AI Agents**: 
/// - Use package records: { Name = "xunit"; Version = "2.9.2" }
/// - Required when test source code contains external package references
/// - Example: [<Fact>], Assert.True, custom library functions
/// 
/// **For AI Agents**: Copy this pattern when test code uses external packages
/// **For Humans**: Reference for testing with package dependencies
/// </summary>
namespace LintKit.AnalyzerPatterns.Tests

open FSharp.Analyzers.SDK.Testing
open LintKit.AnalyzerPatterns.PackageReferenceExample
open Xunit

module PackageReferenceExampleTests =
    
    /// <summary>
    /// AI Pattern: Testing code with external package references
    /// Key: Use package records in mkOptionsFromProject when source contains external dependencies
    /// </summary>
    [<Fact>]
    let ``Should detect Assert.True usage in code with xUnit package reference`` () =
        async {
            // AI Pattern: Include package records for external dependencies
            let! projectOptions =
                mkOptionsFromProject
                    "net9.0"
                    [
                        { Name = "xunit"; Version = "2.9.2" }
                    ]
                |> Async.AwaitTask
            
            let source = """
module TestModule
open Xunit

[<Fact>]
let ``Example test`` () =
    Assert.True(true)
"""
            
            let ctx = getContext projectOptions source
            let! msgs = packageReferenceAnalyzer ctx
            
            Assert.NotEmpty(msgs)
            let message = msgs |> List.head
            Assert.Equal("PKGREF001", message.Code)
            Assert.Contains("Assert.True", message.Message)
        }

    /// <summary>
    /// AI Pattern: Testing with multiple package references
    /// </summary>
    [<Fact>]
    let ``Should work with multiple package references`` () =
        async {
            let! projectOptions =
                mkOptionsFromProject
                    "net9.0"
                    [
                        { Name = "xunit"; Version = "2.9.2" }
                        { Name = "FSharp.Core"; Version = "8.0.0" }
                    ]
                |> Async.AwaitTask
            
            let source = """
module TestModule
open Xunit

let testFunction () =
    Assert.True(1 = 1)
"""
            
            let ctx = getContext projectOptions source
            let! msgs = packageReferenceAnalyzer ctx
            
            Assert.NotEmpty(msgs)
        }

    /// <summary>
    /// AI Pattern: Testing without external packages (empty list)
    /// </summary>
    [<Fact>]
    let ``Should work with no external packages`` () =
        async {
            let! projectOptions =
                mkOptionsFromProject
                    "net9.0"
                    []  // No external packages needed
                |> Async.AwaitTask
            
            let source = """
module TestModule

let simpleFunction () =
    printfn "Hello World"
"""
            
            let ctx = getContext projectOptions source
            let! msgs = packageReferenceAnalyzer ctx
            
            // Should not crash, may return empty list
            Assert.NotNull(msgs)
        }
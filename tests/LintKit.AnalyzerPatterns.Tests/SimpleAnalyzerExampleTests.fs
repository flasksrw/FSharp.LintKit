/// <summary>
/// **AI Test Pattern Reference**: Working test implementation for SimpleAnalyzerExample using correct FSharp.Analyzers.SDK.Testing
/// 
/// **Critical Patterns for AI Agents**:
/// - Use `mkOptionsFromProject "net9.0" [] |> Async.AwaitTask` for project setup
/// - Wrap tests in `async { ... }` blocks
/// - Use `getContext projectOptions source` to create analysis context
/// - Test both positive cases (rule triggers) and negative cases (rule doesn't trigger)
/// 
/// **Based on working examples**: Fabulous.Analyzers project patterns
/// **For AI Agents**: Copy these exact patterns when generating analyzer tests
/// **For Humans**: Use this as a reference for proper test structure
/// </summary>
namespace LintKit.AnalyzerPatterns.Tests

open FSharp.Analyzers.SDK.Testing
open LintKit.AnalyzerPatterns.SimpleAnalyzerExample
open Xunit

module SimpleAnalyzerExampleTests =
    
    /// <summary>
    /// AI Pattern: Positive test case - verifies analyzer detects target pattern
    /// Key elements: async block, mkOptionsFromProject |> Async.AwaitTask, getContext, test assertions
    /// </summary>
    [<Fact>]
    let ``Should detect TODO in string literal`` () =
        async {
            // AI Pattern: Critical - use |> Async.AwaitTask to convert Task to Async
            let! projectOptions =
                mkOptionsFromProject
                    "net9.0"
                    []  // Empty packages list for basic F# testing
                |> Async.AwaitTask
            
            let source = """
module TestModule

let message = "TODO: Implement this feature"
"""
            
            // AI Pattern: Create analysis context from source code
            let ctx = getContext projectOptions source
            // AI Pattern: Run analyzer and await results
            let! msgs = simpleAnalyzer ctx
            
            // AI Pattern: Verify results - check message count, codes, and content
            Assert.NotEmpty(msgs)
            let todoMessage = msgs |> List.head
            Assert.Equal("SIMPLE001", todoMessage.Code)
            Assert.Contains("TODO", todoMessage.Message)
        }

    /// <summary>
    /// AI Pattern: Negative test case - verifies analyzer doesn't trigger on valid code
    /// </summary>
    [<Fact>]
    let ``Should not trigger on normal string literal`` () =
        async {
            let! projectOptions =
                mkOptionsFromProject
                    "net9.0"
                    []
                |> Async.AwaitTask
            
            let source = """
module TestModule

let message = "This is a normal message"
"""
            
            let ctx = getContext projectOptions source
            let! msgs = simpleAnalyzer ctx
            
            Assert.Empty(msgs)
        }

    [<Fact>]
    let ``Should handle empty modules`` () =
        async {
            let! projectOptions =
                mkOptionsFromProject
                    "net9.0"
                    []
                |> Async.AwaitTask
            
            let source = """
module TestModule
// Empty module
"""
            
            let ctx = getContext projectOptions source
            let! msgs = simpleAnalyzer ctx
            
            // Should not crash and return empty list
            Assert.NotNull(msgs)
        }

    [<Fact>]
    let ``Should detect multiple TODOs in same file`` () =
        async {
            let! projectOptions =
                mkOptionsFromProject
                    "net9.0"
                    []
                |> Async.AwaitTask
            
            let source = """
module TestModule

let message1 = "TODO: First task"
let message2 = "TODO: Second task"  
let normalMessage = "This is fine"
"""
            
            let ctx = getContext projectOptions source
            let! msgs = simpleAnalyzer ctx
            
            Assert.True(msgs.Length >= 2, "Should detect at least 2 TODO messages")
            Assert.True(msgs |> List.forall (fun m -> m.Code = "SIMPLE001"))
        }

    /// <summary>
    /// AI Pattern: Testing SimpleAnalyzer with external package references (like xUnit)
    /// Key: Use package records in mkOptionsFromProject when source contains external dependencies
    /// </summary>
    [<Fact>]
    let ``Should detect TODO in code with xUnit package reference`` () =
        async {
            // AI Pattern: Critical - include external packages for test code that uses them
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
    let message = "TODO: Write proper test"
    Assert.True(true)
"""
            
            let ctx = getContext projectOptions source
            let! msgs = simpleAnalyzer ctx
            
            // Should detect TODO even in test code with external packages
            Assert.NotEmpty(msgs)
            let todoMessage = msgs |> List.head
            Assert.Equal("SIMPLE001", todoMessage.Code)
            Assert.Contains("TODO", todoMessage.Message)
        }

    /// <summary>
    /// AI Pattern: Testing SimpleAnalyzer with xUnit package and function usage
    /// Shows how package references work with actual library function calls
    /// </summary>
    [<Fact>]
    let ``Should detect TODO in xUnit test with Assert function`` () =
        async {
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

let testFunction () =
    let message = "TODO: Load actual data"
    Assert.True(1 = 1)
"""
            
            let ctx = getContext projectOptions source
            let! msgs = simpleAnalyzer ctx
            
            // Should detect TODO even in xUnit test with Assert function
            Assert.NotEmpty(msgs)
            let todoMessage = msgs |> List.head
            Assert.Equal("SIMPLE001", todoMessage.Code)
        }

    /// <summary>
    /// AI Pattern: Testing SimpleAnalyzer with no external packages (baseline test)
    /// Demonstrates that package list can be empty when no external dependencies are used
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
    let message = "TODO: Implement this"
    message
"""
            
            let ctx = getContext projectOptions source
            let! msgs = simpleAnalyzer ctx
            
            // Should detect TODO even without any external packages
            Assert.NotEmpty(msgs)
            let todoMessage = msgs |> List.head
            Assert.Equal("SIMPLE001", todoMessage.Code)
            Assert.Contains("TODO", todoMessage.Message)
        }
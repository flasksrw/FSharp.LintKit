/// <summary>
/// Test Template for Custom Analyzer
/// 
/// **FOR AI AGENTS**: Follow these steps to create comprehensive tests:
/// 1. Study SimpleAnalyzerExampleTests.fs for the exact working patterns
/// 2. Use the critical pattern: mkOptionsFromProject |> Async.AwaitTask
/// 3. Create positive tests (rule triggers) and negative tests (rule doesn't trigger)
/// 4. Test edge cases: empty modules, multiple instances, error handling
/// 
/// **FOR HUMAN DEVELOPERS**:
/// - Let AI agent generate tests based on your rule specifications
/// - All tests follow the established working patterns
/// </summary>
namespace MyCustomAnalyzer.Tests

open FSharp.Analyzers.SDK.Testing
open MyCustomAnalyzer.CustomAnalyzer
open Xunit

module CustomAnalyzerTests =
    
    /// <summary>
    /// TODO: AI Agent - Implement positive test case
    /// **CRITICAL PATTERN**: Use mkOptionsFromProject |> Async.AwaitTask
    /// Reference: SimpleAnalyzerExampleTests.fs for exact structure
    /// </summary>
    [<Fact>]
    let ``Should detect target pattern`` () =
        async {
            // TODO: AI Agent - Replace with actual test implementation
            // CRITICAL: Use this exact pattern for project setup
            let! projectOptions =
                mkOptionsFromProject
                    "net8.0"  // Adjust target framework as needed
                    []        // Empty packages list for basic F# testing
                |> Async.AwaitTask
            
            let source = """
module TestModule

// TODO: AI Agent - Add test code that should trigger your rule
let placeholder = "Replace with actual test code"
"""
            
            let ctx = getContext projectOptions source
            let! msgs = customAnalyzer ctx
            
            // TODO: AI Agent - Replace with actual assertions
            // Example assertions:
            // Assert.NotEmpty(msgs)
            // let message = msgs |> List.head
            // Assert.Equal("YOUR_RULE_CODE", message.Code)
            // Assert.Contains("expected text", message.Message)
            
            // Placeholder assertion - AI should replace this
            Assert.True(true, "TODO: AI Agent - Implement actual test assertions")
        }
    
    /// <summary>
    /// TODO: AI Agent - Implement negative test case
    /// Verify analyzer doesn't trigger on valid code
    /// </summary>
    [<Fact>]
    let ``Should not trigger on valid code`` () =
        async {
            let! projectOptions =
                mkOptionsFromProject
                    "net8.0"
                    []
                |> Async.AwaitTask
            
            let source = """
module TestModule

// TODO: AI Agent - Add valid code that should NOT trigger your rule
let validCode = "Replace with code that should pass"
"""
            
            let ctx = getContext projectOptions source
            let! msgs = customAnalyzer ctx
            
            // TODO: AI Agent - For negative tests, typically expect empty results
            // Assert.Empty(msgs)
            
            // Placeholder assertion
            Assert.True(true, "TODO: AI Agent - Implement negative test assertions")
        }
    
    /// <summary>
    /// TODO: AI Agent - Implement edge case test
    /// Test empty modules and error conditions
    /// </summary>
    [<Fact>]
    let ``Should handle edge cases gracefully`` () =
        async {
            let! projectOptions =
                mkOptionsFromProject
                    "net8.0"
                    []
                |> Async.AwaitTask
            
            let source = """
module TestModule
// Empty module
"""
            
            let ctx = getContext projectOptions source
            let! msgs = customAnalyzer ctx
            
            // Should not crash and return a valid result
            Assert.NotNull(msgs)
        }
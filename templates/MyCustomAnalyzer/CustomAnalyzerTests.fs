/// <summary>
/// Test Template for Custom Analyzer - EDIT THIS FILE
/// 
/// **🤖 FOR AI AGENTS (THIS MEANS YOU)**: REPLACE the TODO implementations with comprehensive tests:
/// 1. Study SimpleAnalyzerExampleTests.fs for the exact working patterns
/// 2. Use the critical pattern: mkOptionsFromProject |> Async.AwaitTask
/// 3. EDIT the test methods to match your analyzer rules
/// 4. Create positive tests (rule triggers) and negative tests (rule doesn't trigger)
/// 5. Test edge cases: empty modules, multiple instances, error handling
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
    /// EDIT THIS TEST: Replace TODO with positive test case for your analyzer
    /// **🤖 CRITICAL PATTERN (FOR YOU)**: Use mkOptionsFromProject |> Async.AwaitTask
    /// Reference: SimpleAnalyzerExampleTests.fs for exact structure
    /// </summary>
    [<Fact>]
    let ``Should detect target pattern`` () =
        async {
            // 🤖 EDIT THIS (FOR YOU): Replace with actual test implementation
            // CRITICAL: Use this exact pattern for project setup
            let! projectOptions =
                mkOptionsFromProject
                    "net8.0"  // Adjust target framework as needed
                    []        // Empty packages list for basic F# testing
                |> Async.AwaitTask
            
            let source = """
module TestModule

// 🤖 EDIT THIS (FOR YOU): Add test code that should trigger your rule
let placeholder = "Replace with actual test code"
"""
            
            let ctx = getContext projectOptions source
            let! msgs = customAnalyzer ctx
            
            // 🤖 EDIT THIS (FOR YOU): Replace with actual assertions
            // Example assertions:
            // Assert.NotEmpty(msgs)
            // let message = msgs |> List.head
            // Assert.Equal("YOUR_RULE_CODE", message.Code)
            // Assert.Contains("expected text", message.Message)
            
            // 🤖 EDIT THIS (FOR YOU): Placeholder assertion - replace with actual test logic
            Assert.True(true, "🤖 EDIT THIS (FOR YOU): Implement actual test assertions")
        }
    
    /// <summary>
    /// EDIT THIS TEST: Replace TODO with negative test case for your analyzer
    /// **🤖 FOR YOU**: Verify analyzer doesn't trigger on valid code
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

// 🤖 EDIT THIS (FOR YOU): Add valid code that should NOT trigger your rule
let validCode = "Replace with code that should pass"
"""
            
            let ctx = getContext projectOptions source
            let! msgs = customAnalyzer ctx
            
            // 🤖 EDIT THIS (FOR YOU): For negative tests, typically expect empty results
            // Assert.Empty(msgs)
            
            // 🤖 EDIT THIS (FOR YOU): Placeholder assertion - replace with actual test logic
            Assert.True(true, "🤖 EDIT THIS (FOR YOU): Implement negative test assertions")
        }
    
    /// <summary>
    /// EDIT THIS TEST: Replace TODO with edge case test for your analyzer
    /// **🤖 FOR YOU**: Test empty modules and error conditions
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
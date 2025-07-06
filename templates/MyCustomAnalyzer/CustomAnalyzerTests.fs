/// <summary>
/// TEST TEMPLATE FILE - DO NOT EDIT DIRECTLY  
/// 
/// **🤖 FOR AI AGENTS (THIS MEANS YOU)**: COPY this file for each analyzer:
/// 1. COPY this file to create test files (e.g., NoHardcodedStringsAnalyzerTests.fs)
/// 2. UPDATE the open statement to import your specific analyzer module
/// 3. RENAME the test module to match your analyzer
/// 4. IMPLEMENT tests for your specific rule only
/// 5. ADD the new test file to the .fsproj file
/// 
/// **EXAMPLE TEST FILE NAMES TO CREATE**:
/// - NoHardcodedStringsAnalyzerTests.fs
/// - RequireTypeAnnotationsAnalyzerTests.fs
/// - NamingConventionAnalyzerTests.fs
/// </summary>
namespace MyCustomAnalyzer.Tests

open FSharp.Analyzers.SDK.Testing
// 🤖 UPDATE THIS IMPORT (FOR YOU): Change to your specific analyzer module
// Example: open MyCustomAnalyzer.NoHardcodedStringsAnalyzer
open MyCustomAnalyzer.CustomAnalyzer
open Xunit

// 🤖 RENAME THIS MODULE (FOR YOU): Match your analyzer name
// Example: NoHardcodedStringsAnalyzerTests, RequireTypeAnnotationsAnalyzerTests
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
                    []        // 🤖 EDIT IF NEEDED (FOR YOU): Add packages with Name/Version records
                              // Example: [{ Name = "xunit"; Version = "2.9.2" }] if test code uses [<Fact>]
                              // Example: [{ Name = "MyLibrary"; Version = "1.0.0" }] for custom libraries
                |> Async.AwaitTask
            
            let source = """
module TestModule

// 🤖 EDIT THIS (FOR YOU): Add test code that should trigger your rule
// If using xUnit attributes like [<Fact>], add "xunit" to packages list above
let placeholder = "Replace with actual test code"
"""
            
            let ctx = getContext projectOptions source
            // 🤖 UPDATE THIS (FOR YOU): Change to your analyzer function name
            let! msgs = contextAnalyzer ctx
            
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
            // 🤖 UPDATE THIS (FOR YOU): Change to your analyzer function name
            let! msgs = contextAnalyzer ctx
            
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
            // 🤖 UPDATE THIS (FOR YOU): Change to your analyzer function name
            let! msgs = contextAnalyzer ctx
            
            // Should not crash and return a valid result
            Assert.NotNull(msgs)
        }

    /// <summary>
    /// **🤖 CRITICAL PATTERN (FOR YOU)**: Testing with external package references
    /// **COPY THIS PATTERN**: When your test code uses external packages like xUnit, FSharp.Data, etc.
    /// Key: Use package records with Name and Version in mkOptionsFromProject
    /// </summary>
    [<Fact>]
    let ``Should work with external package references like xUnit`` () =
        async {
            // 🤖 CRITICAL PATTERN (FOR YOU): Include external packages when test source uses them
            let! projectOptions =
                mkOptionsFromProject
                    "net8.0"
                    [
                        { Name = "xunit"; Version = "2.9.2" }
                        // 🤖 ADD MORE PACKAGES AS NEEDED (FOR YOU):
                        // { Name = "FSharp.Data"; Version = "6.4.0" }
                        // { Name = "Newtonsoft.Json"; Version = "13.0.3" }
                    ]
                |> Async.AwaitTask
            
            let source = """
module TestModule
open Xunit

[<Fact>]
let ``Example test function`` () =
    // 🤖 EDIT THIS (FOR YOU): Replace with code that should trigger your rule
    let testCode = "Replace with actual test code"
    Assert.True(true)
"""
            
            let ctx = getContext projectOptions source
            // 🤖 UPDATE THIS (FOR YOU): Change to your analyzer function name
            let! msgs = contextAnalyzer ctx
            
            // 🤖 EDIT THIS (FOR YOU): Verify your analyzer works even with external packages
            // Should analyze code correctly regardless of external package usage
            Assert.NotNull(msgs)
            // Add specific assertions based on your rule:
            // Assert.NotEmpty(msgs) // if your rule should trigger
            // Assert.Empty(msgs)    // if your rule should not trigger
        }

    /// <summary>
    /// **🤖 ESSENTIAL PATTERN (FOR YOU)**: Testing without packages (baseline)
    /// **COPY THIS PATTERN**: Verify analyzer works correctly without external dependencies
    /// </summary>
    [<Fact>]
    let ``Should work without external packages`` () =
        async {
            let! projectOptions =
                mkOptionsFromProject
                    "net8.0"
                    []  // Empty packages list - no external dependencies
                |> Async.AwaitTask
            
            let source = """
module TestModule

let simpleFunction () =
    // 🤖 EDIT THIS (FOR YOU): Replace with code that should trigger your rule
    let result = "Replace with actual test code"
    result
"""
            
            let ctx = getContext projectOptions source
            // 🤖 UPDATE THIS (FOR YOU): Change to your analyzer function name
            let! msgs = contextAnalyzer ctx
            
            // 🤖 EDIT THIS (FOR YOU): Basic functionality test without external packages
            Assert.NotNull(msgs)
            // Add specific assertions based on your rule
        }
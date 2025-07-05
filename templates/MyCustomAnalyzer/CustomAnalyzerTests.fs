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
open MyCustomAnalyzer.TemplateAnalyzer
open Xunit

// 🤖 RENAME THIS MODULE (FOR YOU): Match your analyzer name
// Example: NoHardcodedStringsAnalyzerTests, RequireTypeAnnotationsAnalyzerTests
module TemplateAnalyzerTests =
    
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
            let! msgs = templateAnalyzer ctx
            
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
            let! msgs = templateAnalyzer ctx
            
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
            let! msgs = templateAnalyzer ctx
            
            // Should not crash and return a valid result
            Assert.NotNull(msgs)
        }
/// <summary>
/// **TEST TEMPLATE FILE**: Implement your test suite here
/// 
/// **EDITING INSTRUCTIONS**:
/// 1. Change module name to match your analyzer (e.g., NoHardcodedStringsAnalyzerTests)
/// 2. UPDATE the open statement to import your specific analyzer module
/// 3. Add this file to MyCustomAnalyzer.fsproj: <Compile Include="YourAnalyzerTests.fs" />
/// 4. IMPLEMENT tests for your specific rule only
/// </summary>
namespace MyCustomAnalyzer.Tests

open FSharp.Analyzers.SDK.Testing
// UPDATE THIS IMPORT: Change to your specific analyzer module
// Example: open MyCustomAnalyzer.NoHardcodedStringsAnalyzer
open MyCustomAnalyzer.TemplateAnalyzer
open Xunit

// RENAME THIS MODULE: Match your analyzer name
// Example: NoHardcodedStringsAnalyzerTests, RequireTypeAnnotationsAnalyzerTests
module TemplateAnalyzerTests =
    
    /// <summary>
    /// Test that your rule detects the target pattern
    /// 
    /// TEST PERSPECTIVES (create separate test methods for each):
    /// - Positive cases: Rule correctly detects violations
    /// - Negative cases: Rule ignores valid code  
    /// - Edge cases: Boundary conditions, complex nested expressions
    /// - Error handling: Malformed syntax, parse errors
    /// </summary>
    [<Fact>]
    let ``Test example`` () =
        async {
            // CRITICAL: Use this exact mkOptionsFromProject pattern
            let! projectOptions =
                mkOptionsFromProject
                    "net9.0"  // Adjust target framework as needed
                    []        // Add packages if test code uses external libraries
                              // Example: [{ Name = "xunit"; Version = "2.9.2" }] for [<Fact>] attributes
                              // Example: [{ Name = "MyLibrary"; Version = "1.0.0" }] for custom libraries
                |> Async.AwaitTask
            
            let fsharpCode = """
module TestModule

// REPLACE: Add F# code to be analyzed by your rule
let example = "your test code here"
"""
            
            let ctx = getContext projectOptions fsharpCode
            let! msgs = contextAnalyzer ctx
            
            // REPLACE: Add your assertions
            // Example: Assert.NotEmpty(msgs)
            // Example: Assert.Contains("expected message", msgs.[0].Message)
            Assert.True(true) // Replace with actual test
        }
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
                    []        // Add packages ONLY if fsharpCode uses external libraries
                              // 
                              // Only add packages for libraries used INSIDE fsharpCode string:
                              // 
                              // Example: [{ Name = "Xunit"; Version = "2.9.2" }] if fsharpCode contains "open Xunit"
                              // Example: [{ Name = "MyLibrary"; Version = "1.0.0" }] if fsharpCode contains "open MyLibrary"
                              // Example: [] (empty) if fsharpCode only uses basic F# syntax
                |> Async.AwaitTask
            
            let fsharpCode = """
module TestModule

// REPLACE: Add F# code to be analyzed by your rule
// IMPORTANT: This code must be SELF-CONTAINED (no missing functions, opens, etc.)
// - All variables and functions must be defined
// - All required modules must be opened
// - Code must compile without errors
// 
// Example: To test "f x" pattern, define both f and x:
// let f x = x + 1
// let x = 5
// let result = f x
// 
// If you need external libraries, add them to the packages list above
// Example: open System  (no package needed - built-in)
// Example: open Xunit; Assert.True(true)  (needs Xunit package)

let example = "your test code here"
"""
            
            let ctx = getContext projectOptions fsharpCode
            let! msgs = contextAnalyzer ctx
            
            // REPLACE: Add your assertions
            // Example: Assert.NotEmpty(msgs)
            // Example: Assert.Contains("expected message", msgs.[0].Message)
            Assert.True(true) // Replace with actual test
        }
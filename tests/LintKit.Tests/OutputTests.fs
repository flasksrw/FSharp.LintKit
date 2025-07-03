/// <summary>
/// Tests for the Output module using t-wada methodology
/// </summary>
module OutputTests

open System
open System.Linq
open System.Text.Json
open Xunit
open FSharp.Analyzers.SDK
open FSharp.Compiler.Text
open LintKit.CLI.Output
open LintKit.CLI.Runner

// Helper function to create test messages
let createMessage code severity messageText messageType =
    {
        Type = messageType
        Message = messageText
        Code = code
        Severity = severity
        Range = Range.Zero
        Fixes = []
    }

// Helper function to create test analysis result
let createAnalysisResult messages errors =
    {
        Messages = messages
        Errors = errors
    }

// === Basic Functionality Tests ===

[<Fact>]
let ``parseOutputFormat should parse text format`` () =
    let result = parseOutputFormat "text"
    Assert.Equal(Text, result)

[<Fact>]
let ``parseOutputFormat should parse sarif format`` () =
    let result = parseOutputFormat "sarif"
    Assert.Equal(Sarif, result)

[<Fact>]
let ``parseOutputFormat should be case insensitive`` () =
    let textResult = parseOutputFormat "TEXT"
    let sarifResult = parseOutputFormat "SARIF"
    let mixedResult = parseOutputFormat "Text"
    
    Assert.Equal(Text, textResult)
    Assert.Equal(Sarif, sarifResult)
    Assert.Equal(Text, mixedResult)

[<Fact>]
let ``parseOutputFormat should throw for invalid format`` () =
    Assert.Throws<System.Exception>(fun () -> parseOutputFormat "invalid" |> ignore)

// === Text Output Tests ===

[<Fact>]
let ``formatTextOutput should handle empty messages with verbose false`` () =
    let result = createAnalysisResult [] []
    let output = formatTextOutput result false
    
    Assert.Equal("", output)

[<Fact>]
let ``formatTextOutput should handle empty messages with verbose true`` () =
    let result = createAnalysisResult [] []
    let output = formatTextOutput result true
    
    let expectedOutput = "Analysis completed successfully - no violations found"
    Assert.Equal(expectedOutput, output)

[<Fact>]
let ``formatTextOutput should format single warning message`` () =
    let message = createMessage "W001" Severity.Warning "Test warning" "Test Analyzer"
    let result = createAnalysisResult [message] []
    let output = formatTextOutput result false
    
    let expectedOutput = "[W001] warning: Test warning"
    Assert.Equal(expectedOutput, output)

[<Fact>]
let ``formatTextOutput should format single error message`` () =
    let message = createMessage "E001" Severity.Error "Test error" "Test Analyzer"
    let result = createAnalysisResult [message] []
    let output = formatTextOutput result false
    
    let expectedOutput = "[E001] error: Test error"
    Assert.Equal(expectedOutput, output)

[<Fact>]
let ``formatTextOutput should format single hint message`` () =
    let message = createMessage "H001" Severity.Hint "Test hint" "Test Analyzer"
    let result = createAnalysisResult [message] []
    let output = formatTextOutput result false
    
    let expectedOutput = "[H001] hint: Test hint"
    Assert.Equal(expectedOutput, output)

// Note: Unknown severity testing is difficult with F# Analyzers SDK's Severity type
// The implementation handles unknown values with "unknown" string and "warning" SARIF level

[<Fact>]
let ``formatTextOutput should include verbose information when requested`` () =
    let message = createMessage "W001" Severity.Warning "Test warning" "Test Analyzer"
    let result = createAnalysisResult [message] []
    let output = formatTextOutput result true
    
    let expectedOutput = """Found 1 violation(s):
[W001] warning: Test warning
  Type: Test Analyzer
  File: unknown"""
    Assert.Equal(expectedOutput, output)

[<Fact>]
let ``formatTextOutput should format errors before messages`` () =
    let message = createMessage "W001" Severity.Warning "Test warning" "Test Analyzer"
    let result = createAnalysisResult [message] ["Test error occurred"]
    let output = formatTextOutput result false
    
    let expectedOutput = """Error: Test error occurred
[W001] warning: Test warning"""
    Assert.Equal(expectedOutput, output)

// === SARIF Output Tests ===

[<Fact>]
let ``formatSarifOutput should produce valid JSON`` () =
    let message = createMessage "W001" Severity.Warning "Test warning" "Test Analyzer"
    let result = createAnalysisResult [message] []
    let output = formatSarifOutput result
    
    // Should be valid JSON
    let document = JsonDocument.Parse(output)
    Assert.NotNull(document)

[<Fact>]
let ``formatSarifOutput should have valid SARIF structure`` () =
    let message = createMessage "W001" Severity.Warning "Test warning" "Test Analyzer"
    let result = createAnalysisResult [message] []
    let output = formatSarifOutput result
    
    let document = JsonDocument.Parse(output)
    let root = document.RootElement
    
    // Check SARIF version
    Assert.Equal("2.1.0", root.GetProperty("version").GetString())
    
    // Check schema
    Assert.Equal("https://raw.githubusercontent.com/oasis-tcs/sarif-spec/master/Schemata/sarif-schema-2.1.0.json", root.GetProperty("$schema").GetString())
    
    // Check runs array
    let runs = root.GetProperty("runs")
    Assert.Equal(JsonValueKind.Array, runs.ValueKind)
    Assert.Equal(1, runs.GetArrayLength())
    
    // Check run structure
    let firstRun = runs.EnumerateArray() |> Seq.head
    let tool = firstRun.GetProperty("tool")
    let results = firstRun.GetProperty("results")
    
    // Check tool structure
    let driver = tool.GetProperty("driver")
    Assert.Equal("FSharp.LintKit", driver.GetProperty("name").GetString())
    Assert.True(driver.GetProperty("version").GetString().Length > 0)
    Assert.True(driver.GetProperty("informationUri").GetString().Length > 0)
    Assert.Equal(JsonValueKind.Array, driver.GetProperty("rules").ValueKind)
    
    // Check results structure
    Assert.Equal(JsonValueKind.Array, results.ValueKind)
    Assert.Equal(1, results.GetArrayLength())
    
    let firstResult = results.EnumerateArray() |> Seq.head
    Assert.Equal("W001", firstResult.GetProperty("ruleId").GetString())
    Assert.Equal("warning", firstResult.GetProperty("level").GetString())
    Assert.Equal("Test warning", firstResult.GetProperty("message").GetProperty("text").GetString())
    Assert.Equal(JsonValueKind.Array, firstResult.GetProperty("locations").ValueKind)

[<Fact>]
let ``formatSarifOutput should include SARIF schema`` () =
    let result = createAnalysisResult [] []
    let output = formatSarifOutput result
    
    let document = JsonDocument.Parse(output)
    let root = document.RootElement
    
    // Check SARIF version
    Assert.Equal("2.1.0", root.GetProperty("version").GetString())
    
    // Check schema
    Assert.Equal("https://raw.githubusercontent.com/oasis-tcs/sarif-spec/master/Schemata/sarif-schema-2.1.0.json", root.GetProperty("$schema").GetString())
    
    // Check runs array
    let runs = root.GetProperty("runs")
    Assert.Equal(1, runs.GetArrayLength())
    
    // Check run structure
    let firstRun = runs.EnumerateArray() |> Seq.head
    let results = firstRun.GetProperty("results")
    Assert.Equal(0, results.GetArrayLength())
    
    // Check tool structure
    let tool = firstRun.GetProperty("tool")
    let driver = tool.GetProperty("driver")
    Assert.Equal("FSharp.LintKit", driver.GetProperty("name").GetString())
    Assert.True(driver.GetProperty("version").GetString().Length > 0)
    Assert.True(driver.GetProperty("informationUri").GetString().Length > 0)
    Assert.Equal(0, driver.GetProperty("rules").GetArrayLength())

[<Fact>]
let ``formatSarifOutput should include tool information`` () =
    let result = createAnalysisResult [] []
    let output = formatSarifOutput result
    
    let document = JsonDocument.Parse(output)
    let root = document.RootElement
    let firstRun = root.GetProperty("runs").EnumerateArray() |> Seq.head
    let tool = firstRun.GetProperty("tool")
    let driver = tool.GetProperty("driver")
    
    // Check tool information
    Assert.Equal("FSharp.LintKit", driver.GetProperty("name").GetString())
    Assert.True(driver.GetProperty("version").GetString().Length > 0)
    Assert.True(driver.GetProperty("informationUri").GetString().Contains("FSharp.LintKit"))
    Assert.Equal(JsonValueKind.Array, driver.GetProperty("rules").ValueKind)

// === Boundary Value Tests ===

[<Fact>]
let ``formatTextOutput should handle multiple messages`` () =
    let messages = [
        createMessage "W001" Severity.Warning "First warning" "Analyzer1"
        createMessage "E001" Severity.Error "First error" "Analyzer2"
        createMessage "I001" Severity.Info "First info" "Analyzer3"
    ]
    let result = createAnalysisResult messages []
    let output = formatTextOutput result false
    
    let expectedOutput = """[W001] warning: First warning
[E001] error: First error
[I001] info: First info"""
    Assert.Equal(expectedOutput, output)

[<Fact>]
let ``formatTextOutput should handle messages with special characters`` () =
    let message = createMessage "W001" Severity.Warning "Message with \"quotes\" and <tags>" "Test Analyzer"
    let result = createAnalysisResult [message] []
    let output = formatTextOutput result false
    
    let expectedOutput = "[W001] warning: Message with \"quotes\" and <tags>"
    Assert.Equal(expectedOutput, output)

[<Fact>]
let ``formatSarifOutput should handle empty results`` () =
    let result = createAnalysisResult [] []
    let output = formatSarifOutput result
    
    let document = JsonDocument.Parse(output)
    let runs = document.RootElement.GetProperty("runs")
    let firstRun = runs.EnumerateArray() |> Seq.head
    let results = firstRun.GetProperty("results")
    
    Assert.Equal(0, results.GetArrayLength())

[<Fact>]
let ``formatSarifOutput should handle very long messages`` () =
    let longMessage = String.replicate 1000 "Very long message. "
    let message = createMessage "W001" Severity.Warning longMessage "Test Analyzer"
    let result = createAnalysisResult [message] []
    let output = formatSarifOutput result
    
    // Should still produce valid JSON
    let document = JsonDocument.Parse(output)
    Assert.NotNull(document)

// === Triangulation Tests ===

[<Fact>]
let ``formatSarifOutput should group messages by rule code`` () =
    let messages = [
        createMessage "W001" Severity.Warning "First W001" "Analyzer1"
        createMessage "W001" Severity.Warning "Second W001" "Analyzer1"
        createMessage "E001" Severity.Error "First E001" "Analyzer2"
    ]
    let result = createAnalysisResult messages []
    let output = formatSarifOutput result
    
    let document = JsonDocument.Parse(output)
    let runs = document.RootElement.GetProperty("runs")
    let firstRun = runs.EnumerateArray() |> Seq.head
    let rules = firstRun.GetProperty("tool").GetProperty("driver").GetProperty("rules")
    
    // Should have 2 rules (W001 and E001)
    Assert.Equal(2, rules.GetArrayLength())

[<Fact>]
let ``formatOutput should delegate to correct formatter`` () =
    let message = createMessage "W001" Severity.Warning "Test warning" "Test Analyzer"
    let result = createAnalysisResult [message] []
    
    let textOutput = formatOutput Text result false
    let sarifOutput = formatOutput Sarif result false
    
    let expectedTextOutput = "[W001] warning: Test warning"
    Assert.Equal(expectedTextOutput, textOutput)
    
    // For SARIF, verify it produces valid JSON with expected structure
    let document = JsonDocument.Parse(sarifOutput)
    let root = document.RootElement
    Assert.Equal("2.1.0", root.GetProperty("version").GetString())
    Assert.True(root.GetProperty("$schema").GetString().Contains("sarif-schema"))
    Assert.Equal(1, root.GetProperty("runs").GetArrayLength())

[<Fact>]
let ``formatTextOutput should handle different severity types correctly`` () =
    let messages = [
        createMessage "W001" Severity.Warning "Warning message" "Analyzer1"
        createMessage "E001" Severity.Error "Error message" "Analyzer2" 
        createMessage "I001" Severity.Info "Info message" "Analyzer3"
        createMessage "H001" Severity.Hint "Hint message" "Analyzer4"
    ]
    let result = createAnalysisResult messages []
    let output = formatTextOutput result false
    
    let expectedOutput = """[W001] warning: Warning message
[E001] error: Error message
[I001] info: Info message
[H001] hint: Hint message"""
    Assert.Equal(expectedOutput, output)

[<Fact>]
let ``formatSarifOutput should map severity types correctly`` () =
    let messages = [
        createMessage "W001" Severity.Warning "Warning" "Analyzer1"
        createMessage "E001" Severity.Error "Error" "Analyzer2"
        createMessage "I001" Severity.Info "Info" "Analyzer3"
        createMessage "H001" Severity.Hint "Hint" "Analyzer4"
    ]
    let result = createAnalysisResult messages []
    let output = formatSarifOutput result
    
    let document = JsonDocument.Parse(output)
    let root = document.RootElement
    let firstRun = root.GetProperty("runs").EnumerateArray() |> Seq.head
    let results = firstRun.GetProperty("results")
    
    // Should have 4 results
    Assert.Equal(4, results.GetArrayLength())
    
    let resultArray = results.EnumerateArray() |> Seq.toArray
    
    // Check Warning severity mapping
    let warningResult = resultArray.[0]
    Assert.Equal("W001", warningResult.GetProperty("ruleId").GetString())
    Assert.Equal("warning", warningResult.GetProperty("level").GetString())
    Assert.Equal("Warning", warningResult.GetProperty("message").GetProperty("text").GetString())
    
    // Check Error severity mapping
    let errorResult = resultArray.[1]
    Assert.Equal("E001", errorResult.GetProperty("ruleId").GetString())
    Assert.Equal("error", errorResult.GetProperty("level").GetString())
    Assert.Equal("Error", errorResult.GetProperty("message").GetProperty("text").GetString())
    
    // Check Info severity mapping
    let infoResult = resultArray.[2]
    Assert.Equal("I001", infoResult.GetProperty("ruleId").GetString())
    Assert.Equal("note", infoResult.GetProperty("level").GetString())
    Assert.Equal("Info", infoResult.GetProperty("message").GetProperty("text").GetString())
    
    // Check Hint severity mapping
    let hintResult = resultArray.[3]
    Assert.Equal("H001", hintResult.GetProperty("ruleId").GetString())
    Assert.Equal("note", hintResult.GetProperty("level").GetString())
    Assert.Equal("Hint", hintResult.GetProperty("message").GetProperty("text").GetString())
    
    // Check rules are properly generated
    let driver = firstRun.GetProperty("tool").GetProperty("driver")
    let rules = driver.GetProperty("rules")
    Assert.Equal(4, rules.GetArrayLength())
    
    let ruleArray = rules.EnumerateArray() |> Seq.toArray
    Assert.Equal("W001", ruleArray.[0].GetProperty("id").GetString())
    Assert.Equal("E001", ruleArray.[1].GetProperty("id").GetString())
    Assert.Equal("I001", ruleArray.[2].GetProperty("id").GetString())
    Assert.Equal("H001", ruleArray.[3].GetProperty("id").GetString())
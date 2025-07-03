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
let ``formatSarifOutput should include SARIF schema`` () =
    let result = createAnalysisResult [] []
    let output = formatSarifOutput result
    
    let expectedOutput = """{
  "$schema": "https://raw.githubusercontent.com/oasis-tcs/sarif-spec/master/Schemata/sarif-schema-2.1.0.json",
  "runs": [
    {
      "results": [],
      "tool": {
        "driver": {
          "informationUri": "https://github.com/yourusername/FSharp.LintKit",
          "name": "FSharp.LintKit",
          "rules": [],
          "version": "0.1.0"
        }
      }
    }
  ],
  "version": "2.1.0"
}"""
    Assert.Equal(expectedOutput, output)

[<Fact>]
let ``formatSarifOutput should include tool information`` () =
    let result = createAnalysisResult [] []
    let output = formatSarifOutput result
    
    // This test is covered by the SARIF schema test above, so we can verify it's the same
    let expectedOutput = """{
  "$schema": "https://raw.githubusercontent.com/oasis-tcs/sarif-spec/master/Schemata/sarif-schema-2.1.0.json",
  "runs": [
    {
      "results": [],
      "tool": {
        "driver": {
          "informationUri": "https://github.com/yourusername/FSharp.LintKit",
          "name": "FSharp.LintKit",
          "rules": [],
          "version": "0.1.0"
        }
      }
    }
  ],
  "version": "2.1.0"
}"""
    Assert.Equal(expectedOutput, output)

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
    
    // For SARIF, just verify it contains the schema (since full SARIF is very long)
    Assert.Contains("sarif-schema", sarifOutput)

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
    
    let expectedOutput = """{
  "$schema": "https://raw.githubusercontent.com/oasis-tcs/sarif-spec/master/Schemata/sarif-schema-2.1.0.json",
  "runs": [
    {
      "results": [
        {
          "level": "warning",
          "locations": [
            {
              "physicalLocation": {
                "artifactLocation": {
                  "uri": "unknown"
                },
                "region": {
                  "endColumn": 1,
                  "endLine": 1,
                  "startColumn": 1,
                  "startLine": 1
                }
              }
            }
          ],
          "message": {
            "text": "Warning"
          },
          "ruleId": "W001"
        },
        {
          "level": "error",
          "locations": [
            {
              "physicalLocation": {
                "artifactLocation": {
                  "uri": "unknown"
                },
                "region": {
                  "endColumn": 1,
                  "endLine": 1,
                  "startColumn": 1,
                  "startLine": 1
                }
              }
            }
          ],
          "message": {
            "text": "Error"
          },
          "ruleId": "E001"
        },
        {
          "level": "note",
          "locations": [
            {
              "physicalLocation": {
                "artifactLocation": {
                  "uri": "unknown"
                },
                "region": {
                  "endColumn": 1,
                  "endLine": 1,
                  "startColumn": 1,
                  "startLine": 1
                }
              }
            }
          ],
          "message": {
            "text": "Info"
          },
          "ruleId": "I001"
        },
        {
          "level": "note",
          "locations": [
            {
              "physicalLocation": {
                "artifactLocation": {
                  "uri": "unknown"
                },
                "region": {
                  "endColumn": 1,
                  "endLine": 1,
                  "startColumn": 1,
                  "startLine": 1
                }
              }
            }
          ],
          "message": {
            "text": "Hint"
          },
          "ruleId": "H001"
        }
      ],
      "tool": {
        "driver": {
          "informationUri": "https://github.com/yourusername/FSharp.LintKit",
          "name": "FSharp.LintKit",
          "rules": [
            {
              "defaultConfiguration": {
                "level": "warning"
              },
              "fullDescription": {
                "text": "Warning"
              },
              "id": "W001",
              "name": "Analyzer1",
              "shortDescription": {
                "text": "Warning"
              }
            },
            {
              "defaultConfiguration": {
                "level": "warning"
              },
              "fullDescription": {
                "text": "Error"
              },
              "id": "E001",
              "name": "Analyzer2",
              "shortDescription": {
                "text": "Error"
              }
            },
            {
              "defaultConfiguration": {
                "level": "warning"
              },
              "fullDescription": {
                "text": "Info"
              },
              "id": "I001",
              "name": "Analyzer3",
              "shortDescription": {
                "text": "Info"
              }
            },
            {
              "defaultConfiguration": {
                "level": "warning"
              },
              "fullDescription": {
                "text": "Hint"
              },
              "id": "H001",
              "name": "Analyzer4",
              "shortDescription": {
                "text": "Hint"
              }
            }
          ],
          "version": "0.1.0"
        }
      }
    }
  ],
  "version": "2.1.0"
}"""
    
    Assert.Equal(expectedOutput, output)
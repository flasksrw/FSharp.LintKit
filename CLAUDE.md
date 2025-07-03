# F# Custom Lint Framework Project Requirements

## Overview
This project implements a foundation tool for applying custom lint rules to F# source code, automatically auditing code quality and security.

Traditional FSharpLint lacks a mechanism to dynamically load external plugin DLLs, making it difficult to flexibly operate project or team-specific rules. This project uses FSharp.Analyzers.SDK to build a plugin-based custom lint system.

This tool aims to serve as a guardrail for AI code generation in the future.

---

## Goals
- **Provide a command-line tool that dynamically loads arbitrary F# analyzer DLLs and executes lint**
- **Enable independent distribution of rule DLLs**
- **Implement output formats that can be integrated into build/CI pipelines**
- **Provide at least one sample custom rule**

---

## Functional Requirements

### 1. CLI
- Distribute the LintKit.CLI project as a .NET Global Tool
- Enable installation via `dotnet tool install -g FSharp.LintKit`
- After installation, execute with `dotnet fsharplintkit` command
- Users can specify rule DLLs with `--analyzers` option to analyze arbitrary directories
- The command supports the following minimum options:
  - `--analyzers`: Path to rule DLLs (multiple allowed)
  - `--target`: Target folder or file
  - `--format`: Output format (text, sarif)
  - `--verbose`: Detailed log output
  - `--quiet`: Minimal output
  - `--help`: Help display

### 2. Rule Definition
- Enable implementation of rules using FSharp.Analyzers.SDK's `Analyzer` type as DLLs
- Rules are written in F# code and can be built as independent projects

### 3. Rule Loading
- Load one or more rule DLLs from paths specified to the CLI
- Support batch loading of multiple DLLs

### 4. Target Specification
- Support specifying lint targets in the following ways:
  - Solution file (discover project files included in the solution)
  - Project file (discover .fs files included in the project)
  - Folder (recursively discover .fs files)
  - Single file

### 5. Output
- Output lint results to standard output
- Support the following output formats at minimum:
  - Plain text
  - SARIF (Static Analysis Results Interchange Format) for CI tool integration
- Return exit codes based on the number of rule violations:
  - Violations present: exit code 1
  - No violations: exit code 0

### 6. Testing
- Unit tests for correct rule application
- E2E tests for the CLI

---

## Non-Functional Requirements
- Works on .NET 8 or later
- Cross-platform (Windows/macOS/Linux)
- Can execute without project dependencies once installed

---

## Sample Rule Requirements
Provide the following rule as a minimum sample:
- Warn if a file contains `open System.IO`

---

## Project Structure
```
/FSharp.LintKit
  /src
    /LintKit.CLI                   # CLI host
      Program.fs                   # CLI entry point
      AnalyzerLoader.fs            # Load analyzers from DLLs
      Runner.fs                    # Actual lint execution logic
      Output.fs                    # SARIF/Text output processing
    /LintKit.Analyzers             # (Sample rules)
      ForbiddenOpenRule.fs         # "Forbidden open System.IO" rule
  /templates                       # Templates for custom rules
    /MyCustomAnalyzer
      /MyCustomAnalyzer.fsproj
      /SampleAnalyzer.fs
      /SampleAnalyzer.Tests.fsproj
  /tests
    /LintKit.Tests
README.md
LICENSE
```

---

## Future Extensions
- Visual Studio Code Ionide integration
- MSBuild task
- Rule enable/disable control via configuration files (YAML/JSON)
- GitHub Actions templates
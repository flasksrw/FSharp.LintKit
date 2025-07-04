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

## LintKit.AnalyzerPatterns Project
**Purpose**: AST Analysis Pattern Reference for AI Code Generation

The LintKit.AnalyzerPatterns project serves as a comprehensive reference implementation collection for F# AST analysis patterns. It is **NOT** a required dependency for LintKit.CLI, but rather a learning resource for developers (including AI agents) who want to create custom analyzers.

### Key Characteristics:
- **AI-Optimized Code Examples**: All code includes detailed type annotations and explicit pattern matching for AI agents to learn from
- **Complete AST Coverage**: Demonstrates how to handle all major SynExpr, SynPat, and other F# Compiler syntax tree nodes
- **Best Practices**: Shows proper use of Severity levels, message creation, and range handling
- **Self-Contained**: Each analyzer file is a complete, working example that can be copied and modified

### Sample Implementation Areas:
- **Working Analyzer Example** (SimpleAnalyzerExample.fs - TODO detection with full implementation)
- **Working Test Patterns** (SimpleAnalyzerExampleTests.fs - correct FSharp.Analyzers.SDK.Testing usage)
- **Complete AST pattern matching references**:
  - SynExpr patterns (all expression types with detailed type annotations)
  - SynModuleDecl patterns (module declarations: let, type, open, nested modules)
  - SynPat patterns (pattern matching constructs)
  - SynType patterns (type expressions and annotations)
- **Attribute detection and analysis** (getting custom attributes from declarations)
- **Type annotation checking** (detecting presence/absence of type annotations)
- **Naming convention enforcement** (test prefixes, PascalCase, camelCase patterns)
- **Severity usage guidelines** (Error/Warning/Info/Hint with concrete examples)
- **Complex nested AST traversal patterns**

### Usage:
1. **For Human Developers**: Reference implementations to understand F# AST analysis
2. **For AI Agents**: Concrete patterns to copy and adapt for new analyzer rules
3. **For Learning**: Progressive examples from simple to complex AST operations

### Working Implementation Examples:
- **SimpleAnalyzerExample.fs**: Complete TODO detection analyzer with proper error handling
- **SimpleAnalyzerExampleTests.fs**: Working test suite using correct FSharp.Analyzers.SDK.Testing patterns
- **Critical Testing Pattern**: `mkOptionsFromProject "net9.0" [] |> Async.AwaitTask` for proper async setup

---

## Project Structure
```
/FSharp.LintKit
  /src
    /LintKit.CLI                   # CLI host (main deliverable)
      Program.fs                   # CLI entry point
      AnalyzerLoader.fs            # Load analyzers from DLLs
      Runner.fs                    # Actual lint execution logic
      Output.fs                    # SARIF/Text output processing
    /LintKit.AnalyzerPatterns      # AI-optimized AST analysis pattern reference
      SimpleAnalyzerExample.fs       # Working analyzer example with TODO detection
      SynExprPatterns.fs             # Complete SynExpr pattern matching reference
      SynModuleDeclPatterns.fs       # Complete SynModuleDecl pattern matching reference
      SynPatPatterns.fs              # Complete SynPat pattern matching reference
      SynTypePatterns.fs             # Complete SynType pattern matching reference
      AttributeDetection.fs          # Attribute detection and analysis examples
      TypeAnnotationChecker.fs       # Type annotation presence checking
      NamingConventions.fs           # Naming convention enforcement patterns
      SeverityGuide.fs               # Severity level usage examples (Error/Warning/Info/Hint)
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

## AI-Driven Rule Implementation System

### Goal: Complete AI Automation for Custom Rule Development
The LintKit system is designed to enable full AI automation of custom rule implementation. Human developers specify their requirements, and AI agents generate complete analyzer implementations with tests.

### Components:
1. **Human-Readable Rule Implementation Guide** (`templates/RULE_IMPLEMENTATION_GUIDE_JA.md`, `templates/RULE_IMPLEMENTATION_GUIDE_EN.md`)
   - Step-by-step instructions for humans to understand the rule creation process
   - Examples of rule specifications and expected outputs
   - Best practices for rule design and testing
   - Available in both Japanese and English

2. **AI-Optimized Rule Implementation Instructions** (`templates/AI_RULE_IMPLEMENTATION.md`)
   - Machine-readable instructions for AI agents to implement rules
   - Standardized rule specification format
   - Template sections for human input (rule requirements list)
   - Complete workflow from specification to implementation

3. **Rule Specification Format**
   - Structured format for humans to describe rule requirements
   - Categories: Code Quality, Security, Performance, Naming, etc.
   - Severity levels and expected behavior specifications
   - Test case requirements

### Workflow:
1. **Human Input**: Fill out rule specifications in AI instruction document
2. **AI Processing**: AI agent reads specifications and generates:
   - Complete analyzer implementation with proper F# AST pattern matching
   - Comprehensive test suite following t-wada methodology
   - Documentation and usage examples
3. **Integration**: Generated analyzers work seamlessly with LintKit.CLI

### Benefits:
- **Zero Manual Coding**: Rules implemented entirely by AI based on specifications
- **Consistent Quality**: All implementations follow established patterns from LintKit.AnalyzerPatterns
- **Comprehensive Testing**: AI generates thorough test coverage automatically
- **Rapid Development**: From specification to working analyzer in minutes

---

## Distribution Strategy

### NuGet Packages
The project distributes as two complementary NuGet packages:

1. **FSharp.LintKit** (.NET Global Tool)
   - Installs command-line interface: `dotnet tool install -g FSharp.LintKit`
   - Provides `dotnet fsharplintkit` command for executing custom analyzers
   - Supports loading analyzer DLLs and running lint analysis

2. **FSharp.LintKit.Templates** (Project Template Package)
   - Installs project templates: `dotnet new install FSharp.LintKit.Templates`
   - Provides `dotnet new fsharplintkit-analyzer` command for creating custom analyzer projects
   - Includes AI instruction documents and working patterns

### Complete Usage Workflow
```bash
# 1. Install CLI tool
dotnet tool install -g FSharp.LintKit

# 2. Install project templates
dotnet new install FSharp.LintKit.Templates

# 3. Create custom analyzer project
dotnet new fsharplintkit-analyzer -n MyProjectRules

# 4. Fill out rule specifications in AI_RULE_IMPLEMENTATION.md
# 5. Use AI agent to generate complete analyzer implementation

# 6. Build the analyzer
dotnet build

# 7. Run lint analysis with custom rules
dotnet fsharplintkit --analyzers ./bin/Debug/net8.0/MyProjectRules.dll --target ./src
```

### Benefits of This Distribution Model
- **Easy Installation**: Standard .NET tooling commands
- **Automatic Updates**: `dotnet tool update` and template versioning
- **Cross-Platform**: Works on Windows/macOS/Linux
- **Enterprise-Friendly**: Can be hosted on internal NuGet feeds
- **AI Integration Ready**: Templates include all necessary AI instruction documents

---

## Future Extensions
- Visual Studio Code Ionide integration
- MSBuild task
- Rule enable/disable control via configuration files (YAML/JSON)
- GitHub Actions templates
# FSharp.LintKit

A powerful F# custom lint framework that enables **AI-driven rule implementation**. By specifying rules in the included AI instruction templates, AI agents can generate analyzer implementations with comprehensive tests.

## Overview

FSharp.LintKit provides a foundation for applying custom lint rules to F# source code. Unlike traditional linting tools, it focuses on **team-specific and project-specific rules** and supports AI agent-driven rule development.

### Why FSharp.LintKit?
- **AI-Ready Templates**: Comprehensive instruction templates for AI agents to generate implementations
- **Zero Coding**: No manual F# coding required for rule implementation  
- **Easy Distribution**: Standard .NET tooling with NuGet packages
- **Dynamic Loading**: Load custom analyzer DLLs at runtime
- **Team-Specific**: Perfect for enforcing project conventions and guidelines

## Key Features

- **AI-Driven Rule Implementation**: Automation from specification to working analyzer
- **Dynamic Analyzer Loading**: Load multiple custom rule DLLs at runtime
- **Comprehensive Testing**: Auto-generated test suites
- **Multiple Output Formats**: Plain text and SARIF for CI/CD integration
- **Cross-Platform**: Works on Windows, macOS, and Linux
- **Enterprise-Ready**: Internal NuGet feed support

## Quick Start

### 1. Install Tools
```bash
# Install CLI tool
dotnet tool install -g FSharp.LintKit

# Install project templates  
dotnet new install FSharp.LintKit.Templates
```

### 2. Create Custom Analyzer
```bash
# Create new analyzer project
dotnet new fsharplintkit-analyzer -n MyProjectRules

# Navigate to project
cd MyProjectRules
```

### 3. Specify Rules with AI
1. Open `AI_RULE_IMPLEMENTATION.md`
2. Fill in your rule specifications:
```markdown
### Rule 1: No Console.WriteLine
Rule Name: NoConsoleWriteLine
Category: CodeQuality  
Severity: Warning
Description: Console.WriteLine should not be used in production code
Detection Pattern: Console.WriteLine function calls
Message: Use proper logging instead of Console.WriteLine
```

3. Send the entire document to an AI agent:
> "Please implement the F# analyzers based on the rule specifications provided above."

### 4. Build and Use
```bash
# Build generated analyzer
dotnet build

# Run lint analysis
dotnet fsharplintkit --analyzers ./bin/Debug/net9.0/MyProjectRules.dll --target ./src
```

## Installation

### Prerequisites
- .NET 9.0 or later
- AI agent access (Claude, ChatGPT, etc.)

### Install CLI Tool
```bash
dotnet tool install -g FSharp.LintKit
```

### Install Project Templates
```bash
dotnet new install FSharp.LintKit.Templates
```

## Usage

### Basic Command Line Usage
```bash
# Analyze with custom rules
dotnet fsharplintkit --analyzers path/to/rules.dll --target ./src

# Multiple analyzers
dotnet fsharplintkit --analyzers rules1.dll --analyzers rules2.dll --target ./src

# SARIF output for CI
dotnet fsharplintkit --analyzers rules.dll --target ./src --format sarif

# Verbose output
dotnet fsharplintkit --analyzers rules.dll --target ./src --verbose
```

### Supported Targets
- **Solution files**: `--target MyProject.sln`
- **Project files**: `--target MyProject.fsproj` 
- **Directories**: `--target ./src` (recursive .fs file discovery)
- **Single files**: `--target MyFile.fs`

### Output Formats
- **text**: Human-readable console output (default)
- **sarif**: SARIF format for CI/CD integration

## AI-Driven Rule Implementation

### Complete Automation Workflow

1. **Human Input**: Describe rules in `AI_RULE_IMPLEMENTATION.md` template
2. **AI Agent**: Send template to AI agent for implementation
3. **Integration**: Generated analyzer works immediately with LintKit.CLI

### Rule Specification Format
```markdown
Rule Name: [Descriptive name]
Category: [CodeQuality/Security/Performance/Naming/Style]  
Severity: [Error/Warning/Info/Hint]
Description: [What to detect and why]
Detection Pattern: [Specific F# code patterns]
Exclusions: [Patterns to ignore]
Message: [User-facing message]
Fix Suggestion: [Recommended fixes]
```

## Example Use Cases

### Code Quality Rules
- Detect overly complex functions
- Enforce naming conventions
- Prevent deep nesting
- Ensure proper error handling

### Team Convention Rules
- Enforce project-specific patterns
- Ensure documentation standards
- Validate architecture decisions
- Check code organization

## Architecture

### Core Components
- **LintKit.CLI**: Command-line interface and analyzer engine
- **Templates**: NuGet project templates for custom analyzers with complete AST patterns

### Analyzer Framework
- Built on **FSharp.Analyzers.SDK**
- F# AST analysis through **FSharp.Compiler.Syntax**
- Dynamic DLL loading at runtime
- Message and diagnostic reporting

### AI Support
- Working pattern references for AI agents
- Comprehensive instruction documents
- Template-based code generation
- Automated test suite creation

## Documentation

### For Humans
- **English**: `templates/RULE_IMPLEMENTATION_GUIDE_EN.md`
- **Japanese**: `templates/RULE_IMPLEMENTATION_GUIDE_JA.md`

### For AI Agents  
- **Implementation Instructions**: `templates/MyCustomAnalyzer/AI_RULE_IMPLEMENTATION.md`
- **Complete AST Patterns**: `templates/MyCustomAnalyzer/TemplateAnalyzer.fs`
- **Test Patterns**: `templates/MyCustomAnalyzer/TemplateAnalyzerTests.fs`

### Technical Reference
- **Architecture Details**: F# AST analysis patterns
- **Testing Patterns**: FSharp.Analyzers.SDK.Testing usage

## Development

### Building from Source
```bash
git clone https://github.com/yourorg/FSharp.LintKit.git
cd FSharp.LintKit
dotnet build
dotnet test
```

### Project Structure
```
FSharp.LintKit/
├── src/
│   └── LintKit.CLI/              # Command-line interface
├── templates/
│   └── MyCustomAnalyzer/         # NuGet project template with complete AST patterns
├── tests/
│   └── LintKit.Tests/            # Unit tests for CLI components
└── README.md                     # This file
```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
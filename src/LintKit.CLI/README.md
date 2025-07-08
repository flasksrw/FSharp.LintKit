# FSharp.LintKit CLI

A command-line tool for running custom F# lint analyzers with dynamic DLL loading support.

## Installation

```bash
dotnet tool install -g FSharp.LintKit
```

## Quick Start

```bash
# Run analyzer on a directory
dotnet fsharplintkit --analyzers path/to/analyzer.dll --target ./src

# Run on specific file
dotnet fsharplintkit --analyzers analyzer.dll --target MyFile.fs

# SARIF output for CI/CD
dotnet fsharplintkit --analyzers analyzer.dll --target ./src --format sarif
```

## Usage

### Basic Command

```bash
fsharplintkit --analyzers <analyzer-dll> --target <path>
```

### Options

- `--analyzers <path>` - Path to analyzer DLL (can be specified multiple times)
- `--target <path>` - Target to analyze (file, directory, .fsproj, or .sln)
- `--format <format>` - Output format: `text` (default) or `sarif`
- `--verbose` - Enable detailed output
- `--quiet` - Minimal output

### Multiple Analyzers

```bash
fsharplintkit --analyzers rules1.dll --analyzers rules2.dll --target ./src
```

### Target Types

- **Directory**: `--target ./src` (recursively finds .fs files)
- **Solution**: `--target MyProject.sln` 
- **Project**: `--target MyProject.fsproj`
- **Single file**: `--target MyFile.fs`

## Creating Custom Analyzers

To create your own analyzer rules, install the project templates:

```bash
dotnet new install FSharp.LintKit.Templates
dotnet new fsharplintkit-analyzer -n MyCustomRules
```

See the [main repository](https://github.com/flasksrw/FSharp.LintKit) for detailed documentation on creating custom analyzers with AI assistance.

## Exit Codes

- `0` - No violations found
- `1` - Violations found
- `2` - Error occurred

## Requirements

- .NET 9.0 or later
- Custom analyzer DLLs built with FSharp.Analyzers.SDK

## Links

- [Repository](https://github.com/flasksrw/FSharp.LintKit)
- [Project Templates](https://www.nuget.org/packages/FSharp.LintKit.Templates)
- [Issues](https://github.com/flasksrw/FSharp.LintKit/issues)
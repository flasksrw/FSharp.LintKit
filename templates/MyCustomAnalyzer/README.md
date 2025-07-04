# MyCustomAnalyzer

This is a custom F# analyzer project created with FSharp.LintKit templates.

## 🤖 FOR AI AGENTS - START HERE

**You are an AI agent implementing F# analyzers. Follow these steps:**

1. **Read the rule specifications** in `AI_RULE_IMPLEMENTATION.md`
2. **Edit the existing files** (DO NOT create new files):
   - `CustomAnalyzer.fs` - Replace TODO implementations with your analyzer logic
   - `CustomAnalyzerTests.fs` - Add comprehensive test cases
   - `MyCustomAnalyzer.fsproj` - Adjust if needed
3. **Reference the working patterns** from LintKit.AnalyzerPatterns
4. **Build and test** your implementation

## 👨‍💻 FOR HUMAN DEVELOPERS

This project is designed for AI-driven rule implementation:

1. **Fill out rule specifications** in `AI_RULE_IMPLEMENTATION.md`
2. **Use AI agent** to generate complete analyzer implementation
3. **Build and test** the generated analyzer
4. **Use with LintKit** to analyze F# code

## Quick Start

### 1. Specify Your Rules
Open `AI_RULE_IMPLEMENTATION.md` and fill out the rule specifications section with your custom rules.

### 2. Generate Implementation
Send the completed `AI_RULE_IMPLEMENTATION.md` to an AI agent with the instruction:

> "Please implement the F# analyzers based on the rule specifications provided above."

### 3. Build and Test
```bash
dotnet build
dotnet test
```

### 4. Use with LintKit
```bash
dotnet fsharplintkit --analyzers ./bin/Debug/net8.0/MyCustomAnalyzer.dll --target ./src
```

## Documentation Files

- `AI_RULE_IMPLEMENTATION.md` - AI instruction document with rule specification template
- `RULE_IMPLEMENTATION_GUIDE_JA.md` - Human implementation guide (Japanese)
- `RULE_IMPLEMENTATION_GUIDE_EN.md` - Human implementation guide (English)

## Project Structure

- `CustomAnalyzer.fs` - Main analyzer implementation (AI will complete this)
- `CustomAnalyzerTests.fs` - Test suite (AI will complete this)
- `MyCustomAnalyzer.fsproj` - Project file with all dependencies

## Reference Patterns

The AI agent will reference working patterns from the LintKit.AnalyzerPatterns project:
- SimpleAnalyzerExample.fs - Complete analyzer implementation
- SimpleAnalyzerExampleTests.fs - Working test patterns
- SynExprPatterns.fs - AST pattern matching examples

## Next Steps

1. Customize rule specifications in `AI_RULE_IMPLEMENTATION.md`
2. Use AI to generate your analyzer implementation
3. Build, test, and start analyzing F# code!
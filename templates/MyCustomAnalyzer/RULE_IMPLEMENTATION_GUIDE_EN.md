# FSharp.LintKit Custom Rule Implementation Guide

## 1. Overview

FSharp.LintKit is a framework for applying custom lint rules to F# source code. This guide explains how to automatically implement custom rules using AI agents.

### Key Features
- **Full AI Automation**: AI generates complete implementations and tests from rule specifications
- **Zero Coding**: No manual coding required for humans
- **Comprehensive Testing**: Automatic generation of exhaustive test cases
- **Immediate Integration**: Generated analyzers work seamlessly with LintKit.CLI

## 2. Prerequisites

### System Requirements
- .NET 8 or later
- FSharp.LintKit project (this repository)
- AI agent (Claude, ChatGPT, etc.)

### Required Knowledge
- Basic F# syntax (detailed knowledge not required as AI handles implementation)
- Basic static analysis concepts
- Understanding of code patterns you want to detect

## 3. Rule Implementation Workflow

### Step 1: Create Rule Specifications
1. Open `AI_RULE_IMPLEMENTATION.md`
2. Find the "RULE SPECIFICATIONS" section
3. Describe the rules you want to implement

### Step 2: Launch AI Agent
1. Send the entire content of AI_RULE_IMPLEMENTATION.md to an AI agent
2. Instruct: "Please implement the rules based on the above specifications"

### Step 3: Review Implementation
1. Review the generated analyzer files
2. Review the test files
3. Adjust specifications and regenerate if needed

### Step 4: Integration and Testing
1. Build the generated project
2. Test with LintKit.CLI
3. Deploy to production environment

## 4. Rule Specification Format

### Basic Format
```
Rule Name: [Descriptive name]
Category: [CodeQuality/Security/Performance/Naming/Style]
Severity: [Error/Warning/Info/Hint]
Description: [What to detect and why it's problematic]
Detection Pattern: [Specific F# code patterns]
Exclusions: [Patterns to ignore]
Message: [Message to display to developers]
Fix Suggestion: [Suggested fixes if applicable]
```

### Example
```
Rule Name: ConsecutiveListAppend
Category: Performance
Severity: Warning
Description: Consecutive @ operators on lists cause poor performance
Detection Pattern: list1 @ list2 @ list3 style consecutive @ operators
Exclusions: Only two list concatenations
Message: Consecutive list concatenations should use List.concat
Fix Suggestion: List.concat [list1; list2; list3]
```

## 5. Using AI Instructions

### Basic Usage
1. Open `AI_RULE_IMPLEMENTATION.md`
2. Fill in rule specifications in the "RULE SPECIFICATIONS" section
3. Send the entire file to AI agent
4. Instruct: "Please implement these rules"

### Tips for Effective Instructions
- **Provide specific code examples**: Show the patterns you want to detect with actual F# code
- **Clarify expected behavior**: Clearly separate what should be detected vs. ignored
- **Specify priority**: Indicate implementation order for multiple rules

### Re-adjustment Process
- If generated code doesn't match expectations, refine specifications and resend
- Use "Please modify the previous implementation to fix..." for partial corrections

## 6. Implementation Examples

### Example 1: Simple Pattern Detection
```
Rule Name: ForbiddenSystemIO
Category: Security
Severity: Warning
Description: Direct System.IO usage poses security risks
Detection Pattern: "open System.IO" statements
Exclusions: None
Message: Direct System.IO usage is not recommended. Use wrapper libraries.
```

### Example 2: Complex AST Analysis
```
Rule Name: DeepNestingWarning
Category: CodeQuality
Severity: Info
Description: Deep nesting reduces code readability
Detection Pattern: if-then-else, match expressions, let expressions nested 5+ levels
Exclusions: Simple pattern matching
Message: Nesting too deep ({0} levels). Consider function decomposition.
```

## 7. Test Verification

### Auto-generated Tests
AI automatically generates:
- **Positive tests**: Rules correctly detect target patterns
- **Boundary tests**: Edge cases between detection/non-detection
- **Negative tests**: Patterns that should not be detected
- **Error handling**: No crashes on malformed syntax

### Running Tests
```bash
# Run in generated test project
cd YourCustomAnalyzer
dotnet test
```

### Reviewing Test Results
- Ensure all tests pass (green)
- If tests fail, review specifications and ask AI for corrections

## 8. Troubleshooting

### Common Issues

#### Q: AI-generated code has compilation errors
**A**: Instruct the AI to reference LintKit.AnalyzerPatterns project and use correct pattern matching.

#### Q: Rules don't work as expected
**A**: Provide more specific code examples and clearly separate detection vs. non-detection patterns.

#### Q: Tests are failing
**A**: Review test cases and verify expected behavior is correctly described.

#### Q: Performance issues
**A**: Avoid complex pattern matching and instruct AI to use efficient AST traversal patterns.

#### Q: Rules are too complex to implement
**A**: Consider breaking complex rules into stages:
- Basic pattern detection rule
- Condition-adding rule
- Combined pattern detection rule
Multiple simple rules are more maintainable and easier for AI to implement.

### Support Resources
- **AI Instructions**: `AI_RULE_IMPLEMENTATION.md` (included in this template)
- **Pattern Reference**: https://github.com/flasksrw/FSharp.LintKit/tree/main/src/LintKit.AnalyzerPatterns
- **Template**: https://github.com/flasksrw/FSharp.LintKit/tree/main/templates/MyCustomAnalyzer
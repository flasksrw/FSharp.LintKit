namespace LintKit.Analyzers

open FSharp.Analyzers.SDK

module OptionValueAnalyzer =
    // This attribute is required and needs to match the correct context type!
    [<CliAnalyzer>]
    let optionValueAnalyzer: Analyzer<CliContext> =
        fun (context: CliContext) ->
            async {
                // inspect context to determine the error/warning messages
                // A potential implementation might traverse the untyped syntax tree
                // to find any references of `Option.Value`
                return
                    [
                        {
                            Type = "Option.Value analyzer"
                            Message = "Option.Value shouldn't be used"
                            Code = "OV001"
                            Severity = Severity.Warning
                            Range = FSharp.Compiler.Text.Range.Zero
                            Fixes = []
                        }
                    ]
            }

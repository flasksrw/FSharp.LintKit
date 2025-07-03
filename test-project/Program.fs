module TestProject.Program

open System.IO

let processData (data: string option) =
    let result = data.Value // This should trigger our analyzer
    result

[<EntryPoint>]
let main argv =
    let someOption = Some "hello world"
    let value = someOption.Value // Another violation
    printfn "%s" value
    0
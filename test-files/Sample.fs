module Sample

open System.IO
open System

let readFile (path: string) =
    File.ReadAllText(path)

let processData (data: string option) =
    let result = data.Value // This should trigger our analyzer
    result

let main () =
    let someOption = Some "hello"
    let value = someOption.Value // Another violation
    printfn "%s" value
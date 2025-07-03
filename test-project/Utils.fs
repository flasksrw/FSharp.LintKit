module TestProject.Utils

open System.IO

let readFileWithOption (path: string option) =
    match path with
    | Some p when File.Exists p -> 
        let content = File.ReadAllText p
        Some content
    | _ -> None

let unsafeGet (opt: 'a option) =
    opt.Value // This should also trigger the analyzer
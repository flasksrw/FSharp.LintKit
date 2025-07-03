module WithSystemIO

open System.IO // This should be flagged if we implement the forbidden import rule
open System.Text

let createFile (path: string) (content: string) =
    File.WriteAllText(path, content)

let readAllLines (path: string) =
    File.ReadAllLines(path)
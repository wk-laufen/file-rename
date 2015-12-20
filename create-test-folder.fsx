open System.IO

let dir = Path.Combine(__SOURCE_DIRECTORY__, "test")
let indexFilePath = Path.Combine(dir, "index.txt")

try
    Directory.Delete(dir, true)
with _ -> ()

Directory.CreateDirectory dir |> ignore
[1..10]
|> List.map (sprintf "File %d.pdf")
|> List.map (fun f -> Path.Combine(dir, f))
|> List.iter (fun p -> File.WriteAllText(p, sprintf "Content of file %s" p))

[5..13]
|> List.map (sprintf "Blub %d")
|> fun l -> File.WriteAllLines(indexFilePath, l)

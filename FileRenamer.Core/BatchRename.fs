module FileRenamer.Core.BatchRename

open System
open System.IO

let private createTempDir path =
    Directory.CreateDirectory path |> ignore
    { new IDisposable with member x.Dispose() = Directory.Delete(path, true) }

let private moveFile src dest =
    try
        File.Move(src, dest) |> Choice1Of2
    with e -> Choice2Of2 e.Message

let private deleteFile path =
    try
        File.Delete path |> Choice1Of2
    with e -> Choice2Of2 e.Message

let renameFiles dir indexFile renameProgress =
    let allLines = File.ReadAllLines indexFile
    let allFiles = Directory.GetFiles(dir, "*.pdf")

    let lines =
        allLines
        |> Array.take (Math.Min(allLines.Length, allFiles.Length))
        |> Array.toList

    let files =
        allFiles
        |> Array.take (Math.Min(allLines.Length, allFiles.Length))
        |> Array.toList

    let tmpDir = Path.Combine(dir, "tmp")
    use tmpDirDisposable = createTempDir tmpDir
    List.zip lines files
    |> List.map (fun (l, f) ->
        let newFilePath = Path.Combine(tmpDir, l + Path.GetExtension f)
        renameProgress f newFilePath
        moveFile f newFilePath
    )
    |> Choice.sequenceA
    |> Choice.bind(fun _ ->
        Directory.GetFiles tmpDir
        |> List.ofArray
        |> List.map (fun f ->
            let newFilePath = Path.Combine(Path.GetDirectoryName f, "..", Path.GetFileName f)
            deleteFile newFilePath
            |> Choice.bind (fun () -> moveFile f newFilePath)
            |> Choice.map (fun () -> Path.GetFileName newFilePath)
        )
        |> Choice.sequenceA
    )

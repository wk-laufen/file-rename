module FileRenamer.Core.BatchRename

open System
open System.IO

let private createTempDir path =
    Directory.CreateDirectory path |> ignore
    { new IDisposable with member x.Dispose() = Directory.Delete(path, true) }

let renameFiles dir indexFile renameProgress =
    let lines = File.ReadAllLines indexFile
    let files = Directory.GetFiles(dir, "*.pdf")

    let tmpDir = Path.Combine(dir, "tmp")
    use tmpDirDisposable = createTempDir tmpDir
    Seq.zip lines files
    |> Seq.map (fun (l, f) ->
        let newFilePath = Path.Combine(tmpDir, l + Path.GetExtension f)
        renameProgress f newFilePath
        File.Move(f, newFilePath)
    )
    |> Choice.sequence

    Directory.GetFiles tmpDir
    |> Seq.iter (fun f ->
        let newFilePath = Path.Combine(Path.GetDirectoryName f, "..", Path.GetFileName f)
        File.Delete newFilePath
        File.Move(f, newFilePath)
    )

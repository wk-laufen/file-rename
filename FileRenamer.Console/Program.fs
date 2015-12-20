module FileRenamer.Console.Program

open System
open System.IO

module Array =
    let tryItem idx (list: 'a array) =
        if idx >= 0 && idx < list.Length
        then Some list.[idx]
        else None

let getArg key =
    let args = Environment.GetCommandLineArgs()
    args
    |> Seq.tryFindIndex (fun a -> a.Equals("-" + key, StringComparison.InvariantCultureIgnoreCase))
    |> Option.bind (fun idx -> args |> Array.tryItem (idx + 1))

let assemblyDir =
    System.Reflection.Assembly.GetExecutingAssembly().Location
    |> Path.GetDirectoryName

let renameImpl dir indexFile =
    FileRenamer.Core.BatchRename.renameFiles dir indexFile (fun source target -> ())
    |> Choice.map (fun _ -> dir)

let rename() =
    let dir =
        getArg "dir"
        |> Choice.ofOption "Missing command line arg \"dir\""
        |> Choice.bind (fun dir ->
            Directory.Exists dir
            |> Choice.ofBool (sprintf "Directory %s doesn't exist" dir)
            |> Choice.map (fun () -> dir)
        )

    let indexFile =
        let fileName = "index.txt"
        let path = Path.Combine(assemblyDir, fileName)
        if File.Exists path
        then Choice1Of2 path
        else Choice2Of2 (sprintf "Couldn't find index file %s in %s." fileName assemblyDir)

    let (<*>) = Choice.apply
    (Choice1Of2 renameImpl) <*> dir <*> indexFile
    |> Choice.bind id

let log format =
    let continuation s =
        File.AppendAllText(Path.Combine(assemblyDir, "log.txt"), s + Environment.NewLine)
    Printf.ksprintf continuation format

[<EntryPoint>]
let main argv =
    try
        match rename() with
        | Choice1Of2 dir ->
            log "Successfully renamed files in %s" dir
            0
        | Choice2Of2 error ->
            log "Error while renaming files: %s" error
            -1
    with e ->
        eprintfn "%O" e
        log "%O" e
        reraise()

module FileRenamer.Console.Program

open System
open System.IO

let getArg key =
    let args = Environment.GetCommandLineArgs()
    args
    |> Seq.tryFindIndex (fun a -> a.Equals("-" + key, StringComparison.InvariantCultureIgnoreCase))
    |> Option.bind (fun idx -> args |> Array.tryItem (idx + 1))

let rename() =
    getArg "dir"
    |> Choice.ofOption "Missing command line arg \"dir\""
    |> Choice.bind (fun dir ->
        Directory.Exists dir
        |> Choice.ofBool (sprintf "Directory %s doesn't exist" dir)
        |> Choice.map (fun () -> dir)
    )
    |> Choice.bind (fun dir ->
        let filePattern = "*.txt"
        Directory.GetFiles(dir, filePattern)
        |> Seq.tryHead
        |> Choice.ofOption (sprintf "Index file (%s) not found in %s" filePattern dir);
        |> Choice.bind (fun indexFile ->
            FileRenamer.Core.BatchRename.renameFiles dir indexFile (fun source target -> ())
        )
        |> Choice.map (fun _ -> dir)
    )

let log =
    let continuation s =
        let dir =
            System.Reflection.Assembly.GetExecutingAssembly().Location
            |> Path.GetDirectoryName
        File.AppendAllLines(Path.Combine(dir, "log.txt"), [|s|])
    Printf.ksprintf continuation

[<EntryPoint>]
let main argv =
    match rename() with
    | Choice1Of2 dir ->
        log "Successfully renamed files in %s" dir
        0
    | Choice2Of2 error ->
        log "Error while renaming files: %s" error
        -1

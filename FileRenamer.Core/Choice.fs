module Choice

let ofOption error = function
    | Some x -> Choice1Of2 x
    | None -> Choice2Of2 error

let ofBool error = function
    | true -> Choice1Of2 ()
    | false -> Choice2Of2 error

let bind fn = function
    | Choice1Of2 x -> fn x
    | Choice2Of2 x -> Choice2Of2 x

let map fn = bind (fn >> Choice1Of2)

let apply fn x =
    match fn, x with
    | Choice1Of2 fn, Choice1Of2 x -> Choice1Of2 (fn x)
    | Choice1Of2 fn, Choice2Of2 error -> Choice2Of2 error
    | Choice2Of2 error, Choice1Of2 x -> Choice2Of2 error
    | Choice2Of2 error1, Choice2Of2 error2 -> Choice2Of2 (error1 + "\r\n" + error2)

let traverseA f list =
    let (<*>) = apply
    let retn = Choice1Of2

    let cons head tail = head :: tail

    let initState = retn []
    let folder head tail = 
        retn cons <*> (f head) <*> tail

    List.foldBack folder list initState

let sequenceA list = traverseA id list

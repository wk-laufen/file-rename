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

let apply 

let sequence 

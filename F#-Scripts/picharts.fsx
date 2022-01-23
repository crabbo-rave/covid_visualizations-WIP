(* Code that generates the Pi Charts *)

#r "nuget: FSharp.Data, 4.2.3"
#r "nuget: Plotly.NET, 2.0.0-preview.10"
#r "nuget: Plotly.NET.ImageExport, 2.0.0-preview.10"

open Plotly.NET
open Plotly.NET.ImageExport
open System
open FSharp.Data

[<Literal>]
let PATH = "https://data.cdc.gov/resource/9mfq-cb36.json"
type Covid = JsonProvider<PATH>
let co = Covid.Load(PATH)

let stateName = "NYC" //State name here.
let stateChosen =
    co
    |> Array.filter(fun state -> state.State = stateName)

let deaths =
    stateChosen
    |> Array.sumBy(fun state -> state.TotDeath)

let confirmed =
    stateChosen
    |> Array.choose(fun state -> state.ConfCases)
    |> Array.sum
    |> int
  
let totalCases =
    stateChosen
    |> Array.sumBy(fun state -> state.TotCases)

let values =
    [deaths; confirmed; totalCases]

let labels = ["Deaths"; "Confirmed"; "Total Cases"]

let pie =
    Chart.Pie(values, labels) 
    |> Chart.show 
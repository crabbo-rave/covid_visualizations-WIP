(* Code that generates the bar chart for US state deaths *)

#r "nuget: FSharp.Data, 4.2.3"
#r "nuget: Plotly.NET, 2.0.0-preview.10"
#r "nuget: Plotly.NET.ImageExport, 2.0.0-preview.10"

open Plotly.NET
open Plotly.NET.ImageExport
open System
open FSharp.Data

[<Literal>]
let PATH = "CSV-Data-Files/bar_chart_deaths.csv"
type Covid = CsvProvider<PATH>
let co = Covid.Load(PATH)

let countryNames = 
    co
    |> Seq.map (fun row -> row.State)
    |> Seq.distinct
let countryDeaths countryName = 
    co 
    |> Seq.filter (fun row -> row.State = countryName) 
    |> Seq.sumBy (fun row -> row.TotDeath)
let deaths =
    countryNames
    |> Seq.map countryDeaths
let column = Chart.Column(deaths,countryNames) |> Chart.show //used Chart.saveHtmlAs to generate the html code.


(* Code that generates the Map *)

#r "nuget: FSharp.Data, 4.2.3"
#r "nuget: Plotly.NET, 2.0.0-preview.10"
#r "nuget: Plotly.NET.ImageExport, 2.0.0-preview.10"
#r "nuget: Newtonsoft.Json"

open FSharp.Data
open Newtonsoft.Json
open Plotly.NET.LayoutObjects
open Plotly.NET.TraceObjects
open System
open Plotly.NET 

[<Literal>]
let PATH = "CSV-Data-Files/us_vaccinations.csv"
type Covid = CsvProvider<PATH, Schema="Date (date option), FIPS (string option), , , , , Series_Complete_Yes (int option), ">
type Row = Covid.Row
let data = Covid.Load(PATH)

type NewSchema = 
    { FIPS: string;
      Date: DateTime;
      Series_Complete_Yes: int; }
let newRows = 
    data.Rows 
    |> Seq.choose(fun row -> match row.FIPS, row.Date, row.Series_Complete_Yes with 
                                | Some fips, Some date, Some series -> Some( { FIPS = fips; 
                                                                               Date = date; 
                                                                               Series_Complete_Yes = series; }) 
                                | _ -> None)

let geoJson = 
    Http.RequestString "https://raw.githubusercontent.com/plotly/datasets/master/geojson-counties-fips.json"
    |> JsonConvert.DeserializeObject 

let locationsGeoCSV = 
    newRows
    |> Seq.map (fun row -> row.FIPS)
    |> Seq.distinct

let stateVaccine (rows: NewSchema seq) (stateFIPS: string) =
    match (rows |> Seq.take 1000 |> Seq.toList |> Seq.filter (fun x -> x.FIPS = stateFIPS && x.Date = (rows |> Seq.map (fun x -> x.Date) |> Seq.max)) |> Seq.toList) with
    | [] -> 0
    | head::_ -> head.Series_Complete_Yes
    | result -> result |> Seq.sumBy (fun x -> x.Series_Complete_Yes)


let zGeoCSV =
    Seq.map
        (stateVaccine newRows)
        locationsGeoCSV

Chart.ChoroplethMap(
    locations = locationsGeoCSV,
    z = zGeoCSV,
    Locationmode=StyleParam.LocationFormat.GeoJson_Id,
    GeoJson = geoJson,
    FeatureIdKey="id"
)
|> Chart.withGeo(
    Geo.init(
        Scope=StyleParam.GeoScope.NorthAmerica, 
        Projection=GeoProjection.init(StyleParam.GeoProjectionType.AzimuthalEqualArea),
        ShowLand=true,
        
        LandColor = Color.fromString "lightgrey"
    )
)
|> Chart.withSize (800.,800.)
|> Chart.show //used Chart.saveHtmlAs to generate the html code.

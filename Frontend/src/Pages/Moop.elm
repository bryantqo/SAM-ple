module Pages.Moop exposing (Model, Msg, page)

import Html
import Html.Attributes as Attributes
import Html.Events as Events

import Gen.Params.Moop exposing (Params)
import Page
import Request
import Shared
import View exposing (View)

import Json.Decode as D

import Moopy exposing (..)

import Http

import Task
import Process

page : Shared.Model -> Request.With Params -> Page.With Model Msg
page shared req =
    Page.element
        { init = init
        , update = update
        , view = view
        , subscriptions = subscriptions
        }



-- INIT


type alias Model =
    {}


init : ( Model, Cmd Msg )
init =
    ( {}
    , Cmd.batch
        [ initMap ()
        , Process.sleep
            1000
            |> ( Task.perform <| always Load )
        ]
    )


saveShape : Shape -> Cmd Msg
saveShape shape =
  Http.post
    { url = "/api/Foo/AddSpatial"
    , body = Http.jsonBody shape
    , expect = Http.expectWhatever Uploaded
    }

loadShapes : Cmd Msg
loadShapes =
  Http.post
    { url = "/api/Foo/GetAll"
    , body = Http.emptyBody
    , expect = Http.expectJson (Loaded) (D.list D.value)
    }

-- UPDATE

type alias Shape =
    D.Value

type Msg
    = ReplaceMe
    | Uploaded (Result Http.Error ())
    | Load
    | Loaded (Result Http.Error (List D.Value))
    | UploadClick
    | SaveShape Shape


update : Msg -> Model -> ( Model, Cmd Msg )
update msg model =
    case msg of
        Load ->
            ( model, loadShapes)
        Loaded res ->
            case res of
                Ok r ->
                    ( model, setShapes r )
                _ ->
                    ( model, Cmd.none)
        UploadClick ->
            ( model, upload () )
        SaveShape shp ->
            ( model
            , Cmd.batch
                [ saveShape shp 
                , setShapes [ shp ]
                ]
            )
        _ ->
            ( model, Cmd.none )



-- SUBSCRIPTIONS


subscriptions : Model -> Sub Msg
subscriptions model =
    load (\v -> SaveShape v)



-- VIEW


view : Model -> View Msg
view model =
    { title = "SAM-ple Frontend"
    , body = 
        [ Html.div 
            [ Attributes.id "map"
            , Attributes.style "width" "1200px"
            , Attributes.style "height" "700px" 
            , Attributes.style "border" "1px solid black"
            , Attributes.style "border-radius" "3px"
            ] 
            [ Html.text "" 
            ] 
        , Html.hr [] []
        , Html.button
            [ Events.onClick UploadClick
            ]
            [ Html.text "Upload" 
            ]
        ]
    }

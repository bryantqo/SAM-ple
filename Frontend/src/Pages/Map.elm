module Pages.Map exposing (Model, Msg, page)

import Html
import Html.Attributes as Attributes

import Gen.Params.Map exposing (Params)
import Page
import Request
import Shared
import View exposing (View)


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
    ( {}, Cmd.none )



-- UPDATE


type Msg
    = ReplaceMe


update : Msg -> Model -> ( Model, Cmd Msg )
update msg model =
    case msg of
        ReplaceMe ->
            ( model, Cmd.none )



-- SUBSCRIPTIONS


subscriptions : Model -> Sub Msg
subscriptions model =
    Sub.none



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
            [ Html.text "loading" 
            ] 
        , Html.hr [] []
        , Html.button
            [ ]
            [ Html.text "Upload" 
            ]
        ]
    }

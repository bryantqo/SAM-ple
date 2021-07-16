port module Moopy exposing (..)

import Json.Decode as Json
import Shared exposing (Msg)

port initMap : () -> Cmd msg

port setShapes : List Json.Value -> Cmd msg

port upload : () -> Cmd msg

port load : (Json.Value -> msg) -> Sub msg
module Pages.Home_ exposing (view)

import Html
import Html.Attributes as Attributes
import View exposing (View)


view : View msg
view =
    { title = "SAM-ple Frontend"
    , body = 
        [ Html.a 
            [ Attributes.href "/moop"
            ] 
            [ Html.text "Map" 
            ]
        ]
    }

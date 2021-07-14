
INSERT INTO
	pam.model
		(
            key, 
            singular_name, 
            plural_name, 
            display, 
            display_order, 
            model
        )
	VALUES
        (
            'focus-area',
            'Focus Area',
            'Focus Areas',
            true,
            50,
            '{
                "CWPP": {
                    "id": 0,
                    "name": "CWPP",
                    "type": 0,
                    "limit": 50
                },
                "Funding Source": {
                    "id": 1,
                    "name": "Funding Source",
                    "type": 17
                },
                "Scores": {
                    "id": 2,
                    "name": "Scores",
                    "type": 18
                },
                "Region": {
                    "id": 3,
                    "name": "Region",
                    "type": 7
                },
                "Projects": {
                    "id": 4,
                    "name": "Number of Projects within FA",
                    "type": 10,
                    "reference": 1
                },
                "Geometry": {
                    "id": 5,
                    "name": "Geometry",
                    "type": 11
                },
                "Acres": {
                    "id": 6,
                    "name": "Acres",
                    "type": 3
                }
            }'
        );

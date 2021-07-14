
INSERT INTO 
	pam.model
		(key, singular_name, plural_name, display, display_order, model)
	VALUES
		( 'action',
		'Action',
		'Actions',
		true,
		201,
		'{
			"Start/End Date": {
				"id": 21,
				"name": "Start/End Date",
				"type": 14
			},
			"Type": {
				"id": 2,
				"name": "Type",
				"type": 17
			},
			"Acres": {
				"id": 6,
				"name": "Acres",
				"type": 3,
				"min": 0
			},
			"Comments": {
				"id": 14,
				"name": "Comments",
				"type": 1,
				"limit": 256
			},
			"Date": {
				"id": 21,
				"name": "Start/End",
				"type": 14
			},
			"Phase": {
				"id": 22,
				"name": "Phase",
				"type": 7
			},
			"Status": {
				"id": 23,
				"name": "Status",
				"type": 7
			}
		}');

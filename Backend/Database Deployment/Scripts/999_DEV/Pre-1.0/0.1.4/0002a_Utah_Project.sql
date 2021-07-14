
INSERT INTO 
	pam.model
		(key, singular_name, plural_name, display, display_order, model)
	VALUES
		( 'project',
		  'Project',
		  'Projects',
		  true,
		  100,
		  '{
			"County":{
				"id":1,
				"name":"County",
				"type":7
			},
			"FAM Actions":{
				"id":17,
				"name":"FAM Actions",
				"type":10,
				"reference":2
			},
			"Geometry":{
				"id":16,
				"name":"Geometry",
				"type":11
			},
			"FFSL Area":{
				"id":0,
				"name":"FFSL Area",
				"type":7
			},
			"Project Code":{
				"id":3,
				"name":"Project Code",
				"type":0,
				"limit": 12
			},
			"Funding Source":{
				"id": 2,
				"name": "Funding Source",
				"type": 17
			},
			"Project Status":{
				"id":9,
				"name":"Project Status",
				"type":7
			},
			"Acres completed":{
				"id":10,
				"name":"Acres completed",
				"type": 3
			},			
			"Project Category":{
				"id":4,
				"name":"Project Category",
				"type":7
			},
			"Start/End Date":{
				"id": 7,
				"name": "Start/End Date",
				"type": 14
			},
			"Year of Allocation":{
				"id":8,
				"name":"Year of Allocation",
				"type": 13
			},
			"Maintenance required":{
				"id": 14,
				"name": "Maintenance required",
				"type": 16,
				"limit": 75
			},
			"Primary FFSL Contact":{
				"id":6,
				"name":"Primary FFSL Contact",
				"type":0,
				"limit": 30
			},
			"Estimated cost per acres":{
				"id":11,
				"name":"Estimated cost per acres",
				"type":4,
				"limit": 10
			},
			"Biomass utilization (tons)":{
				"id":12,
				"name":"Biomass utilization (tons)",
				"type":3,
				"limit": 10
			},			
			"Name of CWPP or Preparedness Plan":{
				"id":5,
				"name":"Name of CWPP or Preparedness Plan",
				"type":0,
				"limit": 50
			}
		}');
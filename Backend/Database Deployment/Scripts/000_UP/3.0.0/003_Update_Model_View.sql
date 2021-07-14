CREATE OR REPLACE VIEW pam.model AS
(
	SELECT 
		id,
		key,
		singular_name,
		plural_name,
		display,
		display_order,
		COALESCE(merp.model, moby.model, '{}'::jsonb) as model,
		"extensionOf"
	FROM pam_1_0_0.model as moby
	LEFT JOIN
		(
		SELECT modelid, jsonb_object_agg(name, COALESCE(data, '{}') || 
			jsonb_build_object( 
				'id', id, 
				'fieldType', type, 
				'name', name 
			)) as model
		FROM pam_3_0_0.model_fields
		GROUP BY modelid
	) merp
	ON id = merp.modelid
	ORDER BY id
);
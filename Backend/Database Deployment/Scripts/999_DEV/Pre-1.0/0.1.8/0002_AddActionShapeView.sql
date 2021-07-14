-- View: pam.shapes

-- DROP VIEW pam.shapes;

CREATE OR REPLACE VIEW pam.action_shapes AS
 SELECT ps.id AS shapeid,
    o.id,
    o.type,
    o.name,
    (o.fields -> 'Status'::text) ->> 'selected'::text AS status,
    ps.shape
   FROM pam_0_1_7.shapes ps
     LEFT JOIN pam.objects o ON ps.objectid = o.id
	WHERE o.type = 2;


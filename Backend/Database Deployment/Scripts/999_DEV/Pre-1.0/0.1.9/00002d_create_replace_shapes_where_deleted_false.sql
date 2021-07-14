-- View: pam.shapes

--DROP VIEW pam.shapes;

CREATE OR REPLACE VIEW pam.shapes AS
 SELECT ps.id AS shapeid,
    o.id,
    o.type,
    o.name,
    (o.fields -> 'Project Status'::text) ->> 'selected'::text AS status,
    ps.shape
   FROM pam_0_1_7.shapes ps
     LEFT JOIN pam.objects o ON ps.objectid = o.id
   WHERE o.deleted = false;
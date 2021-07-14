﻿CREATE TABLE pam_2_0_0.objects_foo
(
    id integer NOT NULL GENERATED BY DEFAULT AS IDENTITY ( INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 2147483647 CACHE 1 ),
    name character varying(50) COLLATE pg_catalog."default" NOT NULL,
    created timestamp with time zone DEFAULT now(),
    modified timestamp with time zone DEFAULT now(),
    "createdBy" uuid NOT NULL,
    "lastModifiedBy" uuid NOT NULL,
    deleted boolean DEFAULT false,
    fields jsonb DEFAULT '{}'::jsonb
)
WITH (
    OIDS = FALSE
);

CREATE VIEW pam.objects_foo AS 
(
    SELECT * FROM pam_2_0_0.objects_foo 
);


CREATE TABLE pam_2_0_0.shapes_foo
(
    id integer GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    objectID integer,
	fieldID integer,
	shape geometry,
	acres float,
	metadata jsonb
)
WITH (
    OIDS = FALSE
);


CREATE OR REPLACE VIEW pam.shapes_foo AS
 SELECT *
   FROM pam_2_0_0.shapes_foo;
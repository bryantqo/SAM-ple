CREATE TABLE pam_1_0_0.groupACL
(
    id integer GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    groupkey CHARACTER VARYING(255),
    acl jsonb DEFAULT '[]'
)
WITH (
    OIDS = FALSE
);


CREATE OR REPLACE VIEW pam.groupACL as 
	SELECT * FROM pam_1_0_0.groupACL;
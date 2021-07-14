CREATE TABLE pam_0_1_5.users
(
    id uuid PRIMARY KEY,
    name character varying(50) NOT NULL,
    email character varying(250) NOT NULL
)
WITH (
    OIDS = FALSE
);
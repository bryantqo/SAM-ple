CREATE TABLE pam_1_0_0.users
(
    id uuid PRIMARY KEY,
    name character varying(50) NOT NULL,
    email character varying(250) NOT NULL
)
WITH (
    OIDS = FALSE
);
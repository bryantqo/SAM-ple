CREATE TABLE pam_1_0_0.model
(
    id integer GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    key character varying(50) NOT NULL,
    singular_name character varying(50) NOT NULL,
    plural_name character varying(50) NOT NULL,
    display boolean NOT NULL,
    display_order integer NOT NULL,
    model jsonb DEFAULT '{}'
)
WITH (
    OIDS = FALSE
);


CREATE TABLE pam_1_0_0.objects
(
    id integer GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    name character varying(50) NOT NULL,
    type integer NOT NULL,
	created timestamp with time zone DEFAULT NOW(),
	modified timestamp with time zone DEFAULT NOW(),
    "createdBy" uuid NOT NULL,
    "lastModifiedBy" uuid NOT NULL,
    deleted boolean DEFAULT FALSE,
    fields jsonb DEFAULT '{}'
)
WITH (
    OIDS = FALSE
);

CREATE TABLE pam_1_0_0.choices
(
    id integer GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
	objectType integer NOT NULL,
    type integer NOT NULL,
	value character varying(50) NOT NULL
)
WITH (
    OIDS = FALSE
);

CREATE TABLE pam_1_0_0.layouts
(
    id integer GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
	objectType integer NOT NULL,
    type integer NOT NULL,
	layout jsonb DEFAULT '{}'
)
WITH (
    OIDS = FALSE
);
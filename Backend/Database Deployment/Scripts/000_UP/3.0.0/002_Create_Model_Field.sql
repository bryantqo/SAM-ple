CREATE TABLE pam_3_0_0.model_fields
(
    id integer NOT NULL,
    modelid integer NOT NULL,
    type integer NOT NULL,
    name character varying(50) NOT NULL,
    data jsonb,
    CONSTRAINT pk_model_field_id_model PRIMARY KEY (id, modelid),
    CONSTRAINT uq_model_field_name_per_model UNIQUE (name, modelid),
    CONSTRAINT fk_model_field_model_link FOREIGN KEY (modelid)
        REFERENCES pam_1_0_0.model (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
        NOT VALID
)
WITH (
    OIDS = FALSE
);
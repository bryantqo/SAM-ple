CREATE TABLE pam_0_1_10.shapeScore
(
    id integer GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    layer INT REFERENCES pam_0_1_10.scoreLayers (id),
    shape INT REFERENCES pam_0_1_7.shapes (id),
    "band" INT REFERENCES pam_0_1_10.bands (id),
    "score" FLOAT,
    "min" FLOAT,
    "max" FLOAT,
    "avg" FLOAT,
    "buffer" FLOAT,
    "state" INT 
)
WITH (
    OIDS = FALSE
);
-- View: pam.shapes

-- DROP VIEW pam.shapes;

CREATE OR REPLACE VIEW pam.model AS
 SELECT *
   FROM pam_1_0_0.model;

CREATE OR REPLACE VIEW pam.choices AS
 SELECT *
   FROM pam_1_0_0.choices;

CREATE OR REPLACE VIEW pam.bands AS
 SELECT *
   FROM pam_1_0_0.bands;

CREATE OR REPLACE VIEW pam.estimatedscoregrid AS
 SELECT *
   FROM pam_1_0_0.estimatedscoregrid;

CREATE OR REPLACE VIEW pam.layouts AS
 SELECT *
   FROM pam_1_0_0.layouts;

CREATE OR REPLACE VIEW pam.objects AS
 SELECT *
   FROM pam_1_0_0.objects;

CREATE OR REPLACE VIEW pam.scorelayers AS
 SELECT *
   FROM pam_1_0_0.scorelayers;

CREATE OR REPLACE VIEW pam.shapes AS
 SELECT *
   FROM pam_1_0_0.shapes;

CREATE OR REPLACE VIEW pam.shapescore AS
 SELECT *
   FROM pam_1_0_0.shapescore;

CREATE OR REPLACE VIEW pam.shapescoreestimatedparts AS
 SELECT *
   FROM pam_1_0_0.shapescoreestimatedparts;

CREATE OR REPLACE VIEW pam.users AS
 SELECT *
   FROM pam_1_0_0.users;
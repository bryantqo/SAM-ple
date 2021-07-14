CREATE OR REPLACE FUNCTION deleteScoresForObject ( in_objectId integer, in_fieldId integer )
RETURNS VOID AS $$
BEGIN
    DELETE 
    FROM pam_0_1_10.shapeScore 
    WHERE shape in 
        ( 
            SELECT id 
            FROM pam_0_1_7.shapes 
            WHERE objectid = in_objectId
                AND fieldid = in_fieldId
        ); 

    DELETE 
    FROM pam_0_1_10.shapeScoreEstimatedParts 
    WHERE shape in 
        (
            SELECT id 
            FROM pam_0_1_7.shapes 
            WHERE objectid = in_objectId 
            AND fieldid = in_fieldId
        );
END $$ LANGUAGE plpgsql;
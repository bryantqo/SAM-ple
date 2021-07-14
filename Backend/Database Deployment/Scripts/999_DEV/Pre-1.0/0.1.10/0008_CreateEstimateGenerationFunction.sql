CREATE OR REPLACE FUNCTION generateEstimatesForShape ( in_id integer )
RETURNS VOID AS $$	
DECLARE
    target geometry;
BEGIN

/*
    Set the state to estimating
*/
UPDATE pam_0_1_10.shapeScore
SET state = 1
WHERE shape = in_id;



/*
    Grab the target
*/
SELECT shape into target
FROM pam.shapes
WHERE shapeid = in_id;


/*
    Create two tem tables housing the fully within squares and the partially within squares
    TODO: Replace the estimatedscoregrid table with a function taking a shape and area?
*/
CREATE TEMP TABLE fullyWithin AS
SELECT id
FROM pam_0_1_10.estimatedscoregrid
where ST_Within(shape,target);

CREATE TEMP TABLE partiallyWithin AS
SELECT id
FROM pam_0_1_10.estimatedscoregrid 
where ST_Intersects(shape, target);



/*
    Organize some infos
*/
create TEMP TABLE targetScoreIDs AS
select false as "full",partiallyWithin.id from partiallyWithin
except select false,id from fullyWithin
union
select true as "full",id from fullyWithin;



/* Only for debugging

DELETE FROM pam_0_1_10.shapeScoreEstimatedParts
WHERE shape = in_id;

INSERT INTO pam_0_1_10.shapeScoreEstimatedParts
( shape
, fullyWithin
, gridId
)
SELECT 
    in_id,
    "full",
    "id"
FROM targetScoreIDs;

*/



/*
    Insert our scores
    TODO: Get the layerid from above as were assuming 1 layer atm
    TODO: Replace the estimatedscoregrid table with a function taking a shape and area?
*/

UPDATE pam_0_1_10.shapeScore
set
    "score" = grid.score,
    "min" = grid.min,
    "max" = grid.max,
    "avg" = grid.avg,
    "state" = grid.state
FROM
(
	SELECT 
		layer as gridLayer, 
		SUM(score) as "score", 
		MIN(min) as "min", 
		MAX(max) as "max", 
		AVG(avg) as "avg", 
		2 as "state" --Estimated
	FROM pam_0_1_10.estimatedscoregrid as grid
	JOIN targetScoreIDs as scores
	ON grid.id = scores.id
	GROUP BY grid.layer
) as grid
WHERE shape = in_id
AND layer = grid.gridLayer;


UPDATE pam_0_1_10.shapeScore
SET state = 2, avg = 0
WHERE shape = in_id AND state = 1;

/*
    Cleanup what we created
*/
DROP TABLE fullyWithin;
DROP TABLE partiallyWithin;
DROP TABLE targetScoreIDs;
		
END $$ LANGUAGE plpgsql;
CREATE OR REPLACE FUNCTION getScoresByOID ( in_id integer )
RETURNS TABLE ( id integer, score float, buffer float, avg float, min float, max float, layer int, band int, labelConfig jsonb ) AS $$
DECLARE
	normalized float;
BEGIN
	SELECT score.avg / layer.normalization into normalized
	FROM pam_0_1_10.shapescore as score
	JOIN pam_0_1_10.scorelayers as layer
	ON score.layer = layer.id
	JOIN pam.shapes as shape
	ON score.shape = shape.shapeid
	WHERE shape.id = in_id;
	
	RETURN QUERY
	--SELECT 0,normalized,1,0.0::float;
	
	SELECT 
		score.id,
		case when score.avg <> 0 then band.value + (( normalized )::numeric - lower(band.range)) / (upper(band.range) - lower(band.range))
		ELSE 0 END  as score,
		0.0::float,
		score.avg,
		score.min,
		score.max,
		score.layer,
		band.id,
		band."labelConfig"
		--score.shape,
		--score.min,
		--score.max,
		--score.avg,
		
	FROM pam_0_1_10.shapescore as score
	LEFT JOIN pam_0_1_10.bands as band
	ON ( normalized )::numeric <@ band.range
	JOIN pam.shapes as shape
	ON score.shape = shape.shapeid
	WHERE shape.id = in_id
	AND state > 1;

END $$  LANGUAGE plpgsql;
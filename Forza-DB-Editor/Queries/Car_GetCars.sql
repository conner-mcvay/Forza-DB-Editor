DROP VIEW IF EXISTS uvwCars;
CREATE VIEW IF NOT EXISTS uvwCars AS
SELECT a.Id, Year, ifnull(b.IconPathBase, 'Unknown') 'Make',
	CASE WHEN ModelShort NOT LIKE '_&%' THEN a.DisplayName
	ELSE MediaName END 'Model',
	FrontWheelDiameterIN, FrontTireAspect,ModelFrontTrackOuter, RearWheelDiameterIN, RearTireAspect, ModelRearTrackOuter
FROM Data_Car a
	LEFT JOIN List_CarMake b on a.MakeID = b.ID
	LEFT JOIN Data_CarBody c on (CAST(a.Id AS VARCHAR) || '000') = c.Id;

SELECT Id, Year, Make, Model, 
	Year || ' ' || Make || ' ' || Model 'FullName', 
	FrontWheelDiameterIN, FrontTireAspect, CAST(ModelFrontTrackOuter as NUMERIC) 'ModelFrontTrackOuter',
	RearWheelDiameterIN, RearTireAspect, CAST(ModelRearTrackOuter AS NUMERIC) 'ModelRearTrackOuter'
FROM uvwCars
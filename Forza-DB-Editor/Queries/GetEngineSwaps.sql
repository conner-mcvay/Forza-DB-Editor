DROP VIEW IF EXISTS uvwEngineSwaps;
CREATE VIEW IF NOT EXISTS uvwEngineSwaps AS
SELECT b.Id 'UpgradeEngineID', a.Id 'CarID', Year || ' ' || Make || ' ' || Model 'CarName', CAST(c.EngineID AS NUMERIC) 'EngineID', 
	CASE
    WHEN TRIM(c.EngineName) IS NOT NULL AND TRIM(c.EngineName) <> '' THEN c.EngineName
    ELSE c.MediaName
END AS EngineName,
b.Level, b.IsStock, b.Price
FROM uvwCars a
	INNER JOIN List_UpgradeEngine b on a.Id = b.Ordinal
	INNER JOIN Data_Engine c on b.EngineID = c.EngineID;

SELECT * FROM uvwEngineSwaps;
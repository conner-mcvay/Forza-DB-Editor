SELECT EngineID, CASE
    WHEN TRIM(c.EngineName) IS NOT NULL AND TRIM(c.EngineName) <> '' THEN c.EngineName
    ELSE c.MediaName
END AS EngineName
FROM Data_Engine c

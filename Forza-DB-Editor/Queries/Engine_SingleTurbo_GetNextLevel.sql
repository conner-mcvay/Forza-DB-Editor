SELECT MAX(Level) + 1 'NextLevel', ManufacturerID
	WHERE EngineID = @EngineID
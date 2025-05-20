SELECT MAX(Level) + 1 'NextLevel', ManufacturerID
FROM List_UpgradeEngineTurboSingle
WHERE EngineID = @EngineID

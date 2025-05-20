SELECT EngineID, ManufacturerId, Level, Price, MinScale, MaxScale, PowerMinScale, PowerMaxScale, RobScale
FROM List_UpgradeEngineTurboSingle
WHERE EngineID = @EngineID
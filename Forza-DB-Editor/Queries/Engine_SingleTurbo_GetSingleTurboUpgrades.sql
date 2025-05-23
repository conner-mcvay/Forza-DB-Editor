SELECT EngineID, ManufacturerId, Level, Price, MinScale, PowerMinScale, MaxScale, PowerMaxScale, RobScale
FROM List_UpgradeEngineTurboSingle
WHERE EngineID = @EngineID
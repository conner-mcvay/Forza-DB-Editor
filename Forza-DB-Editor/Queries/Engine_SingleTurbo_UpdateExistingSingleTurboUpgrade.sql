UPDATE List_UpgradeEngineTurboSingle
SET Price = @Price,
    MaxScale = @MaxScale,
    PowerMaxScale = @PowerMaxScale,
    MinScale = @MinScale,
    PowerMinScale = @PowerMinScale,
    RobScale = @RobScale
WHERE EngineID = @EngineID AND Level = @Level;
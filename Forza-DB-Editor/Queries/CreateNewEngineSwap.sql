INSERT INTO List_UpgradeEngine(
    Id, Ordinal, Level, EngineID, IsStock, ManufacturerID, Price, 
    MassDiff, WeightDistDiff, DragScale, WindInstabilityScale
)
VALUES (
    @UpgradeEngineID, @CarID, @Level, @EngineID, @IsStock, @ManufacturerID, @Price,
    0.0, 0.0, 1.0, 1.0
);
WITH step1 AS (
    SELECT 
        CASE
            WHEN length(Ordinal) = 3 THEN substr(Id, 1, 3)
            WHEN length(Ordinal) = 4 THEN substr(ID, 1, 4)
            WHEN length(Ordinal) = 5 THEN substr(ID, 1, 5)
        END AS substr, 
        Id, 
        Ordinal
    FROM List_UpgradeEngine
),
step2 AS (
    SELECT *, ROW_NUMBER() OVER (PARTITION BY substr ORDER BY Id) AS rownumber
    FROM step1
),
step3 AS (
    SELECT *, 
           CASE 
               WHEN rownumber <= 9 THEN substr || '00' || rownumber
               ELSE substr || '0' || rownumber
           END AS newID
    FROM step2
)
SELECT 
    CASE
        WHEN newID = Id THEN MAX(newID) + 1
        ELSE MAX(newID)
    END AS UpgradeEngineID
FROM step3
WHERE Ordinal = @CarID;
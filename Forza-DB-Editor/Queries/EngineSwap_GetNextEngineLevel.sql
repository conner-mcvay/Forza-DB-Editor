WITH step1 AS (
SELECT CASE
	WHEN length(Ordinal) = 3 THEN substr(Id, 1, 3)
	WHEN length(Ordinal) = 4 THEN substr(ID, 1, 4)
	WHEN length(Ordinal) = 5 THEN substr(ID, 1, 5)
END 'substr', Id, Ordinal, Level
FROM List_UpgradeEngine
),
step2 AS (
SELECT *, ROW_NUMBER() OVER (PARTITION BY substr) AS rownumber
from step1 
ORDER BY Id
),
step3 as (
SELECT *, CASE 
	WHEN rownumber <= 9 THEN substr || '00' || (rownumber)
	WHEN rownumber >= 10 THEN substr || '0' || (rownumber)
END newID
FROM step2
)

SELECT 
    COALESCE(MAX(Level), 0) + 1 AS NextLevel
FROM step3
WHERE Ordinal = @CarID

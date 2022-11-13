
-- Calculates age
CREATE FUNCTION [dbo].GetAge (@birthDate DATE) RETURNS INTEGER AS
BEGIN
DECLARE @current DATE = GETDATE();
RETURN (DATEDIFF(YEAR, @birthDate, @current) -
        CASE
            WHEN MONTH(@birthDate) < MONTH(@current)
			THEN 0
            WHEN MONTH(@birthDate) > MONTH(@current)
            THEN 1
            WHEN DAY(@birthDate) > DAY(@current)
            THEN 1
            ELSE 0
        END)
END
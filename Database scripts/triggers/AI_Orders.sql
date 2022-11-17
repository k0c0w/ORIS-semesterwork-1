CREATE TRIGGER AI_Orders_UpdateCarStatus ON Orders 
AFTER INSERT AS
BEGIN
	IF(EXISTS(SELECT Car FROM CarPark WHERE Car in (select Car from inserted) and IsBusy = 0))
		begin
			UPDATE CarPark SET IsBusy = 1 WHERE Car in (select Car from Inserted);
		end
		else
		begin
			UPDATE CarPark SET IsReserved = 1 WHERE Car in (select Car from Inserted);
		end
END
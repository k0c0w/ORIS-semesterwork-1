CREATE FUNCTION GetFreeCar (@CarModel INTEGER, @CityId INTEGER, @SubscriptionStart DATE) returns INTEGER
AS
BEGIN
	declare @Car integer;
	SELECT TOP 1 @Car = Id FROM CarPark WHERE City = @CityId AND IsBusy = 0;
	if(@Car is null)
	BEGIN
		select TOP 1 @Car = CarPark.Id from CarPark 
		JOIN Orders ON CarPark.Id = CarId where IsBusy = 0 or IsBusy = 1 and IsReserved = 0 and SubscriptionEnd < @SubscriptionStart;

		if(@Car is null)
		begin
			return -1;
		end
	END

	return @Car;
END
CREATE TRIGGER AU_PersonalInfo ON PersonalInfo 
AFTER UPDATE AS
BEGIN
	UPDATE Users SET IsVerified = 1
	WHERE Users.Id in (SELECT UserId FROM inserted WHERE 
		FirstName is not null and MiddleName is not null and SecondName is not null
		and TelephoneNumber is not null and DriverLicense is not null and Passport is not null
		and CardNumber is not null and CardOwner is not null and CVC is not null)			

	UPDATE  Users SET IsVerified = 0 WHERE Users.Id in (SELECT UserId FROM inserted WHERE 
		FirstName is null or MiddleName is null or SecondName is null
		or TelephoneNumber is null or DriverLicense is null or Passport is null
		or CardNumber is null or CardOwner is null or CVC is null)
END
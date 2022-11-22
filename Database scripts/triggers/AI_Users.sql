CREATE TRIGGER AI_Users ON Users AS
AFTER INSERT
BEGIN
INSERT INTO PersonalInfo(UserId, FirstName)
	SELECT Id, FirstName FROM inserted
END

CREATE TABLE CompanyDocuments (
	Id INTEGER IDENTITY(1,1) PRIMARY KEY,
	Name VARCHAR(100),
	--descriprion
)

CREATE TABLE QuestionsStatus(
	Status INT NOT NULL PRIMARY KEY,
	StatusName VARCHAR(30) NOT NULL
)

CREATE TABLE CustomersQuestions (
	Id INTEGER IDENTITY(1,1),
	UserId INTEGER,
	Question varchar(5000) NOT NULL,
	ResponseEmail varchar(254) NOT NULL,
	StatusCode INT NOT NULL,
	CONSTRAINT fk_statusCode FOREIGN KEY(StatusCode) REFERENCES  QuestionsStatus(Status),
)



CREATE TABLE Users (
	Id INTEGER NOT NULL IDENTITY(1,1),
	Email VARCHAR(254) NOT NULL,
	Password VARCHAR(50) NOT NULL,
	BirthDate Date NOT NULL,
	DataProcessingAgreement BIT DEFAULT(0),
	IsVerified BIT DEFAULT(0),

	CONSTRAINT UniqueEmail UNIQUE(Email),
	CONSTRAINT CheckAge CHECK([dbo].GetAge(BirthDate) >= 18),
	CONSTRAINT pk_Id PRIMARY KEY(Id),
)

create TABLE PersonalInfo (
	Id INTEGER IDENTITY(1, 1),
	UserId INTEGER NOT NULL UNIQUE,
	FirstName VARCHAR(50),
	MiddleName VARCHAR(50),
	SecondName VARCHAR(50),
	TelephoneNumber INT,
	DriverLicense INT,
	Passport INT,
	CardNumber BIGINT,
	CardOwner VARCHAR(30),
	CVC INT,

	CONSTRAINT cardNumberValidation CHECK(CardNumber BETWEEN 999999999999999 AND 10000000000000000),
	CONSTRAINT cardCVCValidation CHECK(CVC BETWEEN 99 AND 1000),
	CONSTRAINT passportNumberValidation CHECK(Passport BETWEEN 999999999 AND 10000000000),
	CONSTRAINT telephoneNumberValidation CHECK(TelephoneNumber BETWEEN 999999999 AND 10000000000),
	CONSTRAINT fk_UserId Foreign Key(UserId) REFERENCES Users(Id), 
	CONSTRAINT pk_personalInfoId Primary Key(Id), 
)


CREATE TABLE AvailableCities (
	Id INT IDENTITY(1,1)  NOT NULL PRIMARY KEY,
	City VARCHAR(50),
)

CREATE TABLE Cars (
  Id INTEGER IDENTITY(1,1),
  Brand varchar(50) NOT NULL,
  Model varchar(50) NOT NULL,
  RegisterSign varchar(9) NOT NULL UNIQUE,
  Description varchar(500) DEFAULT(''),

  CONSTRAINT pk_carId PRIMARY KEY(Id),
  CONSTRAINT registerSignValidation CHECK(RegisterSign LIKE '[ÀÂÅÊÌÍÎÐÑÒÓÕ][0-9][0-9][0-9][ÀÂÅÊÌÍÎÐÑÒÓÕ][ÀÂÅÊÌÍÎÐÑÒÓÕ]' AND  RegisterSign NOT LIKE '[ÀÂÅÊÌÍÎÐÑÒÓÕ]000[ÀÂÅÊÌÍÎÐÑÒÓÕ][ÀÂÅÊÌÍÎÐÑÒÓÕ]')
)

CREATE TABLE CarPark(
	Id INTEGER IDENTITY(1,1) Primary KEY,
	Car INTEGER NOT NULL CHECK(Car > 0),
	City INTEGER NOT NULL CHECK (City > 0),
	IsBusy BIT DEFAULT(0),

	CONSTRAINT fk_carModel FOREIGN KEY(Car) REFERENCES Cars(Id),
	CONSTRAINT fk_carCity FOREIGN KEY(City) REFERENCES AvailableCities(Id),
)


CREATE TABLE Tariffs(
	Id INTEGER IDENTITY(1,1) UNIQUE,
	Name varchar(30) NOT NULL,
	Description  varchar(800) NOT NULL,
	Price DECIMAL(10,2) NOT NULL CHECK(Price > 0),
	Car INTEGER CHECK(Car > 0)

	CONSTRAINT pk_tariff PRIMARY KEY (Name, Car),
	CONSTRAINT fk_tariffCar FOREIGN KEY(Car) REFERENCES Cars(Id),
)

CREATE TABLE Subscription (
	Id INTEGER IDENTITY(1,1),
	Tariff INTEGER NOT NULL,
	Car INTEGER NOT NULL,
	CarId INTEGER,
	SubStart DATE NOT NULL,	
	SubEnd DATE NOT NULL,
					/*òîæå ññûëêè? ïðåäïîëàãàåòñÿ ÷òî ïîëÿ êîïèðóþòñÿ ñ ïîëåé Orders, 
					åñòü âîðêåð êîòîðûé çàïóñêàåòñÿ íî÷üþ è îáíîâëÿåò ïîëÿ åñëè ñðîê èñòåê*/
	IsActive BIT,

	CONSTRAINT pk_subcriptionId PRIMARY KEY(Id),
	CONSTRAINT fk_tariff FOREIGN KEY(Tariff) REFERENCES Tariffs(Id),
	CONSTRAINT fk_carId FOREIGN KEY(CarId) REFERENCES CarPark(Id),
)


CREATE TABLE Orders (
	Id INTEGER IDENTITY(1,1),
	UserId INTEGER NOT NULL CHECK(UserId > 0),
	Tariff INTEGER NOT NULL CHECK(Tariff > 0),
	SubscriptionStart DATE NOT NULL,
	SubscriptionEnd DATE NOT NULL,
	SubscriptionId INTEGER NOT NULL,
	CityId INTEGER NOT NULL CHECK(CityId > 0),
	IsCancled BIT DEFAULT(0),

	CHECK(SubscriptionStart <= SubscriptionEnd),
	CONSTRAINT fk_ordersUserId FOREIGN KEY (UserId)  REFERENCES Users(Id),
	CONSTRAINT fk_cityId FOREIGN KEY (CityId)  REFERENCES AvailableCities(Id),
	CONSTRAINT fk_tariffId FOREIGN KEY (Tariff)  REFERENCES Tariffs(Id),
	CONSTRAINT fk_subscriptionId FOREIGN KEY (SubscriptionId)  REFERENCES Subscription(Id),
	CONSTRAINT pk_orderId PRIMARY KEY(Id),
)


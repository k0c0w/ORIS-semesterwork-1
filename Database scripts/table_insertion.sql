INSERT INTO QuestionsStatus VALUES (1, 'Открыт'), (2, 'В работе'), (3, 'Закрыт');
GO

INSERT INTO AvailableCities VALUES ('Казань'), ('Москва'), ('Санкт-Петербург');
GO

INSERT INTO Cars (Brand, Model, Description, ImageSource) 
	VALUES ('Газ', '24', DEFAULT, null),
		   ('Volkswagen', 'Passat B8', DEFAULT, null),
		   ('Nissan', 'Laurel', DEFAULT, null),
		   ('Nissan', 'Skyline', DEFAULT, null),
		   ('Hundai', 'Sonata', DEFAULT, null),
		   ('Mazda', 'MX-5', DEFAULT, null),
		   ('Mazda', 'RX-7', DEFAULT, null),
		   ('Toyota', 'GT 86', DEFAULT, null),
		   ('Hundai', 'Cretta', DEFAULT, null),
		   ('Audi', 'Q3', DEFAULT, null),
		   ('Toyota', 'Highlander', DEFAULT, null),
		   ('Hundai', 'H1', DEFAULT, null),
		   ('Mercedes-Benz', 'Vito 3', DEFAULT, null)
GO

INSERT INTO Tariffs VALUES ('Everyday', ' ', 9500, 6),
						   ('Everyday', ' ', 10000, 7),
						   ('Everyday', ' ', 7000, 9),
						   ('Everyday', ' ', 8000, 12),
						   ('Everyday', ' ', 6500, 10),
						   ('Whatever You Want', ' ', 6000, 8),
						   ('Whatever You Want', ' ', 4000, 1),
						   ('Whatever You Want', ' ', 6550, 2),
						   ('Whatever You Want', ' ', 6000, 4),
						   ('Whatever You Want', ' ', 6000, 3),
						   ('Whatever You Want', ' ', 8000, 12),
						   ('Whatever You Want', ' ', 7700, 13),
						   ('Travel', ' ', 6700, 5),
						   ('Travel', ' ', 7000, 2),
						   ('Travel', ' ', 7550, 11),
						   ('Travel', ' ', 5700, 13);
GO